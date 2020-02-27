using System;
using System.Runtime.InteropServices;
using EasyHook;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

public unsafe struct ProduceAbstractScope : IDisposable
{
    GCHandle constructorHandle;
    GCHandle produceHandle;
    GCHandle getTypeHandle;
    readonly IntPtr originalProduce;
    readonly UnityType* type;
    readonly LocalHook transferHook;

    public ProduceAbstractScope(PdbService service, UnityType* type)
        : this()
    {
        if (service == null)
            throw new ArgumentNullException(nameof(service));

        if (type == null)
            throw new ArgumentNullException(nameof(type));

        if (!type->IsAbstract)
            throw new ArgumentException("Type must be abstract.", nameof(type));

        // In order to generate type trees for abstract types, we need to hook
        // VirtualRedirectTransfer to call the appropriate Transfer method on the type.
        var iter = this.type = type;
        while (iter != null && !TryHookTransfer(service, iter->GetName(), out transferHook))
            iter = iter->BaseClass;

        if (transferHook == null)
            throw new ArgumentException("Couldn't locate a ::Transfer method for type.", nameof(type));

        // We also need to locate a constructor that we can call, as not all of the
        // abstract types actually have one defined.
        iter = type;
        ObjectConstructorDelegate ctor = null;
        while (iter != null && !TryFindConstructor(service, iter->GetName(), out ctor))
            iter = iter->BaseClass;

        if (ctor == null)
            throw new ArgumentException("Couldn't locate a constructor for type.", nameof(type));

        constructorHandle = GCHandle.Alloc(ctor, GCHandleType.Pinned);

        // We need to override the 'produce helper' so that attempting to produce
        // the type will no longer result in an error. Additionally, we override the
        // GetTypeVirtualInternal method to avoid errors on types without their own
        // constructors.
        var getType   = CreateGetTypeDelegate(type->RuntimeTypeIndex);
        getTypeHandle = GCHandle.Alloc(getType, GCHandleType.Pinned);
        var produce   = CreateProduceHelper(
            type->ByteSize,
            type->RuntimeTypeIndex,
            Marshal.GetFunctionPointerForDelegate(getType),
            ctor
        );

        // Pin the produce method to avoid GC and assign it to the type.
        // Also set IsAbstract to false to allow the type to be produced.
        produceHandle       = GCHandle.Alloc(produce, GCHandleType.Pinned);
        originalProduce     = type->ProduceHelper;
        type->ProduceHelper = Marshal.GetFunctionPointerForDelegate(produce);
        type->IsAbstract    = false;
    }

    bool TryHookTransfer(PdbService service, string typeName, out LocalHook hook)
    {
        if (service.TryGetAddressForSymbol($"?VirtualRedirectTransfer@{typeName}@@UEAAXAEAVGenerateTypeTreeTransfer@@@Z", out var original) &&
            service.TryGetAddressForSymbol($"??$Transfer@VGenerateTypeTreeTransfer@@@{typeName}@@IEAAXAEAVGenerateTypeTreeTransfer@@@Z", out var transfer))
        {
            hook = LocalHook.CreateUnmanaged(original, transfer, IntPtr.Zero);
            hook.ThreadACL.SetInclusiveACL(new[] { 0 });
            return true;
        }

        hook = null;
        return false;
    }

    bool TryFindConstructor(PdbService service, string typeName, out ObjectConstructorDelegate ctor)
    {
        return service.TryGetDelegateForSymbol(
            $"??0{typeName}@@QEAA@UMemLabelId@@W4ObjectCreationMode@@@Z",
            out ctor
        );
    }

    GetTypeVirtualInternalDelegate CreateGetTypeDelegate(uint typeIndex)
    {
        return (in NativeObject obj) => ref UnityType.RuntimeTypes[typeIndex];
    }

    ProduceHelperDelegate CreateProduceHelper(int objectSize, uint typeIndex, IntPtr getType, ObjectConstructorDelegate ctor)
    {
        return (label, mode) =>
        {
            var alloc  = (NativeObject*)UnsafeUtility.Malloc(objectSize, 8, Allocator.Persistent);
            var vtable = (ObjectMethodTable*)UnsafeUtility.Malloc(sizeof(ObjectMethodTable), 8, Allocator.Persistent);

            // Call the C++ constructor
            ctor.Invoke(alloc, label, mode);

            // Copy the original virtual method table and override GetTypeVirtualInternal
            // to call a custom method that returns the correct runtime type.
            *vtable                        = *alloc->MethodTable;
            vtable->GetTypeVirtualInternal = getType;
            alloc->MethodTable             = vtable;
            alloc->RuntimeTypeIndex        = typeIndex;
            return alloc;
        };
    }

    public void Dispose()
    {
        // Dispose of the EasyHook handle.
        transferHook.Dispose();

        // Dispose of our GC handles, this will allow the delegates to be collected.
        produceHandle.Free();
        getTypeHandle.Free();
        constructorHandle.Free();

        // Restore the original produce helper.
        type->ProduceHelper = originalProduce;

        // Restore 'abstractness' of the type.
        type->IsAbstract = true;
    }

    delegate ref readonly UnityType GetTypeVirtualInternalDelegate(in NativeObject obj);

    delegate NativeObject* ProduceHelperDelegate(MemoryLabel label, ObjectCreationMode mode);

    delegate void ObjectConstructorDelegate(NativeObject* self, MemoryLabel label, ObjectCreationMode mode);
}

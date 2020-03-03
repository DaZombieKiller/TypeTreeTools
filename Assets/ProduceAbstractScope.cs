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
    readonly LocalHook transferHook;

    public ProduceAbstractScope(Rtti* type)
        : this()
    {
        if (type == null)
            throw new ArgumentNullException(nameof(type));

        if (!type->IsAbstract)
            throw new ArgumentException("Type must be abstract.", nameof(type));

        // In order to generate type trees for abstract types, we need to hook
        // VirtualRedirectTransfer to call the appropriate Transfer method on the type.
        for (var iter = type; iter != null; iter = iter->BaseClass)
        {
            if (TryHookTransfer(iter->Name, out transferHook))
                break;
        }

        if (transferHook == null)
            throw new ArgumentException("Couldn't locate a ::Transfer method for type.", nameof(type));

        // We also need to locate a constructor that we can call, as not all of the
        // abstract types actually have one defined.
        ObjectConstructorDelegate ctor = null;
        for (var iter = type; iter != null; iter = iter->BaseClass)
        {
            if (TryFindConstructor(iter->Name, out ctor))
                break;
        }

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
        produceHandle    = GCHandle.Alloc(produce, GCHandleType.Pinned);
        originalProduce  = type->Producer.Pointer;
        type->Producer   = produce;
        type->IsAbstract = false;
    }

    bool TryHookTransfer(string typeName, out LocalHook hook)
    {
        if (ProgramDatabase.TryGetAddressForSymbol($"?VirtualRedirectTransfer@{typeName}@@UEAAXAEAVGenerateTypeTreeTransfer@@@Z", out var original) &&
            ProgramDatabase.TryGetAddressForSymbol($"??$Transfer@VGenerateTypeTreeTransfer@@@{typeName}@@IEAAXAEAVGenerateTypeTreeTransfer@@@Z", out var transfer))
        {
            LocalHook.Release();
            hook = LocalHook.CreateUnmanaged(original, transfer, IntPtr.Zero);
            hook.ThreadACL.SetInclusiveACL(new[] { 0 });
            return true;
        }

        hook = null;
        return false;
    }

    bool TryFindConstructor(string typeName, out ObjectConstructorDelegate ctor)
    {
        return ProgramDatabase.TryGetDelegateForSymbol(
            $"??0{typeName}@@QEAA@UMemLabelId@@W4ObjectCreationMode@@@Z",
            out ctor
        );
    }

    NativeObject.VirtualMethodTable.GetTypeVirtualInternalDelegate CreateGetTypeDelegate(uint typeIndex)
    {
        return (in NativeObject obj) => ref Rtti.RuntimeTypes[typeIndex];
    }

    ObjectProducerDelegate CreateProduceHelper(int objectSize, uint typeIndex, IntPtr getType, ObjectConstructorDelegate ctor)
    {
        return (label, mode) =>
        {
            var vtable = (NativeObject.VirtualMethodTable*)UnsafeUtility.Malloc(sizeof(NativeObject.VirtualMethodTable), 8, Allocator.Persistent);
            var alloc  = (NativeObject*)UnsafeUtility.Malloc(objectSize, 8, Allocator.Persistent);

            // Call the C++ constructor
            ctor.Invoke(alloc, label, mode);

            // Copy the original virtual method table and override GetTypeVirtualInternal
            // to call a custom method that returns the correct runtime type.
            *vtable                        = *alloc->MethodTable;
            vtable->GetTypeVirtualInternal = getType;

            // Assign the new method table and type index to the object.
            alloc->MethodTable     = vtable;
            alloc->CachedTypeIndex = typeIndex;
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
    }
}

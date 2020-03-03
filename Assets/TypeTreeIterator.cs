using System.Runtime.InteropServices;

public unsafe partial struct TypeTreeIterator
{
    readonly void* unknown;
    readonly void* treeData;
    readonly ulong nodeIndex;

    public TypeTreeIterator(void* treeData)
    {
        unknown       = treeData;
        this.treeData = treeData;
        nodeIndex     = 0;
    }

    public string Name => Marshal.PtrToStringAnsi(NameGetter(in this));

    public string Type => Marshal.PtrToStringAnsi(TypeGetter(in this));

#if UNITY_EDITOR
    public uint ByteOffset => ByteOffsetGetter(in this);

    public bool HasConstantSize => HasConstantSizeGetter(in this);
#endif

    public bool IsNull => treeData == null;

    public ref readonly TypeTreeNode Node => ref *GetNode(in this);

    public ref readonly TypeTreeIterator GetNext()
    {
        var copy = this;
        return ref *Next(ref copy);
    }

    public ref readonly TypeTreeIterator GetLast()
    {
        var copy = this;
        return ref *Last(ref copy);
    }

    public ref readonly TypeTreeIterator GetFather()
    {
        var copy = this;
        return ref *Father(ref copy);
    }

    public ref readonly TypeTreeIterator GetChildren()
    {
        var copy = this;
        return ref *Children(ref copy);
    }
}

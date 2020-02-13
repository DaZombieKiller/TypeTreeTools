public struct MemoryLabel
{
#if DEBUG
# pragma warning disable IDE0051 // Remove unused private members
    unsafe readonly int* unk;
# pragma warning restore IDE0051
#endif
    public int ID;
}

using System;

public unsafe readonly struct RuntimeTypeArray
{
    public readonly uint Length;

    public readonly Rtti* FlexibleData;

    public RuntimeTypeEnumerator GetEnumerator()
    {
        fixed (RuntimeTypeArray* array = &this)
            return new RuntimeTypeEnumerator(array);
    }

    public ref Rtti this[long index]
    {
        get
        {
            fixed (Rtti** data = &FlexibleData)
                return ref *data[index];
        }
    }

    public struct RuntimeTypeEnumerator
    {
        readonly RuntimeTypeArray* array;

        long index;

        public RuntimeTypeEnumerator(RuntimeTypeArray* array)
        {
            this.array = array;
            index      = -1;
        }

        public ref Rtti Current
        {
            get
            {
                if (index < 0)
                    throw new InvalidOperationException();

                return ref (*array)[index];
            }
        }

        public bool MoveNext()
        {
            return ++index < array->Length;
        }
    }
}

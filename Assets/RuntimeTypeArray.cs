using System;

public unsafe readonly struct RuntimeTypeArray
{
    public readonly IntPtr Length;

    public readonly UnityType* FlexibleData;

    public RuntimeTypeEnumerator GetEnumerator()
    {
        fixed (RuntimeTypeArray* array = &this)
            return new RuntimeTypeEnumerator(array);
    }

    public ref UnityType this[long index]
    {
        get
        {
            fixed (UnityType** data = &FlexibleData)
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

        public ref UnityType Current
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
            return ++index < array->Length.ToInt64();
        }
    }
}

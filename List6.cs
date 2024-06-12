using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using System;
using Unity.Mathematics;

namespace ZhCollection
{
    public struct List6<T> where T : unmanaged
    {
        public const int Size = 6;
        T item1, item2, item3, item4, item5, item6;

        public int Length { get; private set; }

        public void Add(T item)
        {
            if (Length == Size)
                throw new System.IndexOutOfRangeException();

            ChangeValue(Length, item);
            Length++;
        }

        public bool RemoveAt(int index)
        {
            if (index >= Length)
                throw new System.IndexOutOfRangeException();

            for (int i = index; i < Length - 1; i++)
                ChangeValue(i, this[i + 1]);

            Length--;
            return Length == 0;
        }

        public readonly T this[int index]
        {
            get
            {
                if (index >= Length)
                    throw new System.IndexOutOfRangeException();

                return index switch
                {
                    0 => item1,
                    1 => item2,
                    2 => item3,
                    3 => item4,
                    4 => item5,
                    5 => item6,
                    _ => default,
                };
            }
        }

        public void ChangeValueAt(int index, T value)
        {
            if (index >= Length)
                throw new System.IndexOutOfRangeException();
            ChangeValue(index, value);
        }

        void ChangeValue(int index, T value)
        {
            if (index >= Size)
                throw new System.IndexOutOfRangeException();

            switch (index)
            {
                case 0:
                    item1 = value;
                    break;
                case 1:
                    item2 = value;
                    break;
                case 2:
                    item3 = value;
                    break;
                case 3:
                    item4 = value;
                    break;
                case 4:
                    item5 = value;
                    break;
                case 5:
                    item6 = value;
                    break;
            };
        }
    }
}
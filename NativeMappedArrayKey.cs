using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using System;
using Unity.Mathematics;

namespace ZhCollection
{
    public readonly struct NMAKey<TKey> : IEquatable<NMAKey<TKey>> where TKey : unmanaged, IEquatable<TKey>
    {
        public readonly int chunkIndex;
        public readonly TKey key;

        public NMAKey(TKey key, int chunkIndex)
        {
            this.key = key;
            this.chunkIndex = chunkIndex;
        }

        public bool Equals(NMAKey<TKey> other)
        {
            return chunkIndex.Equals(other.chunkIndex) && key.Equals(other.key);
        }
    }
}
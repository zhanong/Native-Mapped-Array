using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using System;
using Unity.Mathematics;


namespace ZhCollection
{
    public struct NativeMappedArray<TKey, TValue> : INativeDisposable where TKey : unmanaged, IEquatable<TKey> where TValue : unmanaged, IEquatable<TValue>
    {
        private NativeHashMap<int, List6<TValue>> data;
        private NativeHashMap<TKey, int> chunkNumbs;
        const int ListSize = 6;

        public NativeMappedArray(int keyCapacity, int arrayCapacity, Allocator allocator)
        {
            data = new(keyCapacity * Mathf.RoundToInt(arrayCapacity / 6f + 0.5f), allocator);
            chunkNumbs = new(keyCapacity, allocator);
        }

        public void Dispose()
        {
            data.Dispose();
            chunkNumbs.Dispose();
        }

        public JobHandle Dispose(JobHandle inputDeps)
        {
            return JobHandle.CombineDependencies(data.Dispose(inputDeps), chunkNumbs.Dispose(inputDeps));
        }

        #region Getters
        public int KeyCount()
        {
            return chunkNumbs.Count;
        }

        public int ValueCount(TKey key)
        {
            int lastChunkIndex = chunkNumbs[key];
            return (lastChunkIndex * ListSize) + data[GetHash(key, lastChunkIndex)].Length;
        }

        public int ChunkCount(TKey key)
        {
            return chunkNumbs[key] + 1;
        }

        public List6<TValue> GetChunk(TKey key, int index)
        {
            return data[GetHash(key, index)];
        }

        public TValue GetValue(TKey key, int index)
        {
            int chunkIndex = index / ListSize;
            int indexInChunk = index % ListSize;

            var list = data[GetHash(key, chunkIndex)];

            return list[indexInChunk];
        }

        #endregion Getters

        #region Modifier

        public bool AddKey(TKey key)
        {
            // If key is not in the map, add new.
            if (!ContainsKey(key))
            {
                chunkNumbs.Add(key, 0);
                data.Add(GetHash(key, 0), new List6<TValue>());
                return true;
            }
            return false;
        }

        public bool RemoveKey(TKey key)
        {
            // If key is not in the map, return false.
            if (!ContainsKey(key))
                return false;

            // Remove all chunk with the key.
            for (int i = 0; i <= chunkNumbs[key]; i++)
            {
                data.Remove(GetHash(key, i));
            }
            return true;
        }

        public bool Add(TKey key, TValue value)
        {
            // If key is not in the map, add new.
            if (!ContainsKey(key))
            {
                chunkNumbs.Add(key, 0);
                data.Add(GetHash(key, 0), new List6<TValue>());
            }

            int lastChunkKey = GetHash(key, chunkNumbs[key]);
            var list = data[lastChunkKey];

            // If last chunk is not full, add value.
            if (list.Length < ListSize)
            {
                list.Add(value);
                data[lastChunkKey] = list;
                return false;
            }

            // If last chunk is full, expand.
            lastChunkKey = GetHash(key, chunkNumbs[key] + 1);
            list = new List6<TValue>();
            list.Add(value);
            data.Add(lastChunkKey, list);
            chunkNumbs[key] += 1;
            return true;
        }

        public bool Remove(TKey key, TValue value)
        {
            // If key is not in the map, return false.
            if (!ContainsKey(key))
                return false;

            // If the key doesn't contains the value, return false.
            if (!Contains(key, value, out int chunkNumb, out int indexInChunk))
                return false;

            RemoveAt(key, chunkNumb, indexInChunk);
            return true;
        }

        public void RemoveAt(TKey key, int valueIndex)
        {
            int chunkIndex = index / ListSize;
            int indexInChunk = index % ListSize;
            RemoveAt(key, chunkIndex, indexInChunk);
        }

        public void RemoveAt(TKey key, int chunkIndex, int indexInChunk)
        {
            int chunkKey = GetHash(key, chunkIndex);
            var list = data[chunkKey];

            // If the chunk is the last one, remove the value.
            if (chunkIndex == chunkNumbs[key])
            {
                // If the chunk is empty after removal, remove it.
                if (list.Length == 1)
                {
                    RemoveLastChunk(key);
                    return;
                }

                // Otherwise, remove the value.
                list.RemoveAt(indexInChunk);
                data[chunkKey] = list;
                return;
            }

            // If not, move the last value of the last chunk to the remove slot.
            var lastChunk = data[GetHash(key, chunkNumbs[key])];
            var lastValue = lastChunk[lastChunk.Length - 1];

            list.ChangeValueAt(indexInChunk, lastValue);

            // If the last chunk is empty after removal, remove it.
            if (lastChunk.Length == 1)
            {
                RemoveLastChunk(key);
                return;
            }

            // Otherwise, remove the last value.
            lastChunk.RemoveAt(lastChunk.Length - 1);
            data[GetHash(key, chunkNumbs[key])] = lastChunk;
        }
        #endregion Modifier

        #region Check Contain
        public bool ContainsKey(TKey key)
        {
            return chunkNumbs.ContainsKey(key);
        }

        public bool Contains(TKey key, TValue value, out int chunkNumb, out int indexInChunk)
        {
            chunkNumb = -1;
            indexInChunk = -1;

            // If key is not in the map, return false.
            if (!ContainsKey(key))
                return false;

            // Traverse chunks to look for the first matching value.
            for (int i = 0; i <= chunkNumbs[key]; i++)
            {
                var list = data[GetHash(key, i)];
                for (int j = 0; j < list.Length; j++)
                {
                    if (list[j].Equals(value))
                    {
                        chunkNumb = i;
                        indexInChunk = j;
                        return true;
                    }
                }
            }
            return false;
        }
        #endregion Check Contain

        #region Helper
        void RemoveLastChunk(TKey key)
        {
            int lastChunkIndex = chunkNumbs[key];

            // If no chunk left, remove the key.
            if (lastChunkIndex == 0)
            {
                chunkNumbs.Remove(key);
                data.Remove(GetHash(key, 0));
                return;
            }

            // Otherwise, remove the last chunk.
            data.Remove(GetHash(key, lastChunkIndex));
            chunkNumbs[key] = lastChunkIndex - 1;
        }

        int GetHash(TKey key, int chunkIndex)
        {
            // Combine hash code algorithm from : https://stackoverflow.com/a/37449594/25303700
            int h1 = key.GetHashCode();
            int h2 = chunkIndex.GetHashCode();
            return ((h1 << 5) + h1) ^ h2;
        }

        #endregion Helper
    }

}

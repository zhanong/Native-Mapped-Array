# Native Mapped Array

This data structure comes from my personal game. It is unmanaged so capable for burst-compile. It is simple and easy to modify but not necessarily the most efficient.


## FEATURE
1. Burst-Compilable.
2. Shares capacity space among keys.


## IDEA

#### Basic Structure
<img src="https://github.com/zhanong/Native-Mapped-Array/blob/main/image/basic.jpg?raw=true" alt="drawing" width="50%"/>

#### Adding Item
<img src="https://github.com/zhanong/Native-Mapped-Array/blob/main/image/add.jpg?raw=true" alt="drawing" width="50%"/>

#### Removing Item
<img src="https://github.com/zhanong/Native-Mapped-Array/blob/main/image/remove.jpg?raw=true" alt="drawing" width="50%"/>



## HOW TO USE

#### Constructor
```c#
public NativeMappedArray(int keyCapacity, int arrayCapacity, Allocator allocator)
```
`keyCapacity`  How many keys are expected to contain.
`arrayCapacity`   'Array capacity' for each key.

------------

#### Add
If the key doesn't exist, add a key with an empty chunk and return true .
```c#
public bool AddKey(TKey key) 
```
Add value to the last chunk of the key. If a new chunk is added, return true.
```c#
public bool Add(TKey key, TValue value) 
```
------------

#### Remove

Remove a key and all its chunck.
```c#
public bool RemoveKey(TKey key)
```

Remove the first item in key's chunks that `Equals(value)`.
(In the worst case, this will traverse all items in chunks that belongs to key.)
```c#
public bool Remove(TKey key, TValue value)
```

Remove the `valueIndex % chunkSize` item at the `valueIndex / chunkSize` chunk of the key. Use this instead of `Remove()`if possible.
```c#
public void RemoveAt(TKey key, int valueIndex)
```
------------

#### Access Data
The best way to get all values:
```c#
NativeMappedArray<K, V> map;

int valueCount = map.ValueCount(k)
for (int i = 0; i < valueCount; i++)
{
	V value = map.GetValueAt(k, i);
	// Do something.
}
```

## In the Future
The chunk is implemented as List6. It's size is fixed as 6. It's better to make chunk type as a generic type for the structure.


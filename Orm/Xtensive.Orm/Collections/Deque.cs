// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.Serialization;
using System.Security;
using System.Security.Permissions;
using Xtensive.Comparison;
using Xtensive.Core;
using Xtensive.Internals.DocTemplates;


namespace Xtensive.Collections
{
  /// <summary>
  /// Double-ended queue.
  /// </summary>
  /// <typeparam name="T">The type of queued elements.</typeparam>
  /// <remarks>
  /// <para>
  /// <see cref="Deque{T}"/> it is a sequence that supports random access to its elements, 
  /// constant time of insertion and removal of elements at the both ends of the sequence, 
  /// and linear time of insertion and removal of elements in the middle.
  /// </para>
  /// <para>
  /// The capacity of a <see cref="Deque{T}"/> is the number of elements the <see cref="Deque{T}"/> can hold.
  /// In this implementation, the default initial capacity for a <see cref="Deque{T}"/> is 16;
  /// however, that default might change in future versions.
  /// </para>
  /// <para>
  /// As elements are added to a <see cref="Deque{T}"/>, the capacity is automatically increased as required
  /// by reallocating the internal array. The capacity can be decreased by calling <see cref="TrimExcess()"/>.
  /// </para>
  /// <para>
  /// The growth factor is the number by which the current capacity is multiplied when a greater capacity
  /// is required. The growth factor is determined when the <see langword="Deque"/> is constructed.
  /// </para>
  /// <para>
  /// <see cref="Deque{T}"/> accepts a <see langword="null"/>
  /// as a valid value for reference types and allows duplicate elements.
  /// </para>
  /// </remarks>
  [Serializable]
  [DebuggerDisplay("Count = {Count}")]
  public class Deque<T> : IDeque<T>, 
    ISerializable,
    ICloneable
  {
    private const int minimalCapacity = 16;
    private const float defaultGrowFactor = 1.4f;
    private const float trimThresholdFactor = 0.9f;

    private T[] items; // Actual data buffer
    private int headPos; // Heading free element position
    private int tailPos; // Taililg free element position
    private int count; // Actual number of stored elements
    private int version; // Version to detect consistency while enumeration executes
    private readonly float growFactor = defaultGrowFactor;

    /// <inheritdoc/>
    public int Count
    {
      [DebuggerStepThrough]
      get { return count; }
    }

    /// <inheritdoc/>
    public T this[int index] {
      get {
        ArgumentValidator.EnsureArgumentIsInRange(index, 0, count-1, "index");
        return items[ConvertToBufferIndex(index)];
      }
      set {
        ArgumentValidator.EnsureArgumentIsInRange(index, 0, count-1, "index");
        items[ConvertToBufferIndex(index)] = value;
        version++;
      }
    }

    #region Capacity, TrimExcess

    /// <inheritdoc/>
    public int Capacity
    {
      get { return items.Length; }
      set
      {
        if (value!=items.Length) {
          if (value < count)
            throw new ArgumentOutOfRangeException("value", Strings.ExSpecifiedCapacityIsLessThenCollectionCount);
          if (value > minimalCapacity) {
            T[] newItems = new T[value];
            InnerCopyTo(newItems, 0);
            headPos = value - 1;
            tailPos = count;
            items = newItems;
            version++;
          }
          else {
            items = new T[minimalCapacity];
            headPos = minimalCapacity - 1;
            tailPos = 0;
          }
        }
      }
    }

    /// <inheritdoc/>
    public void TrimExcess()
    {
      int trimThreshold = (int)(items.Length*trimThresholdFactor);
      if (count < trimThreshold)
        Capacity = count;
    }

    #endregion

    #region Head, Tail, Add\Extract Head\Tail

    /// <inheritdoc/>
    public T Head {
      get {
        if (count==0)
          throw Exceptions.CollectionIsEmpty(null);
        return items[(items.Length - headPos==1 ? 0 : headPos + 1)];
      }
    }

    /// <inheritdoc/>
    public T HeadOrDefault {
      get {
        return count==0 ? default(T) : items[(items.Length - headPos==1 ? 0 : headPos + 1)];
      }
    }

    /// <inheritdoc/>
    public T Tail {
      get {
        if (count==0)
          throw Exceptions.CollectionIsEmpty(null);
        return items[(tailPos==0 ? items.Length : tailPos) - 1];
      }
    }

    /// <inheritdoc/>
    public T TailOrDefault {
      get {
        return count==0 ? default(T) : items[(tailPos==0 ? items.Length : tailPos) - 1];
      }
    }

    /// <inheritdoc/>
    public void AddHead(T element)
    {
      if (count==items.Length)
        EnsureCapacity(count + 1);

      count++;
      items[headPos] = element;

      // Shift to the "previous free" position
      if ((--headPos)==-1) {
        // If count < buffer.Length then all elements in the interval 
        // [0 ... buffer.Length - count] are "free"
        headPos += items.Length;
      }
      version++;
    }

    /// <inheritdoc/>
    public void AddTail(T element)
    {
      if (count==items.Length)
        EnsureCapacity(count + 1);

      count++;
      items[tailPos] = element;

      // Shift to "previous free" position
      if ((++tailPos)==items.Length) {
        // If count < buffer.Length then all elements in the interval 
        // [count - 1 ... buffer.Length - 1] are "free"
        tailPos = 0;
      }
      version++;
    }

    /// <inheritdoc/>
    public T ExtractHead()
    {
      if (count==0)
        throw Exceptions.CollectionIsEmpty(null);

      // Shift to the "head element" position
      if ((++headPos)==items.Length)
        headPos = 0;

      // Extract element
      T result = items[headPos];
      items[headPos] = default(T);
      count--;
      version++;

      return result;
    }

    /// <inheritdoc/>
    public T ExtractTail()
    {
      if (count==0)
        throw Exceptions.CollectionIsEmpty(null);

      // Shift to the "tail element" position
      if ((--tailPos)==-1)
        tailPos += items.Length;

      // Extract element
      T result = items[tailPos];
      items[tailPos] = default(T);
      count--;
      version++;

      return result;
    }

    #endregion

    #region IndexOf, Contains

    /// <inheritdoc/>
    public int IndexOf(T item)
    {
      Predicate<T> criterion = delegate(T innerItem) {
        return AdvancedComparerStruct<T>.System.Equals(innerItem, item);
      };
      int headIndex = headPos + 1;
      if (headIndex==items.Length)
        headIndex = 0;
      if (tailPos==0 || tailPos - headIndex > 0)
        return ConvertToListIndex(Array.FindIndex(items, headIndex, count, criterion));
      else {
        int itemIndex = -1;
        if (headIndex > 0)
          itemIndex = Array.FindIndex(items, headIndex, criterion);
        if (itemIndex < 0)
          itemIndex = Array.FindIndex(items, 0, tailPos, criterion);
        return ConvertToListIndex(itemIndex);
      }
    }

    /// <inheritdoc/>
    public bool Contains(T item)
    {
      Predicate<T> criterion = delegate(T innerItem) { return AdvancedComparerStruct<T>.System.Equals(innerItem, item); };
      int headIndex = headPos + 1;
      if (tailPos - headIndex > 0)
        return Array.FindIndex(items, headIndex, count, criterion) >= 0;
      else
        return (tailPos > 0 && Array.FindIndex(items, 0, tailPos, criterion) >= 0) ||
          (headIndex < items.Length && Array.FindIndex(items, headIndex, criterion) >= 0);
    }

    #endregion

    #region CopyTo

    /// <inheritdoc/>
    public void CopyTo(T[] array, int arrayIndex)
    {
      ArgumentValidator.EnsureArgumentNotNull(array, "array");
      array.EnsureIndexIsValid<T>(arrayIndex);

      if (arrayIndex + count > array.Length)
        throw new ArgumentException(Strings.ExDestionationArrayIsTooSmall, "array");
      InnerCopyTo(array, arrayIndex);
    }

    /// <inheritdoc/>
    public void CopyTo(Array array, int index)
    {
      ArgumentValidator.EnsureArgumentNotNull(array, "array");
      array.EnsureIndexIsValid(index);

      if (array.Rank!=1)
        throw new ArgumentException(Strings.ExArrayIsMultidimensional, "array");
      if (array.GetLowerBound(0)!=0)
        throw new ArgumentException(Strings.ExArrayDoesNotHaveZeroBasedIndexing, "array");
      if (index + count > array.Length)
        throw new ArgumentException(Strings.ExDestionationArrayIsTooSmall, "array");
      InnerCopyTo(array, index);
    }

    #endregion

    #region Insert, RemoveAt, Remove, RemoveRange, Clear

    /// <inheritdoc/>
    public void Insert(int index, T item)
    {
      ArgumentValidator.EnsureArgumentIsInRange(index, 0, count, "index");

      if (count==items.Length)
        EnsureCapacity(count + 1);

      int bufferIndex = ConvertToBufferIndex(index);
      int headIndex = headPos + 1;
      if (headIndex==items.Length)
        headIndex = 0;
      if (bufferIndex==tailPos)
        AddTail(item);
      else if (bufferIndex==headIndex)
        AddHead(item);
      else if (bufferIndex < tailPos) {
        int dataToMoveLength = tailPos - bufferIndex;
        Array.Copy(items, bufferIndex, items, bufferIndex + 1, dataToMoveLength);
        items[bufferIndex] = item;
        // Shift to "next free" position
        if ((++tailPos)==items.Length)
          tailPos = 0;
        count++;
        version++;
      }
      else if (bufferIndex > headIndex) {
        int dataToMoveLength = bufferIndex - headIndex;
        Array.Copy(items, headPos + 1, items, headPos, dataToMoveLength);
        items[bufferIndex - 1] = item;
        // Shift to "previous free" position
        if ((--headPos)==-1)
          headPos += items.Length;
        count++;
        version++;
      }
      else {
        throw Exceptions.InternalError("Deque.Insert: Wrong buffer index detected.", CoreLog.Instance);
      }
    }

    /// <inheritdoc/>
    public void RemoveAt(int index)
    {
      ArgumentValidator.EnsureArgumentIsInRange(index, 0, count-1, "index");

      int bufferIndex = ConvertToBufferIndex(index);
      if (tailPos - bufferIndex==1)
        ExtractTail();
      else if (bufferIndex - headPos==1)
        ExtractHead();
      else if (bufferIndex > headPos) {
        int dataToMoveLength = bufferIndex - headPos - 1;
        // Shift to the "tail element" position
        headPos++;
        Array.Copy(items, headPos, items, headPos + 1, dataToMoveLength);
        items[headPos] = default(T);
        count--;
        version++;
      }
      else if (bufferIndex < tailPos) {
        int dataToMoveLength = tailPos - bufferIndex - 1;
        // Shift to the "head element" position
        tailPos--;
        Array.Copy(items, bufferIndex + 1, items, bufferIndex, dataToMoveLength);
        items[tailPos] = default(T);
        count--;
        version++;
      }
      else {
        throw Exceptions.InternalError("Deque.RemoveAt: Wrong buffer index detected.", CoreLog.Instance);
      }
    }

    /// <inheritdoc/>
    public bool Remove(T item)
    {
      int itemIndex = IndexOf(item);
      if (itemIndex < 0)
        return false;
      RemoveAt(itemIndex);
      version++;
      return true;
    }

    /// <inheritdoc/>
    public void RemoveRange(int index, int count)
    {
      ArgumentValidator.EnsureArgumentIsInRange(index, 0, this.count-1, "index");
      ArgumentValidator.EnsureArgumentIsInRange(count, 0, this.count-index, "count");

      if (count < 0 || (count > this.count - index))
        throw new ArgumentOutOfRangeException("count");

      int bufferIndex = ConvertToBufferIndex(index);
      if (this.count > 0) {
        int tailLength = this.count - count - index;
        if (tailLength > 0) {
          if (tailPos - bufferIndex > 0) {
            Array.Copy(items, bufferIndex + count, items, bufferIndex, this.count - count - index);
          }
          else {
            int endBufferIndex = ConvertToBufferIndex(index + count);
            int firstChunkLength = Math.Min(count, items.Length - bufferIndex);
            int secondChunkLength = Math.Max(0, count - firstChunkLength);
            if (firstChunkLength > 0) {
              Array.Copy(items, endBufferIndex, items, bufferIndex, Math.Min(firstChunkLength, tailLength));
            }
            if (secondChunkLength > 0 && tailLength > firstChunkLength) {
              Array.Copy(items, endBufferIndex + firstChunkLength, items, 0, tailLength - firstChunkLength);
            }
          }
        }
        int startFillPosition = ConvertToBufferIndex(this.count - count);
        int newTail = startFillPosition;
        if (tailPos < startFillPosition) {
          for (int i = startFillPosition; i < items.Length; i++)
            items[i] = default(T);
          startFillPosition = 0;
        }
        for (int i = startFillPosition; i < tailPos; i++)
          items[i] = default(T);

        tailPos = newTail;
        this.count -= count;
      }
    }

    /// <inheritdoc/>
    public void Clear()
    {
      int capacity = items.Length;
      Array.Clear(items, 0, capacity);
      headPos = capacity - 1;
      tailPos = 0;
      count = 0;
      version++;
    }

    #endregion

    #region ICollection<T>, ICollection members

    /// <inheritdoc/>
    void ICollection<T>.Add(T item)
    {
      AddTail(item);
    }

    /// <inheritdoc/>
    bool ICollection<T>.IsReadOnly
    {
      get { return false; }
    }

    #endregion

    #region IEnumerable<T>, IEnumerable members

    /// <inheritdoc/>
    [DebuggerStepThrough]
    IEnumerator IEnumerable.GetEnumerator()
    {
      return GetEnumerator();
    }

    /// <inheritdoc/>
    public IEnumerator<T> GetEnumerator()
    {
      int itemIndex = headPos;
      int itemCount = count;
      int length = items.Length;
      int oldVersion = version;
      while (itemCount-- > 0) {
        if (version!=oldVersion)
          throw Exceptions.CollectionHasBeenChanged(null);
        if ((++itemIndex)==length)
          itemIndex = 0;
        yield return items[itemIndex];
      }
    }

    #endregion

    #region ICloneable members

    /// <summary>
    /// Creates a shallow copy of the <see cref="Deque{T}"/>.
    /// </summary>
    /// <returns>A shallow copy of the <see cref="Deque{T}"/>.</returns>
    /// <remarks>
    /// <para>
    /// A shallow copy of a collection copies only the elements of the collection,
    /// whether they are reference types or value types, but it does not copy 
    /// the objects that the references refer to. The references in the new collection 
    /// point to the same objects that the references in the original collection point to.
    /// </para>
    /// <para>
    /// In contrast, a deep copy of a collection copies the elements and 
    /// everything directly or indirectly referenced by the elements.
    /// </para>
    /// <para>
    /// This method is an O(n) operation, where n is <see langword="Count"/>.
    /// </para>
    /// </remarks>
    public virtual object Clone()
    {
      return new Deque<T>(this);
    }

    #endregion


    #region Private \ internal methods

    private void InnerCopyTo(Array array, int arrayIndex)
    {
      int headIndex = headPos + 1;
      if (count > 0) {
        if (tailPos - headIndex > 0)
          Array.Copy(items, headIndex, array, arrayIndex, count);
        else {
          int headPartLength = items.Length - headIndex;
          if (headPartLength > 0)
            Array.Copy(items, headIndex, array, arrayIndex, headPartLength);
          Array.Copy(items, 0, array, arrayIndex + headPartLength, count - headPartLength);
        }
      }
    }

    private int ConvertToListIndex(int bufferIndex)
    {
      if (bufferIndex < 0)
        return bufferIndex;
      return (bufferIndex > headPos ? 0 : items.Length) + bufferIndex - headPos - 1;
    }

    private int ConvertToBufferIndex(int listIndex)
    {
      if (listIndex < 0)
        return listIndex;
      int bufferIndex = headPos + listIndex + 1;
      return bufferIndex - (bufferIndex >= items.Length ? items.Length : 0);
    }

    private void EnsureCapacity(int requiredCapacity)
    {
      int currentCapacity = items.Length;
      if (currentCapacity < requiredCapacity) {
        int newCapacity = currentCapacity==0
          ? minimalCapacity
          : Convert.ToInt32(currentCapacity*growFactor);
        if (newCapacity < requiredCapacity)
          newCapacity = requiredCapacity;
        Capacity = newCapacity;
      }
    }

    #endregion


    // Constructors

    private Deque(Deque<T> source)
    {
      count = source.count;
      items = new T[count];
      source.InnerCopyTo(items, 0);
      headPos = count - 1;
      tailPos = 0;
      growFactor = source.growFactor;
    }

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true" />
    /// </summary>
    public Deque()
      : this(minimalCapacity)
    {
    }

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true" />
    /// </summary>
    /// <param name="initialCapacity">The initial <see cref="Capacity"/> property value.</param>
    public Deque(int initialCapacity)
    {
      if (initialCapacity < minimalCapacity)
        initialCapacity = minimalCapacity;
      items = new T[initialCapacity];
      headPos = initialCapacity - 1;
      tailPos = 0;
      count = 0;
    }

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true" />
    /// </summary>
    /// <param name="initialCapacity">The initial <see cref="Capacity"/> property value.</param>
    /// <param name="growFactor">The factor by which the capacity of the <see cref="Deque{T}"/> is expanded.</param>
    public Deque(int initialCapacity, float growFactor)
      : this(initialCapacity)
    {
      this.growFactor = growFactor;
    }

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true" />
    /// </summary>
    /// <param name="source">The initial contents of the <see cref="Deque{T}"/>.</param>
    public Deque(IEnumerable<T> source)
    {
      items = new List<T>(source).ToArray();
      headPos = -1;
      tailPos = items.Length;
      count = items.Length;
      EnsureCapacity(Capacity);
    }

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true" />
    /// </summary>
    /// <param name="source">The initial contents of the <see cref="Deque{T}"/>.</param>
    /// <param name="growFactor">The factor by which the capacity of the <see cref="Deque{T}"/> is expanded.</param>
    public Deque(IEnumerable<T> source, float growFactor)
      : this(source)
    {
      this.growFactor = growFactor;
    }

    #region ISerializable Members

    /// <see cref="SerializableDocTemplate.GetObjectData" copy="true" />
    [SecurityCritical]
    public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
    {
      T[] arrItems = new T[count];
      InnerCopyTo(arrItems, 0);
      info.AddValue("Items", arrItems);
      info.AddValue("GrowFactor", growFactor);
    }

    /// <see cref="SerializableDocTemplate.Ctor" copy="true" />
    protected Deque(SerializationInfo info, StreamingContext context)
    {
      items = (T[])info.GetValue("Items", typeof (T[]));
      growFactor = info.GetSingle("GrowFactor");
      count = items.Length;
      headPos = count - 1;
      tailPos = count - 1;
    }

    #endregion
  }
}
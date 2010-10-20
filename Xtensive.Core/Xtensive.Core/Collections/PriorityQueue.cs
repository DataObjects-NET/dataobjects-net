// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Gamzov
// Created:    2007.06.13

using System;
using System.Collections.Generic;
using System.Collections;
using System.Diagnostics;
using System.Runtime.Serialization;
using System.Security;
using System.Security.Permissions;
using Xtensive.Comparison;
using Xtensive.Core;
using Xtensive.Internals.DocTemplates;
using Xtensive.Resources;

namespace Xtensive.Collections
{
  /// <summary>
  /// Priority queue.
  /// </summary>
  /// <typeparam name="T"><see cref="Type"/> of objects to be stored in queue.</typeparam>
  /// <typeparam name="TPriority"><see cref="Type"/> of priority value.</typeparam>
  [Serializable]
  [DebuggerDisplay("Count = {Count}")]
  public class PriorityQueue<T, TPriority> : IPriorityQueue<T, TPriority>, 
    ISerializable
    where TPriority: IComparable<TPriority>
  {
    // Consts
    private const int minimalCapacity = 16;
    private const float defaultGrowFactor = 1.4f;
    private const float trimThresholdFactor = 0.9f;

    // Fields
    [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
    private Pair<T, TPriority>[] items;   // Actual data buffer
    private int version; // Version to detect consistency while enumeration executes
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private int count; // Count of items in queue
    private readonly float growFactor = defaultGrowFactor;
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private readonly Direction direction;
    private readonly PriorityQueueItemComparer<T, TPriority> comparer;

    #region IPriorityQueue<T,TPriority> Members

    /// <inheritdoc/>
    public int Capacity
    {
      [DebuggerStepThrough]
      get {
        return items.Length;
      }
      set {
        if (value != items.Length) {
          if (value < count)
            throw new ArgumentOutOfRangeException("value", Strings.ExInvalidCapacity);
          if (value < minimalCapacity) {
            value = minimalCapacity;
          }
          Pair<T, TPriority>[] newItems = new Pair<T, TPriority>[value];
          Array.Copy(items, newItems, Count);
          items = newItems;
          version++;
        }
      }
    }

    /// <inheritdoc/>
    public Direction Direction
    {
      [DebuggerStepThrough]
      get { return direction; }
    }

    /// <inheritdoc/>
    public long Count
    {
      [DebuggerStepThrough]
      get { return count; }
    }

    /// <inheritdoc/>
    public T this[int index]
    {
      get {
        if (index < 0)
          throw new ArgumentOutOfRangeException(Strings.ExArgumentValueMustBeGreaterThanOrEqualToZero);
        if (index >= count)
          throw new ArgumentOutOfRangeException(string.Format(Strings.ExIndexShouldBeInNMRange, 0, count-1));
        return items[count-index-1].First;
      }
    }

    /// <inheritdoc/>
    public bool Contains(T item)
    {
      Predicate<Pair<T, TPriority>> predicate = GetEqualityPredicate(item);
      return Array.FindIndex(items, 0, count, predicate) >= 0;
    }

    /// <inheritdoc/>
    public void Remove(T item)
    {
      Predicate<Pair<T, TPriority>> predicate = GetEqualityPredicate(item);
      int index = Array.FindIndex(items, 0, count, predicate);
      if (index<0) {
        throw new InvalidOperationException(Strings.ExItemNotFound);
      }
      Array.Copy(items, index+1, items, index, count-index-1);
      Array.Clear(items, count-1, 1);
      count--;
    }

    /// <inheritdoc/>
    public void TrimExcess()
    {
      int trimThreshold = (int)(items.Length * trimThresholdFactor);
      if (count < trimThreshold)
        Capacity = count;
    }

    /// <inheritdoc/>
    public void Clear()
    {
      int capacity = items.Length;
      Array.Clear(items, 0, capacity);
      count = 0;
      version++;
    }

    /// <inheritdoc/>
    public void Enqueue(T item, TPriority priority)
    {
      int insertIndex = GetIndex(item, priority);
      EnsureCapacity((int) (Count+1));
      if (insertIndex < Count)
      {
        Array.Copy(items, insertIndex, items, insertIndex + 1, Count - insertIndex);
      }
      items[insertIndex] = new Pair<T, TPriority>(item, priority);
      count++;
      version++;
    }

    /// <inheritdoc/>
    public T Dequeue()
    {
      if (Count == 0){
        throw new InvalidOperationException(Strings.ExCollectionIsEmpty);
      }
      T result = items[Count - 1].First;
      Array.Clear(items, (int) (Count - 1), 1);
      count--;
      TrimExcess();
      version++;
      return result;
    }

    /// <inheritdoc/>
    public T[] DequeueRange(TPriority priority)
    {
      int index = GetIndex(default(T), priority);
      if (index < count){
        T[] result = new T[count - index];
        for (int i = index; i < count;i++ ) {
          result[i - index] = items[i].First;
        }
        Array.Clear(items, index, count-index);
        count = index;
        return result;
      }
      else {
        return new T[0];
      }

    }

    /// <inheritdoc/>
    public T Peek()
    {
      if (Count==0) {
        throw new InvalidOperationException(Strings.ExCollectionIsEmpty);
      }
      version++;
      return items[Count - 1].First;
    }

    #endregion

    #region IEnumerable<...> Members

    /// <inheritdoc/>
    [DebuggerStepThrough]
    IEnumerator IEnumerable.GetEnumerator()
    {
      return GetEnumerator();
    }

    /// <inheritdoc/>
    public IEnumerator<T> GetEnumerator()
    {
      int initialVersion = version;
      for (int i = 0; i < Count;i++ ) {
        if (version != initialVersion)
          throw new InvalidOperationException(Strings.ExCollectionHasBeenChanged);
        yield return items[i].First;
      }
    }

    #endregion

    #region ICloneable Members

    /// <inheritdoc/>
    public object Clone()
    {
      return new PriorityQueue<T, TPriority>(this);
    }

    #endregion

    #region Private \ internal members

    private int GetIndex(T item, TPriority priority)
    {
      Pair<T,TPriority> element = new Pair<T, TPriority>(item, priority);
      int insertIndex = Array.BinarySearch(items, 0, (int)Count, element, comparer);
      if (insertIndex < 0) {
        insertIndex ^= -1;
      }
      while (insertIndex < Count && insertIndex > 0 && comparer.Compare(items[insertIndex-1], element) == 0)
      {
        insertIndex--;
      }
      return insertIndex;
    }

    private static Predicate<Pair<T, TPriority>> GetEqualityPredicate(T item)
    {
      return delegate(Pair<T, TPriority> innerItem)
      {
        return AdvancedComparerStruct<T>.System.Equals(innerItem.First, item);
      };
    }

    private void EnsureCapacity(int requiredCapacity)
    {
      int currentCapacity = items.Length;
      if (currentCapacity < requiredCapacity)
      {
        int newCapacity = currentCapacity == 0
          ? minimalCapacity
          : Convert.ToInt32(currentCapacity * growFactor);
        if (newCapacity < requiredCapacity)
          newCapacity = requiredCapacity;
        Capacity = newCapacity;
      }
    }

    #endregion


    // Constructors

    private PriorityQueue(PriorityQueue<T, TPriority> source)
    {
      items = new Pair<T, TPriority>[source.Count];
      Array.Copy(source.items, items, source.Count);
      count = items.Length;
      direction = source.direction;
    }

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    public PriorityQueue()
      : this(Direction.Positive)
    {
    }

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="direction">Initial <see cref="Direction"/> property value.</param>
    public PriorityQueue(Direction direction)
      : this(direction, minimalCapacity)
    {
    }


    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="direction">Initial <see cref="Direction"/> property value.</param>
    /// <param name="initialCapacity">Initial <see cref="Capacity"/> property value.</param>
    public PriorityQueue(Direction direction, int initialCapacity) 
      : this(direction, initialCapacity, defaultGrowFactor)
    {
    }
 
    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="direction">Initial <see cref="Direction"/> property value.</param>
    /// <param name="initialCapacity">Initial <see cref="Capacity"/> property value.</param>
    /// <param name="growFactor">Capacity grow factor.</param>
    public PriorityQueue(Direction direction, int initialCapacity, float growFactor)
    {
      if (direction==Direction.None)
        throw Exceptions.InvalidArgument(direction, "direction");
      this.direction = direction;
      if (initialCapacity < minimalCapacity) {
        initialCapacity = minimalCapacity;
      }
      items = new Pair<T, TPriority>[initialCapacity]; 
      count = 0;
      this.growFactor = growFactor;
      comparer = new PriorityQueueItemComparer<T, TPriority>(direction);
    }

    #region ISerializable Members

    /// <see cref="SerializableDocTemplate.GetObjectData" copy="true" />
    #if NET40
    [SecurityCritical]
    #else
    [SecurityPermission(SecurityAction.LinkDemand, SerializationFormatter=true)]
    #endif
    public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
    {
      Pair<T, TPriority>[] arrItems = new Pair<T, TPriority>[count];
      Array.Copy(items, arrItems, count);
      info.AddValue("Items", arrItems);
      info.AddValue("GrowFactor", growFactor);
      info.AddValue("Direction", direction);
    }

    /// <see cref="SerializableDocTemplate.Ctor" copy="true" />
    protected PriorityQueue(SerializationInfo info, StreamingContext context)
    {
      items = (Pair<T, TPriority>[])info.GetValue("Items", typeof(Pair<T, TPriority>[]));
      growFactor = info.GetSingle("GrowFactor");
      direction = (Direction)info.GetValue("Direction", typeof(Direction));
      if (direction==Direction.None)
        throw Exceptions.InvalidArgument(direction, "direction");
      count = items.Length;
    }

    #endregion
  }
}
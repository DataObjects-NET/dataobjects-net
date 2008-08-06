// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2007.05.24

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using Xtensive.Core.Internals.DocTemplates;
using Xtensive.Core.Resources;
using Xtensive.Core.Threading;

namespace Xtensive.Core.Collections
{
  [Serializable]
  public class Pool<T> : IPool<T>
  {
    private const int DefaultCapacity = 16;

    private int capacity;
    private SetSlim<T> items = new SetSlim<T>();
    private SetSlim<T> availableItems = new SetSlim<T>();


    [DebuggerHidden]
    public int Capacity
    {
      get { return capacity; }
    }

    [DebuggerHidden]
    public int AvailableCount
    {
      get { return availableItems.Count; }
    }

    [DebuggerHidden]
    public int Count
    {
      get { return items.Count; }
    }

    #region Add, Remove, Consume, Release, etc...

    public bool Add(T item)
    {
      ArgumentValidator.EnsureArgumentNotNull(item, "item");
      if (items.Contains(item))
        return false;
      items.Add(item);
      availableItems.Add(item);
      return true;
    }

    public bool Remove(T item)
    {
      ArgumentValidator.EnsureArgumentNotNull(item, "item");
      if (!IsPooled(item))
        throw new InvalidOperationException(Strings.ExItemIsNotPooled);
      if (IsConsumed(item))
        throw new InvalidOperationException(Strings.ExItemIsInUse);
      items.Remove(item);
      availableItems.Remove(item);
      OnItemRemoved(new ItemRemovedEventArgs<T>(item));
      return true;
    }

    public T Consume()
    {
      if (AvailableCount==0)
        throw new InvalidOperationException(Strings.ExNoAvailableItems);
      T item = default(T);
      foreach (T i in availableItems) {
        item = i;
        break;
      }
      availableItems.Remove(item);
      return item;
    }

    public void Consume(T item)
    {
      Add(item);
      if (IsConsumed(item))
        throw new InvalidOperationException(Strings.ExItemIsInUse);
      availableItems.Remove(item);
    }

    public T Consume(Func<T> itemGenerator)
    {
      T item;
      if (AvailableCount>0)
        item = Consume();
      else {
        item = itemGenerator();
        Consume(item);
      }
      return item;
    }

    public void Release(T item)
    {
      if (!IsPooled(item))
        throw new InvalidOperationException(Strings.ExItemIsNotPooled);
      if (!IsConsumed(item))
        throw new InvalidOperationException(Strings.ExItemIsNotInUse);
      availableItems.Add(item);
      if (Count>Capacity && Capacity>0)
        Remove(item);
    }

    public void ExecuteConsumer(Func<T> itemGenerator, Action<T> consumer)
    {
      T item = Consume(itemGenerator);
      try {
        consumer(item);
      }
      finally {
        Release(item);
      }
    }

    #endregion

    #region IsXxx methods

    public bool IsPooled(T item)
    {
      ArgumentValidator.EnsureArgumentNotNull(item, "item");
      return items.Contains(item);
    }

    public bool IsAvailable(T item)
    {
      ArgumentValidator.EnsureArgumentNotNull(item, "item");
      if (!IsPooled(item))
        return false;
      return availableItems.Contains(item);
    }

    public bool IsConsumed(T item)
    {
      ArgumentValidator.EnsureArgumentNotNull(item, "item");
      if (!IsPooled(item))
        return false;
      return !availableItems.Contains(item);
    }

    #endregion

    #region IEnumerable<...> Members

    /// <summary>
    /// Returns enumerator enumerating all the pooled items (available or not).
    /// </summary>
    /// <returns>Enumerator enumerating all the pooled items (available or not).</returns>
    IEnumerator<T> IEnumerable<T>.GetEnumerator()
    {
      return items.GetEnumerator();
    }

    /// <summary>
    /// Returns enumerator enumerating all the pooled items (available or not).
    /// </summary>
    /// <returns>Enumerator enumerating all the pooled items (available or not).</returns>
    IEnumerator IEnumerable.GetEnumerator()
    {
      return items.GetEnumerator();
    }

    #endregion

    #region Events

    /// <inheritdoc/>
    public event EventHandler<ItemRemovedEventArgs<T>> ItemRemoved;

    /// <summary>
    /// Invokes <see cref="ItemRemoved"/> event.
    /// </summary>
    /// <param name="eventArgs">Event arguments.</param>
    protected virtual void OnItemRemoved(ItemRemovedEventArgs<T> eventArgs)
    {
      if (ItemRemoved!=null)
        ItemRemoved(this, eventArgs);
    }

    #endregion


    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true" />
    /// </summary>
    public Pool()
      : this(DefaultCapacity)
    {
    }

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true" />
    /// </summary>
    /// <param name="capacity">Initial <see cref="Capacity"/> property value.</param>
    public Pool(int capacity)
    {
      if (capacity < 0)
        throw new ArgumentOutOfRangeException("capacity", Strings.ExArgumentValueMustBeGreaterThanZero);
      this.capacity = capacity;
    }
  }
}

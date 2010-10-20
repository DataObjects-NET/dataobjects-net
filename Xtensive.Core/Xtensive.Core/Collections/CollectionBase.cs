// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2007.03.01

using System;
using System.Collections.Generic;
using System.Diagnostics;
using Xtensive.Core;
using Xtensive.Internals.DocTemplates;
using Xtensive.Notifications;

namespace Xtensive.Collections
{
  /// <summary>
  /// Base class for any collection.
  /// </summary>
  [Serializable]
  [DebuggerDisplay("Count = {Count}")]
  public class CollectionBase<TItem>: CollectionBaseSlim<TItem>,
    ICollectionChangeNotifier<TItem>
  {
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private EventHandler<ChangeNotifierEventArgs> itemChangedHandler;
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private EventHandler<ChangeNotifierEventArgs> itemChangingHandler;
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private CollectionEventBroker<TItem> eventBroker;

    /// <summary>
    /// Gets or sets the element at the specified index.
    /// </summary>
    /// <param name="index">The zero-based index of the element to get or set.</param>
    /// <returns>The element at the specified index.</returns>
    public override TItem this[int index] {
      get {
        return base[index];
      }
      set {
        if (index < 0 || index >= Count)
          throw new IndexOutOfRangeException();
        OnValidate(value);
        TItem oldValue = base[index];
        if ((oldValue==null && value==null) ||
          (oldValue!=null && oldValue.Equals(value)))
          return;
        OnChanging();
        OnRemoving(oldValue, index);
        OnInserting(value, index);
        base[index] = value;
        try {
          OnRemoved(oldValue, index);
          OnInserted(value, index);
          OnChanged();
        }
        catch (Exception) {
          base[index] = oldValue;
          throw;
        }
      }
    }

    /// <summary>
    /// Inserts an element to the collection.
    /// </summary>
    /// <param name="index">The zero-based index at which value should be inserted.</param>
    /// <param name="value">Item to insert.</param>
    public override void Insert(int index, TItem value)
    {
      if (index < 0 || index > Count)
        throw new ArgumentOutOfRangeException("index");
      OnValidate(value);
      OnChanging();
      OnInserting(value, index);
      base.Insert(index, value);
      try {
        OnInserted(value, index);
        OnChanged();
      }
      catch (Exception) {
        base.RemoveAt(index);
        throw;
      }
    }

    /// <summary>
    /// Removes the element at the specified index of the
    /// collection instance.
    /// </summary>
    /// <param name="index">The zero-based index of the element to remove.</param>
    public override void RemoveAt(int index)
    {
      if (index < 0 || index >= Count)
        throw new ArgumentOutOfRangeException("index");
      TItem o = base[index];
      OnValidate(o);
      OnChanging();
      OnRemoving(o, index);
      base.RemoveAt(index);
      OnRemoved(o, index);
      OnChanged();
    }

    /// <summary>
    /// Removes all objects from the 
    /// collection instance.
    /// </summary>
    public override void Clear()
    {
      OnChanging();
      OnClearing();
      foreach (TItem item in Items)
        TryUnsubscribe(item);
      base.Clear();
      OnCleared();
      OnChanged();
    }

    /// <summary>
    /// Adds new element to the collection.
    /// </summary>
    /// <param name="value">Item to add.</param>
    public override void Add(TItem value)
    {
      OnValidate(value);
      int index = Count;
      OnChanging();
      OnInserting(value, index);
      base.Add(value);
      try {
        OnInserted(value, index);
        OnChanged();
      }
      catch (Exception) {
        base.RemoveAt(index);
        throw;
      }
    }

    /// <inheritdoc/>
    public override void AddRange(IEnumerable<TItem> collection)
    {
      foreach (TItem item in collection)
        Add(item);
    }

    /// <summary>
    /// Removes element from the the collection.
    /// </summary>
    /// <param name="value">Item to remove.</param>
    public override bool Remove(TItem value)
    {
      OnValidate(value);
      int index = IndexOf(value);
      if (index < 0)
        return false;
      OnChanging();
      OnRemoving(value, index);
      base.RemoveAt(index);
      OnRemoved(value, index);
      OnChanged();
      return true;
    }

    private CollectionEventBroker<TItem> EventBroker {
      get {
        if (eventBroker==null)
          eventBroker = new CollectionEventBroker<TItem>();
        return eventBroker;
      }
    }

    private bool EventBrokerExists {
      get {
        return eventBroker!=null;
      }
    }

    /// <summary>
    /// Tries to subscribe the collection on 
    /// change notifications from the specified item.
    /// </summary>
    /// <param name="item">The item to try.</param>
    protected void TrySubscribe(TItem item)
    {
      IChangeNotifier notifier = item as IChangeNotifier;
      if (notifier != null) {
        notifier.Changing += itemChangingHandler;
        notifier.Changed += itemChangedHandler;
      }
    }

    /// <summary>
    /// Tries to unsubscribe the collection from
    /// change notifications from the specified item.
    /// </summary>
    /// <param name="item">The item to try.</param>
    protected void TryUnsubscribe(TItem item)
    {
      IChangeNotifier notifier = item as IChangeNotifier;
      if (notifier != null) {
        notifier.Changing -= itemChangingHandler;
        notifier.Changed -= itemChangedHandler;
      }
    }

    #region OnXxx methods

    /// <summary>
    /// Performs additional custom processes when changing the contents of the 
    /// collection instance.
    /// </summary>
    protected virtual void OnChanging()
    {
      if (EventBrokerExists)
        EventBroker.RaiseOnChanging(this, new ChangeNotifierEventArgs("Changing"));
    }

    /// <summary>
    /// Performs additional custom processes after the contents of the 
    /// collection instance was changed.
    /// </summary>
    protected virtual void OnChanged()
    {
      if (EventBrokerExists)
        EventBroker.RaiseOnChanged(this, new ChangeNotifierEventArgs("Changed"));
    }

    /// <summary>
    /// Performs additional custom processes when clearing the contents of the 
    /// collection instance.
    /// </summary>
    protected virtual void OnClearing()
    {
      if (EventBrokerExists)
        EventBroker.RaiseOnClearing(this, new ChangeNotifierEventArgs("Clearing"));
    }

    /// <summary>
    /// Performs additional custom processes after clearing the contents of the 
    /// collection instance.
    /// </summary>
    protected virtual void OnCleared()
    {
      if (EventBrokerExists)
        EventBroker.RaiseOnCleared(this, new ChangeNotifierEventArgs("Cleared"));
    }

    /// <summary>
    /// Performs additional custom processes before inserting a new element into the
    /// collection instance.
    /// </summary>
    /// <param name="index">The zero-based <paramref name="index"/> of the value to insert.</param>
    /// <param name="value">The value to insert.</param>
    protected virtual void OnInserting(TItem value, int index)
    {
      if (EventBrokerExists)
        EventBroker.RaiseOnInserting(this, new CollectionChangeNotifierEventArgs<TItem>(value, index));
    }

    /// <summary>
    /// Performs additional custom processes after inserting a new element into the
    /// collection instance.
    /// </summary>
    /// <param name="index">The zero-based <paramref name="index"/> of the value to insert.</param>
    /// <param name="value">The inserted value.</param>
    protected virtual void OnInserted(TItem value, int index)
    {
      TrySubscribe(value);
      if (EventBrokerExists)
        EventBroker.RaiseOnInserted(this, new CollectionChangeNotifierEventArgs<TItem>(value, index));
    }

    /// <summary>
    /// Performs additional custom processes before removing an element from the
    /// collection instance.
    /// </summary>
    /// <param name="index">The zero-based <paramref name="index"/> at which to insert value.</param>
    /// <param name="value">The element to remove.</param>
    protected virtual void OnRemoving(TItem value, int index)
    {
      if (EventBrokerExists)
        EventBroker.RaiseOnRemoving(this, new CollectionChangeNotifierEventArgs<TItem>(value, index));
    }

    /// <summary>
    /// Performs additional custom processes after removing an element from the
    /// collection instance.
    /// </summary>
    /// <param name="index">The zero-based <paramref name="index"/> at which to insert value.</param>
    /// <param name="value">The element to remove.</param>
    protected virtual void OnRemoved(TItem value, int index)
    {
      TryUnsubscribe(value);
      if (EventBrokerExists)
        EventBroker.RaiseOnRemoved(this, new CollectionChangeNotifierEventArgs<TItem>(value, index));
    }

    /// <summary>
    /// Performs additional custom processes when validating a value.
    /// </summary>
    /// <param name="value">The object to validate.</param>
    /// <remarks>
    /// The default implementation of this method determines whether value is a <see langword="null"/> 
    /// reference (Nothing in Visual Basic), and, if so, throws <see cref="ArgumentNullException"/>. 
    /// It is intended to be overridden by a derived class to perform additional action 
    /// when the specified element is validated.
    /// </remarks>
    protected virtual void OnValidate(TItem value)
    {
      if (EventBrokerExists)
        EventBroker.RaiseOnValidate(this, new CollectionChangeNotifierEventArgs<TItem>(value));
      ArgumentValidator.EnsureArgumentNotNull(value, "value");
    }

    /// <summary>
    /// Called when item is about to be changed.
    /// </summary>
    /// <param name="sender">The sender.</param>
    /// <param name="e">The <see cref="ChangeNotifierEventArgs"/> instance containing the event data.</param>
    protected virtual void OnItemChanging(object sender, ChangeNotifierEventArgs e)
    {
      if (EventBrokerExists)
        EventBroker.RaiseOnItemChanging(this, new CollectionChangeNotifierEventArgs<TItem>((TItem)sender));
    }

    /// <summary>
    /// Called when item was changed.
    /// </summary>
    /// <param name="sender">The sender.</param>
    /// <param name="e">The <see cref="ChangeNotifierEventArgs"/> instance containing the event data.</param>
    protected virtual void OnItemChanged(object sender, ChangeNotifierEventArgs e)
    {
      if (EventBrokerExists)
        EventBroker.RaiseOnItemChanged(this, new CollectionChangeNotifierEventArgs<TItem>((TItem)sender));
    }

    #endregion

    #region IChangeNotifier Members

    /// <summary>
    /// Occurs when collection is intended to be changed.
    /// </summary>
    public event EventHandler<ChangeNotifierEventArgs> Changing
    {
      add { EventBroker.OnChanging += value; }
      remove { EventBroker.OnChanging -= value; }
    }

    /// <summary>
    /// Occurs when collection was changed.
    /// </summary>
    public event EventHandler<ChangeNotifierEventArgs> Changed
    {
      add { EventBroker.OnChanged += value; }
      remove { EventBroker.OnChanged -= value; }
    }

    #endregion

    #region ISetChangeNotifier<TItem> Members

    /// <summary>
    /// Occurs when item is validated.
    /// </summary>
    public event EventHandler<CollectionChangeNotifierEventArgs<TItem>> Validate
    {
      add { EventBroker.OnValidate += value; }
      remove { EventBroker.OnValidate -= value; }
    }

    /// <summary>
    /// Occurs when collection is inteneded to be cleared.
    /// </summary>
    public event EventHandler<ChangeNotifierEventArgs> Clearing
    {
      add { EventBroker.OnClearing += value; }
      remove { EventBroker.OnClearing -= value; }
    }

    /// <summary>
    /// Occurs when collection was cleared.
    /// </summary>
    public event EventHandler<ChangeNotifierEventArgs> Cleared
    {
      add { EventBroker.OnCleared += value; }
      remove { EventBroker.OnCleared -= value; }
    }

    /// <summary>
    /// Occurs when item is inserting into collection.
    /// </summary>
    public event EventHandler<CollectionChangeNotifierEventArgs<TItem>> Inserting
    {
      add { EventBroker.OnInserting += value; }
      remove { EventBroker.OnInserting -= value; }
    }

    /// <summary>
    /// Occurs when item was inserted into colection.
    /// </summary>
    public event EventHandler<CollectionChangeNotifierEventArgs<TItem>> Inserted
    {
      add { EventBroker.OnInserted += value; }
      remove { EventBroker.OnInserted -= value; }
    }

    /// <summary>
    /// Occurs when item is removing from collection.
    /// </summary>
    public event EventHandler<CollectionChangeNotifierEventArgs<TItem>> Removing
    {
      add { EventBroker.OnRemoving += value; }
      remove { EventBroker.OnRemoving -= value; }
    }

    /// <summary>
    /// Occurs when item was removed from colection.
    /// </summary>
    public event EventHandler<CollectionChangeNotifierEventArgs<TItem>> Removed
    {
      add { EventBroker.OnRemoved += value; }
      remove { EventBroker.OnRemoved -= value; }
    }

    /// <inheritdoc/>
    public event EventHandler<CollectionChangeNotifierEventArgs<TItem>> ItemChanging
    {
      add { EventBroker.OnItemChanging += value; }
      remove { EventBroker.OnItemChanging -= value; }
    }

    /// <inheritdoc/>
    public event EventHandler<CollectionChangeNotifierEventArgs<TItem>> ItemChanged
    {
      add { EventBroker.OnItemChanged += value; }
      remove { EventBroker.OnItemChanged -= value; }
    }

    #endregion

    
    // Initializer

    private void Initialize()
    {
      itemChangingHandler = OnItemChanging;
      itemChangedHandler = OnItemChanged;
      foreach (TItem item in this)
        TrySubscribe(item);
    }


    // Constructors
    
    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true" />
    /// </summary>
    public CollectionBase()
      : this(0)
    {
    }

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true" />
    /// </summary>
    /// <param name="capacity">The capacity.</param>
    public CollectionBase(int capacity)
      : base(capacity)
    {
      Initialize();
    }

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true" />
    /// </summary>
    /// <param name="collection">The collection.</param>
    public CollectionBase(IEnumerable<TItem> collection)
      : base(collection)
    {
      Initialize();
    }
  }
}
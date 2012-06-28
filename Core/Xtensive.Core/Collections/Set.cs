// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2007.10.26

using System;
using System.Collections.Generic;
using System.Diagnostics;
using Xtensive.Core;
using Xtensive.Internals.DocTemplates;
using Xtensive.Collections;

namespace Xtensive.Collections
{
  /// <summary>
  /// A set of items (with event-handling support).
  /// </summary>
  /// <typeparam name="TItem">The type of the item.</typeparam>
  [Serializable]
  public class Set<TItem> : SetSlim<TItem>,
    ICollectionChangeNotifier<TItem>
  {
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private EventHandler<ChangeNotifierEventArgs> itemChangedHandler;
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private EventHandler<ChangeNotifierEventArgs> itemChangingHandler;
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private CollectionEventBroker<TItem> eventBroker;

    /// <inheritdoc/>
    public override bool Add(TItem item)
    {
      OnValidate(item);
      OnChanging();
      OnInserting(item);
      bool result = base.Add(item);
      try {
        OnInserted(item);
        OnChanged();
      } 
      // TODO: For Dmitri Maximov: catch more specific exception (FxCop)?
      catch (Exception) {
        Remove(item);
        return false;
      }
      return result;
    }

    /// <inheritdoc/>
    public override void Clear()
    {
      OnChanging();
      OnClearing();
      foreach (TItem item in this) {
        TryUnsubscribe(item);
      }
      base.Clear();
      OnCleared();
      OnChanged();
    }

    /// <inheritdoc/>
    public override bool Remove(TItem item)
    {
      OnChanging();
      OnRemoving(item);
      bool result = base.Remove(item);
      OnRemoved(item);
      OnChanged();
      return result;
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

    private void TrySubscribe(TItem item)
    {
      IChangeNotifier notifier = item as IChangeNotifier;
      if (notifier != null) {
        notifier.Changing += itemChangingHandler;
        notifier.Changed += itemChangedHandler;
      }
    }

    private void TryUnsubscribe(TItem item)
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
    /// <param name="value">The value to insert.</param>
    protected virtual void OnInserting(TItem value)
    {
      if (EventBrokerExists)
        EventBroker.RaiseOnInserting(this, new CollectionChangeNotifierEventArgs<TItem>(value));
    }

    /// <summary>
    /// Performs additional custom processes after inserting a new element into the
    /// collection instance.
    /// </summary>
    /// <param name="value">The inserted value.</param>
    protected virtual void OnInserted(TItem value)
    {
      TrySubscribe(value);
      if (EventBrokerExists)
        EventBroker.RaiseOnInserted(this, new CollectionChangeNotifierEventArgs<TItem>(value));
    }

    /// <summary>
    /// Performs additional custom processes before removing an element from the
    /// collection instance.
    /// </summary>
    /// <param name="value">The item to remove.</param>
    protected virtual void OnRemoving(TItem value)
    {
      if (EventBrokerExists)
        EventBroker.RaiseOnRemoving(this, new CollectionChangeNotifierEventArgs<TItem>(value));
    }

    /// <summary>
    /// Performs additional custom processes after removing an element from the
    /// collection instance.
    /// </summary>
    /// <param name="value">The item to remove.</param>
    protected virtual void OnRemoved(TItem value)
    {
      TryUnsubscribe(value);
      if (EventBrokerExists)
        EventBroker.RaiseOnRemoved(this, new CollectionChangeNotifierEventArgs<TItem>(value));
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

    /// <inheritdoc/>
    public event EventHandler<ChangeNotifierEventArgs> Changing
    {
      add { EventBroker.OnChanging += value; }
      remove { EventBroker.OnChanging -= value; }
    }

    /// <inheritdoc/>
    public event EventHandler<ChangeNotifierEventArgs> Changed
    {
      add { EventBroker.OnChanged += value; }
      remove { EventBroker.OnChanged -= value; }
    }

    #endregion

    #region ICollectionChangeNotifier<TItem> Members

    /// <inheritdoc/>
    public event EventHandler<CollectionChangeNotifierEventArgs<TItem>> Validate
    {
      add { EventBroker.OnValidate += value; }
      remove { EventBroker.OnValidate -= value; }
    }

    /// <inheritdoc/>
    public event EventHandler<ChangeNotifierEventArgs> Clearing
    {
      add { EventBroker.OnClearing += value; }
      remove { EventBroker.OnClearing -= value; }
    }

    /// <inheritdoc/>
    public event EventHandler<ChangeNotifierEventArgs> Cleared
    {
      add { EventBroker.OnCleared += value; }
      remove { EventBroker.OnCleared -= value; }
    }

    /// <inheritdoc/>
    public event EventHandler<CollectionChangeNotifierEventArgs<TItem>> Inserting
    {
      add { EventBroker.OnInserting += value; }
      remove { EventBroker.OnInserting -= value; }
    }

    /// <inheritdoc/>
    public event EventHandler<CollectionChangeNotifierEventArgs<TItem>> Inserted
    {
      add { EventBroker.OnInserted += value; }
      remove { EventBroker.OnInserted -= value; }
    }

    /// <inheritdoc/>
    public event EventHandler<CollectionChangeNotifierEventArgs<TItem>> Removing
    {
      add { EventBroker.OnRemoving += value; }
      remove { EventBroker.OnRemoving -= value; }
    }

    /// <inheritdoc/>
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

    private void Initialize()
    {
      itemChangingHandler = OnItemChanging;
      itemChangedHandler = OnItemChanged;
      if (Items.Count>0)
        foreach (TItem item in this)
          TrySubscribe(item);
    }


    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true" />
    /// </summary>
    public Set() 
      : this((IEqualityComparer<TItem>)null)
    {
    }

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true" />
    /// </summary>
    /// <param name="comparer">Equality comparer for the set type.</param>
    public Set(IEqualityComparer<TItem> comparer) 
      : base(comparer)
    {
      Initialize();
    }

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true" />
    /// </summary>
    /// <param name="items">Collection to copy the items from.</param>
    public Set(IEnumerable<TItem> items) 
      : base(items)
    {
      Initialize();
    }

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true" />
    /// </summary>
    /// <param name="items">Collection to copy the items from.</param>
    /// <param name="comparer">Equality comparer for the set type.</param>
    public Set(IEnumerable<TItem> items, IEqualityComparer<TItem> comparer) 
      : base(items, comparer)
    {
      Initialize();
    }
  }
}
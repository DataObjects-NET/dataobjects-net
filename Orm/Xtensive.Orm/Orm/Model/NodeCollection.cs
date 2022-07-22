// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2007.07.30

using System;
using System.Collections.Generic;
using System.Diagnostics;
using Xtensive.Collections;
using Xtensive.Core;

namespace Xtensive.Orm.Model
{
  /// <summary>
  /// A base class for collection of nodes in model.
  /// </summary>
  /// <typeparam name="TNode">The type of the node.</typeparam>
  [Serializable]
  public class NodeCollection<TNode> : CollectionBaseSlim<TNode>
    where TNode: Node
  {
    [NonSerialized, DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private EventHandler<ChangeNotifierEventArgs> itemChangedHandler;

    [NonSerialized, DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private EventHandler<ChangeNotifierEventArgs> itemChangingHandler;

    protected Dictionary<string, TNode> NameIndex = new Dictionary<string, TNode>();
    
    /// <summary>
    /// Gets empty collection.
    /// </summary>
    public readonly static NodeCollection<TNode> Empty;

    /// <summary>
    /// Gets the owner.
    /// </summary>
    public Node Owner { get; private set; }

    /// <summary>
    /// Gets the name of this collection.
    /// </summary>
    public string Name { get; private set; }

    /// <summary>
    /// Gets the full name.
    /// </summary>
    public string FullName {
      get {
        return Owner==null 
          ? Name
          : string.Format(Strings.NodeCollectionFullNameFormat, Owner.Name, Name);
      }
    }

    /// <summary>
    /// Adds new element to the collection.
    /// </summary>
    /// <param name="item">Item to add.</param>
    /// <exception cref="InvalidOperationException">Item already exists.</exception>
    [DebuggerStepThrough]
    public override void Add(TNode item)
    {
      try {
        base.Add(item);
        NameIndex.Add(item.Name, item);
        TrySubscribe(item);
      }
      catch (ArgumentException e){
        throw new InvalidOperationException(
          string.Format(Strings.ExItemWithNameXAlreadyExistsInY, item.Name, FullName), e);
      }
      catch (InvalidOperationException e) {
        throw new InvalidOperationException(
          string.Format(Strings.ExItemWithNameXAlreadyExistsInY, item.Name, FullName), e);
      }
    }

    /// <inheritdoc/>
    public override void AddRange(IEnumerable<TNode> nodes)
    {
      EnsureNotLocked();
      foreach (var node in nodes) {
        Add(node);
      }
    }

    /// <inheritdoc/>
    public override bool Remove(TNode item)
    {
      if (base.Remove(item)) {
        TryUnsubscribe(item);
        NameIndex.Remove(item.Name);
        return true;
      }
      return false;
    }

    /// <inheritdoc/>
    public override void Clear()
    {
      EnsureNotLocked();
      foreach(var item in this) {
        TryUnsubscribe(item);
      }
      base.Clear();
      NameIndex.Clear();
    }

    /// <inheritdoc/>
    public virtual void UpdateState()
    {
      if (this==Empty)
        return;
      EnsureNotLocked();
      foreach (TNode node in this)
        node.UpdateState();
    }

    /// <summary>
    /// Determines whether this instance contains an item with the specified key.
    /// </summary>
    /// <param name="key">The key.</param>
    /// <returns>
    /// <see langword="true"/> if this instance contains the specified key; otherwise, <see langword="false"/>.
    /// </returns>
    [DebuggerStepThrough]
    public bool Contains(string key)
    {
      return NameIndex.ContainsKey(key);
    }


    /// <summary>
    /// Gets the value associated with the specified key.
    /// </summary>
    /// <param name="key">The key of the value to get.</param>
    /// <param name="value"><typeparamref name="TNode"/> if it was found; otherwise <see langword="null"/>.</param>
    /// <returns><see langword="true"/> if value is found by specified <paramref name="key"/>; otherwise <see langword="false"/>.</returns>
    [DebuggerStepThrough]
    public bool TryGetValue(string key, out TNode value)
    {
      return NameIndex.TryGetValue(key, out value);
    }

    /// <summary>
    /// An indexer that provides access to collection items.
    /// Returns <see langword="default(TNode)"/> if there is no such item.
    /// </summary>
    /// <exception cref="ArgumentException">Item was not found.</exception>
    public TNode this[string key]
    {
      [DebuggerStepThrough]
      get {
        TNode result;
        if (!TryGetValue(key, out result))
          throw new KeyNotFoundException(GetExceptionMessage(key));
        return result;
      }
    }

    protected virtual string GetExceptionMessage(string key)
    {
      return string.Format(Strings.ExItemWithKeyXWasNotFound, key);
    }

    /// <summary>
    /// Tries to subscribe the collection on 
    /// change notifications from the specified item.
    /// </summary>
    /// <param name="item">The item to try.</param>
    protected void TrySubscribe(TNode item)
    {
      if (item is IChangeNotifier notifier) {
        notifier.Changing += itemChangingHandler;
        notifier.Changed += itemChangedHandler;
      }
    }

    /// <summary>
    /// Tries to unsubscribe the collection from
    /// change notifications from the specified item.
    /// </summary>
    /// <param name="item">The item to try.</param>
    protected void TryUnsubscribe(TNode item)
    {
      if (item is IChangeNotifier notifier) {
        notifier.Changing -= itemChangingHandler;
        notifier.Changed -= itemChangedHandler;
      }
    }

    protected virtual void OnItemChanging(object sender, ChangeNotifierEventArgs e)
    {
      NameIndex.Remove(((TNode) sender).Name);
    }

    protected virtual void OnItemChanged(object sender, ChangeNotifierEventArgs e)
    {
      var tNode = (TNode)sender;
      NameIndex.Add(tNode.Name, tNode);
    }

    /// <inheritdoc/>
    public override void Lock(bool recursive)
    {
      base.Lock(recursive);
      if (recursive) {
        foreach (var item in this) {
          item.Lock(true);
        }
      }
    }

    // Constructors

    /// <summary>
    /// Initializes a new instance of this class.
    /// </summary>
    /// <param name="owner">The owner.</param>
    /// <param name="name">The name.</param>
    public NodeCollection(Node owner, string name)
    {
      ArgumentValidator.EnsureArgumentNotNullOrEmpty(name, "name");
      Owner = owner;
      Name = name;
      itemChangingHandler = OnItemChanging;
      itemChangedHandler = OnItemChanged;
    }

    // Type initializer
    
    static NodeCollection()
    {
      Empty = new NodeCollection<TNode>(null, "Empty");
      Empty.Lock(false);
    }
  }
}
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

using Xtensive.Notifications;


namespace Xtensive.Orm.Model
{
  /// <summary>
  /// A base class for collection of nodes in model.
  /// </summary>
  /// <typeparam name="TNode">The type of the node.</typeparam>
  [Serializable]
  public class NodeCollection<TNode>
    : CollectionBase<TNode>
    where TNode: Node
  {
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
          : Strings.NodeCollectionFullNameFormat.FormatWith(Owner.Name, Name);
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

    
    public virtual void UpdateState(bool recursive)
    {
      if (this==Empty)
        return;
      this.EnsureNotLocked();
      if (!recursive)
        return;
      foreach (TNode node in this)
        node.UpdateState(true);
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
      get
      {
        TNode result;
        if (!TryGetValue(key, out result))
          throw new ArgumentException(GetExceptionMessage(key));
        return result;
      }
    }

    /// <summary>
    /// Gets the exception message.
    /// </summary>
    /// <param name="key">The key.</param>
    /// <returns></returns>
    protected virtual string GetExceptionMessage(string key)
    {
      return string.Format(Strings.ExItemWithKeyXWasNotFound, key);
    }

    /// <summary>
    /// Called when value is inserted.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <param name="index">The index.</param>
    protected override void OnInserted(TNode value, int index)
    {
      base.OnInserted(value, index);
      NameIndex.Add(value.Name, value);
    }

    /// <summary>
    /// Called when value is removed.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <param name="index">The index.</param>
    protected override void OnRemoved(TNode value, int index)
    {
      base.OnRemoved(value, index);
      NameIndex.Remove(value.Name);
    }

    /// <summary>
    /// Called when this instance is cleared.
    /// </summary>
    protected override void OnCleared()
    {
      base.OnCleared();
      NameIndex.Clear();
    }

    protected override void OnItemChanging(object sender, ChangeNotifierEventArgs e)
    {
      base.OnItemChanging(sender, e);
      NameIndex.Remove(((TNode) sender).Name);
    }

    protected override void OnItemChanged(object sender, ChangeNotifierEventArgs e)
    {
      base.OnItemChanged(sender, e);
      var tNode = (TNode)sender;
      NameIndex.Add(tNode.Name, tNode);
    }

    
    /// <summary>
    /// Locks the instance and (possibly) all dependent objects.
    /// 
    /// </summary>
    /// <param name="recursive"><see langword="True"/> if all dependent objects should be locked as well.</param>
    public override void Lock(bool recursive)
    {
      base.Lock(recursive);
      foreach (var item in Items)
        item.Lock(true);
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
    }

    // Type initializer
    
    static NodeCollection()
    {
      Empty = new NodeCollection<TNode>(null, "Empty");
    }
  }
}
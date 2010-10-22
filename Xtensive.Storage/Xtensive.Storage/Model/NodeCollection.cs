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
using Xtensive.Internals.DocTemplates;
using Xtensive.Notifications;
using Xtensive.Storage.Model.Resources;

namespace Xtensive.Storage.Model
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
    private readonly Dictionary<string, TNode> nameIndex = new Dictionary<string, TNode>();
    
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

    /// <inheritdoc/>
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
      return nameIndex.ContainsKey(key);
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
      return nameIndex.TryGetValue(key, out value);
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
          throw new ArgumentException(
            string.Format(Strings.ExItemWithKeyXWasNotFound, key));
        return result;
      }
    }

    protected override void OnInserted(TNode value, int index)
    {
      base.OnInserted(value, index);
      nameIndex.Add(value.Name, value);
    }

    protected override void OnRemoved(TNode value, int index)
    {
      base.OnRemoved(value, index);
      nameIndex.Remove(value.Name);
    }

    protected override void OnCleared()
    {
      base.OnCleared();
      nameIndex.Clear();
    }

    protected override void OnItemChanging(object sender, ChangeNotifierEventArgs e)
    {
      base.OnItemChanging(sender, e);
      nameIndex.Remove(((TNode) sender).Name);
    }

    protected override void OnItemChanged(object sender, ChangeNotifierEventArgs e)
    {
      base.OnItemChanged(sender, e);
      var tNode = (TNode)sender;
      nameIndex.Add(tNode.Name, tNode);
    }

    /// <inheritdoc/>
    public override void Lock(bool recursive)
    {
      base.Lock(recursive);
      foreach (var item in Items)
        item.Lock(true);
    }


    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
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
// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2009.03.16

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using Xtensive.Core;
using Xtensive.Core.Collections;
using Xtensive.Core.Internals.DocTemplates;
using Xtensive.Core.Notifications;
using Xtensive.Indexing.Storage.Model;
using Xtensive.Indexing.Storage.Resources;

namespace Xtensive.Storage.Model
{
  /// <summary>
  /// A base class for collection of nodes in model.
  /// </summary>
  /// <typeparam name="TNode">The type of the node.</typeparam>
  /// <typeparam name="TParent">The type of the parent.</typeparam>
  [Serializable]
  public class NodeCollection<TNode, TParent>
    : CollectionBase<TNode>
    where TNode: Node
    where TParent: Node
  {
    private readonly Dictionary<string, TNode> nameIndex = new Dictionary<string, TNode>();
    
    /// <summary>
    /// Gets empty collection.
    /// </summary>
    public readonly static NodeCollection<TNode,TParent> Empty;

    /// <summary>
    /// Gets the parent node collection belongs to.
    /// </summary>
    public TParent Parent
    {
      [DebuggerStepThrough]
      get; 
      [DebuggerStepThrough]
      private set;
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
          string.Format(CultureInfo.InvariantCulture, Strings.ExItemWithNameXAlreadyExists, item.Name), e);
      }
      catch (InvalidOperationException e) {
        throw new InvalidOperationException(
          string.Format(CultureInfo.InvariantCulture, Strings.ExItemWithNameXAlreadyExists, item.Name), e);
      }
    }

    /// <inheritdoc/>
    public override void Lock(bool recursive)
    {
      base.Lock(recursive);
      if (recursive)
        foreach (TNode node in this)
          node.Lock(recursive);
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
    /// <exception cref="ArgumentException"> when item was not found.</exception>
    public TNode this[string key]
    {
      [DebuggerStepThrough]
      get
      {
        TNode result;
        if (!TryGetValue(key, out result))
          throw new ArgumentException(String.Format(String.Format("Item by key ='{0}' was not found.", key)));
        return result;
      }
    }

    /// <inheritdoc/>
    protected override void OnInserted(TNode value, int index)
    {
      base.OnInserted(value, index);
      nameIndex.Add(value.Name, value);
    }

    /// <inheritdoc/>
    protected override void OnRemoved(TNode value, int index)
    {
      base.OnRemoved(value, index);
      nameIndex.Remove(value.Name);
    }

    /// <inheritdoc/>
    protected override void OnCleared()
    {
      base.OnCleared();
      nameIndex.Clear();
    }

    /// <inheritdoc/>
    protected override void OnItemChanging(object sender, ChangeNotifierEventArgs e)
    {
      base.OnItemChanging(sender, e);
      nameIndex.Remove(((TNode) sender).Name);
    }

    /// <inheritdoc/>
    protected override void OnItemChanged(object sender, ChangeNotifierEventArgs e)
    {
      base.OnItemChanged(sender, e);
      var tNode = (TNode)sender;
      nameIndex.Add(tNode.Name, tNode);
    }

    /// <inheritdoc/>
    /// <exception cref="ArgumentException">Invalid <paramref name="value"/>.Parent value.</exception>
    protected override void OnValidate(TNode value)
    {
      base.OnValidate(value);
      if (Parent!=null && value.Parent!=Parent)
        throw new ArgumentException(Strings.ExInvalidParentValue, "value.Parent");
    }


    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="parent"><see cref="Parent"/> property value.</param>
    public NodeCollection(TParent parent)
    {
      ArgumentValidator.EnsureArgumentNotNull(parent, "parent");
      Parent = parent;
    }

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    public NodeCollection()
    {
      Parent = null;
    }

    // Type initializer
    
    static NodeCollection()
    {
      Empty = new NodeCollection<TNode, TParent>();
      Empty.Lock();
    }
  }
}


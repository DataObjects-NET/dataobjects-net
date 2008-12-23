// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2007.07.30

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using Xtensive.Core.Collections;
using Xtensive.Core.Notifications;

namespace Xtensive.Storage.Model
{
  [Serializable]
  public class NodeCollection<TNode>
    : CollectionBase<TNode>
    where TNode: Node
  {
    private readonly Dictionary<string, TNode> nameIndex = new Dictionary<string, TNode>();
    public readonly static NodeCollection<TNode> Empty;

    /// <summary>
    /// Adds new element to the collection.
    /// </summary>
    /// <param name="item">Item to add.</param>
    [DebuggerStepThrough]
    public override void Add(TNode item)
    {
      try {
        base.Add(item);
      }
      catch (ArgumentException e){
        throw new InvalidOperationException(
          string.Format(CultureInfo.InvariantCulture, "Item with name '{0}' already exists.", item.Name), e);
      }
      catch (InvalidOperationException e) {
        throw new InvalidOperationException(
          string.Format(CultureInfo.InvariantCulture, "Item with name '{0}' already exists.", item.Name), e);
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


    // Constructors

    /// <summary>
    /// Initializes a new instance of the <see cref="NodeCollection&lt;TNode&gt;"/> class.
    /// </summary>
    public NodeCollection()
    {
    }

    static NodeCollection()
    {
      Empty = new NodeCollection<TNode>();
      Empty.Lock();
    }
  }
}
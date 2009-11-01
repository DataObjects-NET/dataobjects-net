// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2007.07.30

using System;
using System.Globalization;
using Xtensive.Core.Collections;
using Xtensive.Core.Comparison;
using Xtensive.Indexing;

namespace Xtensive.Storage.Model
{
  [Serializable]
  public class NodeCollection<TNode>
    : CollectionBase<TNode>
    where TNode: Node
  {
    private readonly IUniqueIndex<string, TNode> nameIndex;

    public static NodeCollection<TNode> Empty;

    /// <summary>
    /// Adds new element to the collection.
    /// </summary>
    /// <param name="item">Item to add.</param>
    public override void Add(TNode item)
    {
      try {
        base.Add(item);
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
    public bool Contains(string key)
    {
      return nameIndex.ContainsKey(key);
    }

    /// <summary>
    /// Gets the value associated with the specified key.
    /// </summary>
    /// <param name="key">The key of the value to get.</param>
    /// <param name="value">When this method returns, contains the value 
    /// associated with the specified key, if the key is found; otherwise, 
    /// the default value for the type of the value parameter. 
    /// This parameter is passed uninitialized.</param>
    /// <returns></returns>
    public bool TryGetValue(string key, out TNode value)
    {
      value = null;
      if (!Contains(key))
        return false;
      value = nameIndex.GetItem(key);
      return true;
    }

    /// <summary>
    /// An indexer that provides access to collection items.
    /// Returns <see langword="default(TNode)"/> if there is no such item.
    /// </summary>
    public TNode this[string key]
    {
      get
      {
        TNode result;
        if (!TryGetValue(key, out result))
          throw new ArgumentException(String.Format(String.Format("Item '{0}' not found.", key)));
        return result;
      }
    }


    // Constructors

    /// <summary>
    /// Initializes a new instance of the <see cref="NodeCollection&lt;TNode&gt;"/> class.
    /// </summary>
    public NodeCollection()
    {
      IndexConfiguration<string, TNode> config = new IndexConfiguration<string, TNode>(delegate (TNode node) { return node.Name; }, AdvancedComparer<string>.Default);
      IUniqueIndex<string, TNode> implementation = IndexFactory.CreateUnique<string, TNode, DictionaryIndex<string, TNode>>(config);
      nameIndex = new CollectionIndex<string, TNode>("nameIndex", this, implementation);
    }

    static NodeCollection()
    {
      Empty = new NodeCollection<TNode>();
      Empty.Lock();
    }
  }
}
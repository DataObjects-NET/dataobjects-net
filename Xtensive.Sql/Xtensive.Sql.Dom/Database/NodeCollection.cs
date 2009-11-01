// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.

using System;
using System.Collections.Generic;
using Xtensive.Core;
using Xtensive.Core.Collections;
using Xtensive.Core.Comparison;
using Xtensive.Core.Helpers;
using Xtensive.Indexing;

namespace Xtensive.Sql.Dom.Database
{
  /// <summary>
  /// Represents lockable indexed by <see cref="Node.Name"/> collection of <see cref="Node"/>s.
  /// </summary>
  /// <typeparam name="TNode">Node type</typeparam>
  [Serializable]
  public class NodeCollection<TNode>: CollectionBase<TNode>,
    ILockable
    where TNode: Node
  {
    private bool isLocked;
    private IUniqueIndex<string, TNode> nameIndex;

    /// <summary>
    /// Gets a value indicating whether this instance is read-only.
    /// </summary>
    /// <value></value>
    /// <returns><see langword="True"/> if this instance is read-only; otherwise, <see langword="false"/>.</returns>
    public override bool IsReadOnly
    {
      get { return IsLocked ? true : base.IsReadOnly; }
    }

    /// <summary>
    /// Performs additional custom processes when changing the contents of the
    /// collection instance.
    /// </summary>
    protected override void OnChanging()
    {
      this.EnsureNotLocked();
    }

    #region IHasUniqueIndex<string,TNode> Members

    /// <summary>
    /// Gets the <see typeparam name="T"/> at the specified index.
    /// </summary>
    public TNode this[string index]
    {
      get
      {
        if (string.IsNullOrEmpty(index))
          return default(TNode);
        return (nameIndex.ContainsKey(index)) ? nameIndex.GetItem(index) : default(TNode);
      }
    }

    #endregion

    #region Constructors

    /// <summary>
    /// Initializes a new instance of the <see cref="NodeCollection{T}"/> class.
    /// </summary>
    public NodeCollection()
      : this(0)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="NodeCollection{T}"/> class.
    /// </summary>
    /// <param name="capacity">The initial collection capacity.</param>
    public NodeCollection(int capacity)
      : base(capacity)
    {
      IndexConfiguration<string, TNode> config = new IndexConfiguration<string, TNode>(delegate (TNode node) { return node.Name; }, AdvancedComparer<string>.Default);
      IUniqueIndex<string, TNode> implementation = IndexFactory.CreateUnique<string, TNode, DictionaryIndex<string, TNode>>(config);
      nameIndex = new CollectionIndex<string, TNode>("nameIndex", this, implementation);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="NodeCollection{T}"/> class.
    /// </summary>
    /// <param name="collection">The collection whose elements are copied to the new list.</param>
    public NodeCollection(IEnumerable<TNode> collection)
      : base(collection)
    {
      IndexConfiguration<string, TNode> config = new IndexConfiguration<string, TNode>(delegate (TNode node) { return node.Name; }, AdvancedComparer<string>.Default);
      IUniqueIndex<string, TNode> implementation = IndexFactory.CreateUnique<string, TNode, DictionaryIndex<string, TNode>>(config);
      nameIndex = new CollectionIndex<string, TNode>("nameIndex", this, implementation);
    }

    #endregion
  }
}
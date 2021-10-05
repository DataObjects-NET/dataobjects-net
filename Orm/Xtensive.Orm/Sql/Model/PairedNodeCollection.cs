// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.

using System;
using System.Collections;
using System.Collections.Generic;
using Xtensive.Core;

namespace Xtensive.Sql.Model
{
  /// <summary>
  /// Represents paired collection of <see cref="Node"/>s.
  /// </summary>
  /// <typeparam name="TOwner">Owner node type</typeparam>
  /// <typeparam name="TNode">Item node type</typeparam>
  [Serializable]
  public class PairedNodeCollection<TOwner, TNode>: NodeCollection<TNode>
    where TOwner : Node
    where TNode : Node, IPairedNode<TOwner>
  {
    private readonly TOwner owner;
    private readonly string property;

    /// <summary>
    /// Gets a value indicating whether the <see cref="IList"/> is read-only.
    /// </summary>
    /// <value></value>
    /// <returns><see langword="True"/> if the <see cref="IList"/> is read-only; otherwise, <see langword="false"/>.</returns>
    public override bool IsReadOnly
    {
      get { return IsLocked ? true : base.IsReadOnly; }
    }

    /// <inheritdoc/>
    public override void Add(TNode item)
    {
      base.Add(item);
      item.UpdatePairedProperty(property, owner);
    }

    /// <inheritdoc/>
    public override void AddRange(IEnumerable<TNode> items)
    {
      foreach (var item in items) {
        Add(item);
      }
    }

    /// <inheritdoc/>
    public override bool Remove(TNode item)
    {
      bool result = base.Remove(item);
      item.UpdatePairedProperty(property, null);
      return result;
    }

    /// <inheritdoc/>
    public override void Clear()
    {
      foreach (var item in this) {
        item.UpdatePairedProperty(property, null);
      }
      base.Clear();
    }

    #region Constructors

    /// <summary>
    /// Initializes a new instance of the <see cref="PairedNodeCollection{TOwner,TNode}"/> class.
    /// </summary>
    /// <param name="owner">The collectionowner.</param>
    /// <param name="property">Owner collection property.</param>
    public PairedNodeCollection(TOwner owner, string property)
      : this(owner, property, 0)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="PairedNodeCollection{TOwner,TNode}"/> class.
    /// </summary>
    /// <param name="owner">The collection owner.</param>
    /// <param name="property">Owner collection property.</param>
    /// <param name="capacity">The initial collection capacity.</param>
    public PairedNodeCollection(TOwner owner, string property, int capacity)
      : base(capacity)
    {
      ArgumentValidator.EnsureArgumentNotNull(owner, "owner");
      ArgumentValidator.EnsureArgumentNotNullOrEmpty(property, "property");
      this.owner = owner;
      this.property = property;
    }

    #endregion
  }
}
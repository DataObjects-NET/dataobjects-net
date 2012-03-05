// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using Xtensive.Collections;
using Xtensive.Internals.DocTemplates;

namespace Xtensive.Sql.Model
{
  /// <summary>
  /// Represents lockable indexed by <see cref="Node.Name"/> collection of <see cref="Node"/>s.
  /// </summary>
  /// <typeparam name="TNode">Node type</typeparam>
  [Serializable]
  [DebuggerDisplay("Count = {Count}")]
  public class NodeCollection<TNode>: CollectionBaseSlim<TNode>
    where TNode: Node
  {
    private readonly IDictionary<string, TNode> nameIndex;

    /// <summary>
    /// Gets a value indicating whether this instance is read-only.
    /// </summary>
    /// <value></value>
    /// <returns><see langword="True"/> if this instance is read-only; otherwise, <see langword="false"/>.</returns>
    public override bool IsReadOnly { get { return IsLocked || base.IsReadOnly; } }

    /// <inheritdoc/>
    public override void Add(TNode item)
    {
      base.Add(item);
      if (!string.IsNullOrEmpty(item.Name))
        nameIndex.Add(item.Name, item);
    }

    public override bool Remove(TNode item)
    {
      bool result = base.Remove(item);
      if (result)
        nameIndex.Remove(item.Name);
      return result;
    }

    public override void Clear()
    {
      base.Clear();
      nameIndex.Clear();
    }
    
    /// <summary>
    /// Gets the <typeparamref name="TNode"/> at the specified index.
    /// </summary>
    public TNode this[string index] {
      get {
        if (string.IsNullOrEmpty(index))
          return default(TNode);
        TNode result;
        return nameIndex.TryGetValue(index, out result) ? result : default(TNode);
      }
    }


    // Constructors
    
    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    public NodeCollection()
      : this(0)
    {
    }

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="capacity">The initial collection capacity.</param>
    public NodeCollection(int capacity)
      : base(capacity)
    {
      nameIndex = new Dictionary<string, TNode>(capacity, StringComparer.OrdinalIgnoreCase);
    }
  }
}
// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Xtensive.Core.Collections;
using Xtensive.Core.Helpers;
using Xtensive.Core.Internals.DocTemplates;

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
    public override bool IsReadOnly { get { return IsLocked ? true : base.IsReadOnly; } }

    /// <inheritdoc/>
    public override void Add(TNode item)
    {
      base.Add(item);
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
    /// Gets the <see typeparam name="T"/> at the specified index.
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
      nameIndex = new Dictionary<string, TNode>(0, StringComparer.OrdinalIgnoreCase);
    }

    /// <summary>
    ///	<see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="capacity">The initial collection capacity.</param>
    public NodeCollection(int capacity)
      : base(capacity)
    {
      nameIndex = new Dictionary<string, TNode>(capacity, StringComparer.OrdinalIgnoreCase);
    }
    
    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="collection">The collection whose elements are copied to the new list.</param>
    public NodeCollection(IEnumerable<TNode> collection)
      : base(collection)
    {
      nameIndex = new Dictionary<string, TNode>(StringComparer.OrdinalIgnoreCase);
      foreach (var item in collection)
        nameIndex.Add(item.Name, item);
    }
  }
}
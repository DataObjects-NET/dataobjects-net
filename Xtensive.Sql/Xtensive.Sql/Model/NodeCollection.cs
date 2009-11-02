// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.

using System;
using System.Collections.Generic;
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
  public class NodeCollection<TNode>: CollectionBase<TNode>
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
    protected override void OnChanging()
    {
      this.EnsureNotLocked();
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
      nameIndex = new Dictionary<string, TNode>(0);
    }

    /// <summary>
    ///	<see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="capacity">The initial collection capacity.</param>
    public NodeCollection(int capacity)
      : base(capacity)
    {
      nameIndex = new Dictionary<string, TNode>(capacity);
    }


    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="collection">The collection whose elements are copied to the new list.</param>
    public NodeCollection(IEnumerable<TNode> collection)
      : base(collection)
    {
      nameIndex = collection.ToDictionary(item => item.Name);
    }
  }
}
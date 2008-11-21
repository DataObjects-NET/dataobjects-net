// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2008.08.01

using System;
using System.Collections.Generic;
using System.Diagnostics;
using Xtensive.Core.Collections;
using Xtensive.Core.Internals.DocTemplates;

namespace Xtensive.Storage.Model
{
  /// <summary>
  /// Describes a group of columns that belongs to the specified <see cref="HierarchyInfoRef"/>.
  /// </summary>
  [Serializable]
  [DebuggerDisplay("Hierarchy = {HierarchyInfoRef}, Keys = {Keys}, Columns = {Columns}")]
  public sealed class ColumnGroup
  {
    /// <summary>
    /// Gets the <see cref="Model.HierarchyInfoRef"/> pointing to <see cref="HierarchyInfo"/>
    /// this column group belongs to.
    /// </summary>
    public HierarchyInfoRef HierarchyInfoRef { get; private set; }

    /// <summary>
    /// Gets the indexes of key columns.
    /// </summary>
    public ReadOnlyList<int> Keys { get; private set; }

    /// <summary>
    /// Gets the indexes of other columns.
    /// </summary>
    public ReadOnlyList<int> Columns { get; private set; }


    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="hierarchy">The hierarchy.</param>
    /// <param name="keys">The keys.</param>
    /// <param name="columns">The columns.</param>
    public ColumnGroup(HierarchyInfoRef hierarchy, IEnumerable<int> keys, IEnumerable<int> columns)
      : this(hierarchy, new List<int>(keys), new List<int>(columns))
    {
    }

    /// <summary>
    ///   <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="hierarchy">The hierarchy.</param>
    /// <param name="keys">The keys.</param>
    /// <param name="columns">The columns.</param>
    public ColumnGroup(HierarchyInfoRef hierarchy, IList<int> keys, IList<int> columns)
    {
      HierarchyInfoRef = hierarchy;
      Keys = new ReadOnlyList<int>(keys);
      Columns = new ReadOnlyList<int>(columns);
    }
  }
}
// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Kofman
// Created:    2008.08.07

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Xtensive.Core;
using Xtensive.Orm.Model;

namespace Xtensive.Orm.Rse
{
  /// <summary>
  /// Read only collection of the <see cref="ColumnGroup"/> instances.
  /// </summary>
  [Serializable]
  public class ColumnGroupCollection : IReadOnlyList<ColumnGroup>
  {
    private readonly IReadOnlyList<ColumnGroup> items;

    private static Lazy<ColumnGroupCollection> cachedEmpty =
      new Lazy<ColumnGroupCollection>(() => new ColumnGroupCollection(Array.Empty<ColumnGroup>()));

    /// <inheritdoc/>
    public int Count => items.Count;

    /// <summary>
    /// Gets the <see cref="ColumnGroup"/> by specified group index.
    /// </summary>
    public ColumnGroup this[int groupIndex] => items[groupIndex];

    /// <inheritdoc/>
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    /// <inheritdoc/>
    public IEnumerator<ColumnGroup> GetEnumerator() => items.GetEnumerator();

    /// <summary>
    /// Gets the empty <see cref="ColumnGroupCollection"/>.
    /// </summary>    
    public static ColumnGroupCollection Empty {
      [DebuggerStepThrough]
      get {
        return cachedEmpty.Value;
      }
    }

    // Constructors

    /// <summary>
    /// Initializes a new instance of this class.
    /// </summary>
    /// <param name="items">The collection items.</param>
    public ColumnGroupCollection(IEnumerable<ColumnGroup> items)
    {
      this.items = items.ToList();
    }

    /// <summary>
    /// Initializes a new instance of this class.
    /// </summary>
    /// <param name="items">The collection items.</param>
    public ColumnGroupCollection(IReadOnlyList<ColumnGroup> items)
    {
      this.items = items;
    }
  }
}

// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Kofman
// Created:    2008.08.07

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Xtensive.Collections;
using Xtensive.Core;
using Xtensive.Internals.DocTemplates;
using Xtensive.Orm.Model;
using Xtensive.Threading;
using Xtensive.Storage.Model;
using Xtensive.Storage.Rse.Resources;

namespace Xtensive.Storage.Rse
{
  /// <summary>
  /// Read only collection of <see cref="ColumnGroup"/>.
  /// </summary>
  [Serializable]
  public class ColumnGroupCollection : ReadOnlyCollection<ColumnGroup>
  {
    private static ThreadSafeCached<ColumnGroupCollection> cachedEmpty =
      ThreadSafeCached<ColumnGroupCollection>.Create(new object());

    /// <summary>
    /// Gets the <see cref="ColumnGroup"/> by specified group index.
    /// </summary>
    public ColumnGroup this[int groupIndex]
    {
      get
      {
        return this.ElementAt(groupIndex);
      }
    }

    /// <summary>
    /// Gets the empty <see cref="ColumnGroupCollection"/>.
    /// </summary>    
    public static ColumnGroupCollection Empty {
      [DebuggerStepThrough]
      get {
        return cachedEmpty.GetValue(
          () => new ColumnGroupCollection(Enumerable.Empty<ColumnGroup>()));
      }
    }

    /// <summary>
    /// Gets the index of the group by provided <paramref name="segment"/>.
    /// </summary>
    /// <param name="segment">Segment of record' columns.</param>
    public int GetGroupIndexBySegment(Segment<int> segment)
    {
      int index = 0;
      foreach (var columnGroup in this) {
        Func<int, bool> predicate = i => i >= segment.Offset && i < segment.EndOffset;
        if (columnGroup.Keys.Any(predicate))
          return index;
        if (columnGroup.Columns.Any(predicate))
          return index;
        index++;
      }
      throw new InvalidOperationException(Strings.ExColumnGroupCouldNotBeFound);
    } 

    
    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="items">The collection items.</param>
    public ColumnGroupCollection(IEnumerable<ColumnGroup> items)
      : base(items.ToList())
    {      
    }
  }
}
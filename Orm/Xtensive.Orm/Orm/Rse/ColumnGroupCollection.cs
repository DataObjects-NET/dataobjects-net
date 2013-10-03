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
using Xtensive.Orm.Model;

namespace Xtensive.Orm.Rse
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


    // Constructors

    /// <summary>
    /// Initializes a new instance of this class.
    /// </summary>
    /// <param name="items">The collection items.</param>
    public ColumnGroupCollection(IEnumerable<ColumnGroup> items)
      : base(items.ToList())
    {      
    }
  }
}
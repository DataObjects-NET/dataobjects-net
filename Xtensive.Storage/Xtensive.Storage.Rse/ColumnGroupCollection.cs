// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Kofman
// Created:    2008.08.07

using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Xtensive.Core.Collections;
using Xtensive.Core.Internals.DocTemplates;

namespace Xtensive.Storage.Rse
{
  /// <summary>
  /// Read only collection of <see cref="ColumnGroup"/>.
  /// </summary>
  public class ColumnGroupCollection : ReadOnlyCollection<ColumnGroup>
  {
    private static ColumnGroupCollection empty;

    /// <summary>
    /// Gets the empty <see cref="ColumnGroupCollection"/>.
    /// </summary>    
    public static ColumnGroupCollection Empty {
      [DebuggerStepThrough]
      get {
        if (empty==null)
          empty = new ColumnGroupCollection(Enumerable.Empty<ColumnGroup>());
        return empty;
      }
    }

    /// <summary>
    ///   <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="items">The collection items.</param>
    public ColumnGroupCollection(IEnumerable<ColumnGroup> items)
      : base(items.ToList())
    {      
    }
  }
}
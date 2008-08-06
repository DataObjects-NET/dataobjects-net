// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Nick Svetlov
// Created:    2008.07.10

using System.Collections.Generic;
using System.Diagnostics;
using Xtensive.Core.Collections;
using Xtensive.Core.Internals.DocTemplates;

namespace Xtensive.Storage.Rse
{
  /// <summary>
  /// Describes how a group of columns associated with a key mapped on <see cref="RecordSetHeader"/>.
  /// </summary>
  public struct RecordColumnGroupMapping
  {
    private readonly IEnumerable<int> keys;
    private readonly IEnumerable<int> columns;

    /// <summary>
    /// Gets indexes of key columns this instance associated with.
    /// </summary>
    public IEnumerable<int> Keys
    {
      [DebuggerStepThrough]
      get { return keys; }
    }

    /// <summary>
    /// Gets indexes of current <see cref="RecordColumnGroupMapping"/> columns.
    /// </summary>
    public IEnumerable<int> Columns
    {
      [DebuggerStepThrough]
      get { return columns; }
    }


    // Constructors

    /// <summary>
    ///   <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    public RecordColumnGroupMapping(IEnumerable<int> keys, IEnumerable<int> columns)
    {
      this.keys = new ReadOnlyList<int>(new List<int>(keys));
      this.columns = new ReadOnlyList<int>(new List<int>(columns));
    }
  }
}
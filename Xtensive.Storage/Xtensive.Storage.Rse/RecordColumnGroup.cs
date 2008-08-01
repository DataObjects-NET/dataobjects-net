// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Nick Svetlov
// Created:    2008.07.10

using System.Collections.Generic;
using System.Diagnostics;
using Xtensive.Core.Internals.DocTemplates;

namespace Xtensive.Storage.Rse
{
  /// <summary>
  /// Describes a group of columns associated with a key.
  /// </summary>
  public struct RecordColumnGroup
  {
    private readonly IEnumerable<int> keyColumnIndexes;
    private readonly IEnumerable<int> columnIndexes;

    /// <summary>
    /// Gets indexes of key columns
    /// </summary>
    public IEnumerable<int> KeyColumnIndexes
    {
      [DebuggerStepThrough]
      get { return keyColumnIndexes; }
    }

    /// <summary>
    /// Gets indexes of current <see cref="RecordColumnGroup"/> columns.
    /// </summary>
    public IEnumerable<int> ColumnIndexes
    {
      [DebuggerStepThrough]
      get { return columnIndexes; }
    }

    // Constructors

    /// <summary>
    ///   <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    public RecordColumnGroup(IEnumerable<int> keyColumnIndexes, IEnumerable<int> columnIndexes)
    {
      this.keyColumnIndexes = keyColumnIndexes;
      this.columnIndexes = columnIndexes;
    }
  }
}
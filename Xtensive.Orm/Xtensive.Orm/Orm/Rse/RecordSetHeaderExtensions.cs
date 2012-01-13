// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2008.08.12

using System;
using System.Collections.Generic;
using System.Linq;
using Xtensive.Core;
using Xtensive.Collections;
using Xtensive.Tuples;
using Tuple = Xtensive.Tuples.Tuple;
using IndexInfo = Xtensive.Orm.Model.IndexInfo;
using Xtensive.Storage.Model;

namespace Xtensive.Orm.Rse
{
  /// <summary>
  /// <see cref="RecordSetHeader"/> related extension methods.
  /// </summary>
  public static class RecordSetHeaderExtensions
  {
    /// <summary>
    /// Gets index of column with specified name.
    /// </summary>
    /// <param name="header">The header to search in.</param>
    /// <param name="columnName">Name of the column to get.</param>
    /// <returns>Index of the specified column.
    /// <see langword="-1" />, if there is no column with specified name.</returns>
    public static int IndexOf(this RecordSetHeader header, string columnName)
    {
      var column = (MappedColumn) header.Columns[columnName];
      return column==null ? -1 : column.Index;
    }

    /// <summary>
    /// Gets the <see cref="RecordSetHeader"/> object for the specified <paramref name="indexInfo"/>.
    /// </summary>
    /// <param name="indexInfo">The index info to get the header for.</param>
    /// <returns>The <see cref="RecordSetHeader"/> object.</returns>
    public static RecordSetHeader GetRecordSetHeader(this IndexInfo indexInfo)
    {
      return RecordSetHeader.GetHeader(indexInfo);
    }
  }
}
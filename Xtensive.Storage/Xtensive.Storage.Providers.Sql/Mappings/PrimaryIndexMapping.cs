// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.09.23

using System;
using System.Collections.Generic;
using Xtensive.Sql.Model;
using Xtensive.Sql;
using Xtensive.Storage.Model;

namespace Xtensive.Storage.Providers.Sql.Mappings
{
  /// <summary>
  /// Defines primary index mapping.
  /// </summary>
  [Serializable]
  public sealed class PrimaryIndexMapping
  {
    #region Primary index mapping

    public IndexInfo PrimaryIndex { get; private set; }

    public Table Table { get; private set; }

    #endregion

    #region Secondary index mappings

    private readonly Dictionary<IndexInfo, SecondaryIndexMapping> indexMappings = new Dictionary<IndexInfo, SecondaryIndexMapping>();

    internal SecondaryIndexMapping RegisterMapping(IndexInfo indexInfo, Index index)
    {
      SecondaryIndexMapping result = new SecondaryIndexMapping(indexInfo, index);
      indexMappings[indexInfo] = result;
      return result;
    }

    public SecondaryIndexMapping this[IndexInfo indexInfo]
    {
      get
      {
        SecondaryIndexMapping result;
        indexMappings.TryGetValue(indexInfo, out result);
        return result;
      }
    }

    #endregion

    #region Column mappings

    private readonly Dictionary<ColumnInfo, ColumnMapping> columnMappings = new Dictionary<ColumnInfo, ColumnMapping>();

    internal ColumnMapping RegisterMapping(ColumnInfo columnInfo, TableColumn column, TypeMapping typeMapping)
    {
      ColumnMapping result = new ColumnMapping(columnInfo, column, typeMapping);
      columnMappings[columnInfo] = result;
      return result;
    }

    public ColumnMapping this[ColumnInfo columnInfo]
    {
      get
      {
        ColumnMapping result;
        columnMappings.TryGetValue(columnInfo, out result);
        return result;
      }
    }

    #endregion


    // Constructors

    internal PrimaryIndexMapping(IndexInfo index, Table table)
    {
      PrimaryIndex = index;
      Table = table;
    }
  }
}
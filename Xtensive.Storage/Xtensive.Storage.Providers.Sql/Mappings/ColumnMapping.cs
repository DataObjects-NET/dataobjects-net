// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.09.23

using System;
using Xtensive.Sql.Dom.Database;
using Xtensive.Storage.Model;

namespace Xtensive.Storage.Providers.Sql.Mappings
{
  [Serializable]
  public sealed class ColumnMapping
  {
    public ColumnInfo ColumnInfo { get; private set; }

    public TableColumn Column { get; private set; }

    public DataTypeMapping TypeMapping { get; private set; }


    // Constructors

    internal ColumnMapping(ColumnInfo columnInfo, TableColumn column, DataTypeMapping typeMapping)
    {
      ColumnInfo = columnInfo;
      Column = column;
      TypeMapping = typeMapping;
    }
  }
}
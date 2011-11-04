// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.09.23

using System;
using System.Diagnostics;
using Xtensive.Sql.Model;
using Xtensive.Sql;
using Xtensive.Storage.Model;
using ColumnInfo = Xtensive.Orm.Model.ColumnInfo;

namespace Xtensive.Storage.Providers.Sql.Mappings
{
  /// <summary>
  /// Defines column mapping.
  /// </summary>
  [Serializable]
  [DebuggerDisplay("ColumnInfo = {ColumnInfo}")]
  public sealed class ColumnMapping
  {
    public ColumnInfo ColumnInfo { get; private set; }

    public TableColumn Column { get; private set; }

    public TypeMapping TypeMapping { get; private set; }
    
    // Constructors

    internal ColumnMapping(ColumnInfo columnInfo, TableColumn column, TypeMapping typeMapping)
    {
      ColumnInfo = columnInfo;
      Column = column;
      TypeMapping = typeMapping;
    }
  }
}
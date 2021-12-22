// Copyright (C) 2012 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2012.06.05

using System;
using System.Data.SQLite;

namespace Xtensive.Sql.Drivers.Sqlite.Collations
{
  /// <summary>
  /// Provides custom implementation of the ordinal collation.
  /// </summary>
  [SQLiteFunction(FuncType = FunctionType.Collation, Name = "StringComparer_Ordinal")]
  public class OrdinalCollation : SQLiteFunction
  {
    /// <inheritdoc/>
    public override int Compare(string param1, string param2)
    {
      return StringComparer.Ordinal.Compare(param1, param2);
    }
  }
}
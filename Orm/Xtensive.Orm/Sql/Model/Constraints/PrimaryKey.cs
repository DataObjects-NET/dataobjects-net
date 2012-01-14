// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.

using System;

namespace Xtensive.Sql.Model
{
  /// <summary>
  /// Represents primary key constraint.
  /// </summary>
  [Serializable]
  public class PrimaryKey
    : UniqueConstraint
  {
    internal PrimaryKey(Table table, string name, params TableColumn[] columns)
      : base(table, name, columns)
    {
    }
  }
}
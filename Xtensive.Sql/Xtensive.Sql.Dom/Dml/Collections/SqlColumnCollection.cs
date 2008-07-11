// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Xtensive.Core;

namespace Xtensive.Sql.Dom.Dml
{
  /// <summary>
  /// Represents collection of <see cref="SqlColumn"/>s.
  /// </summary>
  [Serializable]
  public class SqlColumnCollection: Collection<SqlColumn>
  {
    public SqlColumn this[string name]
    {
      get
      {
        if (string.IsNullOrEmpty(name))
          return null;
        foreach (SqlColumn column in this) {
          if (column.Name == name)
            return column;
        }
        return null;
      }
      set { throw new NotSupportedException(); }
    }

    public bool IsReadOnly
    {
      get { return false; }
    }

    public void Add(SqlColumn item, string alias)
    {
      base.Add(Sql.ColumnRef(item, alias));
    }

    public void Add(SqlExpression expression)
    {
      if (expression is SqlColumn)
        base.Add(expression as SqlColumn);
      else
        base.Add(Sql.ColumnRef(Sql.Column(expression)));
    }

    public void Add(SqlExpression expression, string alias)
    {
      ArgumentValidator.EnsureArgumentNotNull(alias, "alias");
      base.Add(Sql.ColumnRef(Sql.Column(expression), alias));
    }

    public void AddRange(params SqlColumn[] columns)
    {
      ArgumentValidator.EnsureArgumentNotNull(columns, "columns");
      foreach (SqlColumn c in columns) {
        ArgumentValidator.EnsureArgumentNotNull(c, "column");
        base.Add(c);
      }
    }

    public void AddRange(IEnumerable<SqlColumn> columns)
    {
      ArgumentValidator.EnsureArgumentNotNull(columns, "columns");
      foreach (SqlColumn c in columns)
        base.Add(c);
    }
  }
}
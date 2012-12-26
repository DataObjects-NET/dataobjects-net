// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Xtensive.Core;
using Xtensive.Internals.DocTemplates;

namespace Xtensive.Sql.Dml
{
  /// <summary>
  /// Represents collection of <see cref="SqlColumn"/>s.
  /// </summary>
  [Serializable]
  public class SqlColumnCollection : Collection<SqlColumn>
  {
    private static readonly StringComparer Comparer = StringComparer.OrdinalIgnoreCase;

    public SqlColumn this[string name]
    {
      get
      {
        if (string.IsNullOrEmpty(name))
          return null;
        return this.FirstOrDefault(column => Comparer.Equals(column.Name, name));
      }
      set { throw new NotSupportedException(); }
    }

    public bool IsReadOnly
    {
      get { return false; }
    }

    public void Add(SqlColumn item, string alias)
    {
      base.Add(SqlDml.ColumnRef(item, alias));
    }

    public void Add(SqlExpression expression)
    {
      if (expression is SqlColumn)
        base.Add(expression as SqlColumn);
      else
        base.Add(SqlDml.ColumnRef(SqlDml.Column(expression)));
    }

    public void Add(SqlExpression expression, string alias)
    {
      ArgumentValidator.EnsureArgumentNotNull(alias, "alias");
      base.Add(SqlDml.ColumnRef(SqlDml.Column(expression), alias));
    }

    public void Add(SqlColumnRef columnReference)
    {
      ArgumentValidator.EnsureArgumentNotNull(columnReference, "columnReference");
      base.Add(columnReference);
    }

    public void Insert(int index, SqlExpression expression, string alias)
    {
      ArgumentValidator.EnsureArgumentNotNull(alias, "alias");
      Insert(index, SqlDml.ColumnRef(SqlDml.Column(expression), alias));
    }

    public void AddRange(params SqlColumn[] columns)
    {
      ArgumentValidator.EnsureArgumentNotNull(columns, "columns");
      foreach (SqlColumn c in columns) {
        ArgumentValidator.EnsureArgumentNotNull(c, "column");
        base.Add(c);
      }
    }

    public void AddRange<TColumn>(IEnumerable<TColumn> columns)
      where TColumn : SqlColumn
    {
      ArgumentValidator.EnsureArgumentNotNull(columns, "columns");
      foreach (TColumn c in columns)
        base.Add(c);
    }


    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    public SqlColumnCollection()
    {
    }

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    public SqlColumnCollection(IList<SqlColumn> list)
      : base(list)
    {
    }
  }
}
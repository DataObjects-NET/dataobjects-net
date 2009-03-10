// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.

using System;
using System.Collections;
using System.Collections.Generic;
using Xtensive.Sql.Dom.Database;

namespace Xtensive.Sql.Dom.Dml
{
  /// <summary>
  /// Describes a reference to <see cref="Table"/> object;
  /// </summary>
  [Serializable]
  public abstract class SqlTable : 
    SqlNode,
    IEnumerable<SqlTable>
  {
    private string name;
    protected SqlTableColumnCollection columns;
    private readonly SqlTableColumn asterisk;

    /// <summary>
    /// Gets the name of the instance.
    /// </summary>
    /// <value>The name.</value>
    public virtual string Name
    {
      get { return name; }
      internal set { name = value; }
    }

    /// <summary>
    /// Gets the collection of columns.
    /// </summary>
    /// <value>The collection of column references.</value>
    public SqlTableColumnCollection Columns
    {
      get { return columns; }
    }

    public SqlTableColumn Asterisk
    {
      get { return asterisk; }
    }

    /// <summary>
    /// An indexer that provides access to collection items by their names.
    /// Returns <see langword="null"/> if there is no such item.
    /// </summary>
    public SqlTableColumn this[string name]
    {
      get { return (name==Sql.Asterisk) ? asterisk : columns[name]; }
    }

    /// <summary>
    /// Gets the <see cref="Xtensive.Sql.Dom.Dml.SqlColumn"/> at the specified index.
    /// </summary>
    /// <value></value>
    public SqlTableColumn this[int index]
    {
      get { return columns[index]; }
    }

    public virtual IEnumerator<SqlTable> GetEnumerator()
    {
      yield return this;
      yield break;
    }

    public virtual SqlJoinedTable CrossJoin(SqlTable right)
    {
      return Sql.Join(SqlJoinType.CrossJoin, this, right);
    }

    public virtual SqlJoinedTable UnionJoin(SqlTable right)
    {
      return Sql.Join(SqlJoinType.UnionJoin, this, right, null);
    }

    public virtual SqlJoinedTable InnerJoin(SqlTable right)
    {
      return InnerJoin(right, null);
    }

    public virtual SqlJoinedTable LeftOuterJoin(SqlTable right)
    {
      return LeftOuterJoin(right, null);
    }

    public virtual SqlJoinedTable RightOuterJoin(SqlTable right)
    {
      return RightOuterJoin(right, null);
    }

    public virtual SqlJoinedTable FullOuterJoin(SqlTable right)
    {
      return FullOuterJoin(right, null);
    }

    public virtual SqlJoinedTable InnerJoin(SqlTable right, SqlExpression expression)
    {
      return Sql.Join(SqlJoinType.InnerJoin, this, right, expression);
    }

    public virtual SqlJoinedTable LeftOuterJoin(SqlTable right, SqlExpression expression)
    {
      return Sql.Join(SqlJoinType.LeftOuterJoin, this, right, expression);
    }

    public virtual SqlJoinedTable RightOuterJoin(SqlTable right, SqlExpression expression)
    {
      return Sql.Join(SqlJoinType.RightOuterJoin, this, right, expression);
    }

    public virtual SqlJoinedTable FullOuterJoin(SqlTable right, SqlExpression expression)
    {
      return Sql.Join(SqlJoinType.FullOuterJoin, this, right, expression);
    }

    public virtual SqlJoinedTable UsingJoin(SqlTable right, params SqlColumn[] columns)
    {
      return Sql.Join(this, right, columns);
    }

    public virtual SqlJoinedTable CrossApply(SqlTable right)
    {
      return Sql.Join(SqlJoinType.CrossApply, this, right);
    }

    public virtual SqlJoinedTable LeftOuterApply(SqlTable right)
    {
      return Sql.Join(SqlJoinType.LeftOuterApply, this, right);
    }

    /// <inheritdoc/>
    IEnumerator IEnumerable.GetEnumerator()
    {
      return GetEnumerator();
    }


    // Constructors

    protected SqlTable() : base(SqlNodeType.Table)
    {
      asterisk = new SqlTableColumn(this, Sql.Asterisk.Value);
    }

    protected SqlTable(string name) : this()
    {
      if (!string.IsNullOrEmpty(name))
        this.name = name;
    }
  }
}

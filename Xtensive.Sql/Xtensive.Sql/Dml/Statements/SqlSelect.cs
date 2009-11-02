// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.

using System;
using System.Collections;
using System.Collections.Generic;

namespace Xtensive.Sql.Dml
{
  /// <summary>
  /// Represents SQL SELECT statement.
  /// </summary>
  [Serializable]
  public class SqlSelect
    : SqlQueryStatement,
      ISqlQueryExpression
  {
    private readonly SqlUserColumn asterisk = SqlDml.Column(SqlDml.Asterisk);
    private readonly SqlColumnCollection columns = new SqlColumnCollection();
    private bool distinct;
    private SqlTable from;
    private SqlColumnCollection groupBy = new SqlColumnCollection();
    private SqlExpression having;
    private SqlOrderCollection orderBy;
    private SqlExpression where;

    /// <summary>
    /// Gets the collection of columns to select.
    /// </summary>
    /// <value>The collection of columns.</value>
    public SqlColumnCollection Columns
    {
      get { return columns; }
    }

    /// <summary>
    /// An indexer that provides access to collection items by their names.
    /// Returns <see langword="null"/> if there is no such item.
    /// </summary>
    public SqlColumn this[string name]
    {
      get { return columns[name]; }
    }

    /// <summary>
    /// An indexer that provides access to collection items by their index.
    /// Returns <see langword="null"/> if there is no such item.
    /// </summary>
    public SqlColumn this[int index]
    {
      get { return columns[index]; }
    }

    /// <summary>
    /// Gets or sets from clause.
    /// </summary>
    /// <value>The from clause.</value>
    public SqlTable From
    {
      get { return from; }
      set { from = value; }
    }

    /// <summary>
    /// Gets or sets the where clause.
    /// </summary>
    /// <value>The where clause.</value>
    public SqlExpression Where
    {
      get { return where; }
      set
      {
        if (!SqlExpression.IsNull(value))
          SqlValidator.EnsureIsBooleanExpression(value);
        where = value;
      }
    }

    /// <summary>
    /// Gets the collection of columns to group by.
    /// </summary>
    /// <value>The collection of columns.</value>
    public SqlColumnCollection GroupBy
    {
      get
      {
        if (groupBy == null)
          groupBy = new SqlColumnCollection();
        return groupBy;
      }
    }

    /// <summary>
    /// Gets or sets the having clause.
    /// </summary>
    /// <value>The having clause.</value>
    public SqlExpression Having
    {
      get { return having; }
      set
      {
        if (!SqlExpression.IsNull(value))
          SqlValidator.EnsureIsBooleanExpression(value);
        having = value;
      }
    }

    /// <summary>
    /// Gets the order by clause.
    /// </summary>
    /// <value>The order by clause.</value>
    public SqlOrderCollection OrderBy
    {
      get
      {
        if (orderBy == null)
          orderBy = new SqlOrderCollection();
        return orderBy;
      }
    }

    /// <summary>
    /// Gets or sets a value indicating whether this <see cref="SqlSelect"/> is distinct.
    /// </summary>
    /// <value><see langword="true"/> if distinct is set; otherwise, <see langword="false"/>.</value>
    public bool Distinct
    {
      get { return distinct; }
      set { distinct = value; }
    }

    public SqlUserColumn Asterisk
    {
      get { return asterisk; }
    }

    internal override object Clone(SqlNodeCloneContext context)
    {
      if (context.NodeMapping.ContainsKey(this))
        return context.NodeMapping[this];

      SqlSelect clone = new SqlSelect(from == null ? null : (SqlTable)from.Clone(context));

      foreach (SqlColumn c in columns)
        clone.Columns.Add((SqlColumn)c.Clone(context));
      if (groupBy != null)
        foreach (SqlColumn c in groupBy)
          clone.GroupBy.Add((SqlColumn)c.Clone(context));
      if (!SqlExpression.IsNull(where))
        clone.Where = (SqlExpression)where.Clone(context);
      if (!SqlExpression.IsNull(having))
        clone.Having = (SqlExpression)having.Clone(context);
      if (orderBy != null)
        foreach (SqlOrder so in orderBy)
          clone.OrderBy.Add((SqlOrder)so.Clone(context));
      clone.Distinct = distinct;
      clone.Top = Top;
      clone.Offset = Offset;

      if (Hints.Count > 0)
        foreach (SqlHint hint in Hints)
          clone.Hints.Add((SqlHint)hint.Clone(context));

      context.NodeMapping[this] = clone;

      return clone;
    }

    #region ISqlCompileUnit Members

    public override void AcceptVisitor(ISqlVisitor visitor)
    {
      visitor.Visit(this);
    }

    #endregion

    #region ISqlQueryExpression Members

    public SqlQueryExpression Except(ISqlQueryExpression operand)
    {
      return SqlDml.Except(this, operand);
    }

    public SqlQueryExpression ExceptAll(ISqlQueryExpression operand)
    {
      return SqlDml.ExceptAll(this, operand);
    }

    public SqlQueryExpression Intersect(ISqlQueryExpression operand)
    {
      return SqlDml.Intersect(this, operand);
    }

    public SqlQueryExpression IntersectAll(ISqlQueryExpression operand)
    {
      return SqlDml.IntersectAll(this, operand);
    }

    public SqlQueryExpression Union(ISqlQueryExpression operand)
    {
      return SqlDml.Union(this, operand);
    }

    public SqlQueryExpression UnionAll(ISqlQueryExpression operand)
    {
      return SqlDml.UnionAll(this, operand);
    }

    public IEnumerator<ISqlQueryExpression> GetEnumerator()
    {
      yield return this;
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
      return ((IEnumerable<ISqlQueryExpression>)this).GetEnumerator();
    }

    #endregion


    // Constructors

    internal SqlSelect(SqlTable from)
      : this()
    {
      this.from = from;
    }

    internal SqlSelect()
      : base(SqlNodeType.Select)
    {
    }
  }
}
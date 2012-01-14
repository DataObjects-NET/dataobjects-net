// Copyright (C) 2003-2010 Xtensive LLC.
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
    private SqlLockType _lock;
    private bool distinct;
    private SqlTable from;
    private SqlColumnCollection groupBy = new SqlColumnCollection();
    private SqlExpression having;
    private SqlOrderCollection orderBy;
    private SqlExpression where;
    private SqlExpression limit;
    private SqlExpression offset;

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
        if (!value.IsNullReference())
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
        if (!value.IsNullReference())
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

    public SqlLockType Lock
    {
      get { return _lock; }
      set { _lock = value; }
    }

    /// <summary>
    /// Gets or sets the limit.
    /// </summary>
    public SqlExpression Limit
    {
      get { return limit; }
      set {
        if (!value.IsNullReference())
          SqlValidator.EnsureIsLimitOffsetArgument(value);
        limit = value;
      }
    }

    /// <summary>
    /// Gets or sets the offset.
    /// </summary>
    public SqlExpression Offset
    {
      get { return offset; }
      set {
        if (!value.IsNullReference())
          SqlValidator.EnsureIsLimitOffsetArgument(value);
        offset = value;
      }
    }

    internal override object Clone(SqlNodeCloneContext context)
    {
      if (context.NodeMapping.ContainsKey(this))
        return context.NodeMapping[this];

      SqlSelect clone = new SqlSelect(from==null ? null : (SqlTable) from.Clone(context));

      foreach (SqlColumn c in columns)
        clone.Columns.Add((SqlColumn)c.Clone(context));
      if (groupBy != null)
        foreach (SqlColumn c in groupBy)
          clone.GroupBy.Add((SqlColumn)c.Clone(context));
      if (!where.IsNullReference())
        clone.Where = (SqlExpression)where.Clone(context);
      if (!having.IsNullReference())
        clone.Having = (SqlExpression)having.Clone(context);
      if (orderBy != null)
        foreach (SqlOrder so in orderBy)
          clone.OrderBy.Add((SqlOrder)so.Clone(context));
      clone.Distinct = distinct;
      clone.Limit = Limit;
      clone.Offset = Offset;
      clone.Lock = Lock;

      if (Hints.Count > 0)
        foreach (SqlHint hint in Hints)
          clone.Hints.Add((SqlHint)hint.Clone(context));

      context.NodeMapping[this] = clone;

      return clone;
    }

    /// <summary>
    /// Makes a shallow clone of the instance.
    /// </summary>
    public SqlSelect ShallowClone()
    {
      var result = ReferenceEquals(From, null) 
        ? SqlDml.Select() 
        : SqlDml.Select(From);
      result.Columns.AddRange(Columns);
      result.Distinct = Distinct;
      result.GroupBy.AddRange(GroupBy);
      result.Having = Having;
      result.Offset = Offset;
      result.Limit = Limit;
      foreach (var order in OrderBy)
        result.OrderBy.Add(order);
      foreach (var hint in Hints)
        result.Hints.Add(hint);
      result.Where = Where;
      result.Lock = Lock;
      return result;
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
      return ((IEnumerable<ISqlQueryExpression>) this).GetEnumerator();
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
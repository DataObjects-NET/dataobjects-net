// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.

using System;

namespace Xtensive.Sql.Dml
{
  /// <summary>
  /// Describes SQL DELETE statement.
  /// </summary>
  [Serializable]
  public class SqlDelete : SqlQueryStatement, ISqlCompileUnit
  {
    private SqlExpression where;
    private SqlTableRef delete;
    private SqlTable from;
    private SqlExpression limit;

    /// <summary>
    /// Gets or sets the table.
    /// </summary>
    /// <value>The table to change.</value>
    public SqlTableRef Delete
    {
      get { return delete; }
      set { delete = value; }
    }

    /// <summary>
    /// Gets or sets the WHERE clause expression.
    /// </summary>
    /// <value>The WHERE clause expression.</value>
    public SqlExpression Where {
      get {
        return where;
      }
      set {
        if (!value.IsNullReference() && value.GetType()!=typeof(SqlCursor))
          SqlValidator.EnsureIsBooleanExpression(value);
        where = value;
      }
    }

    /// <summary>
    /// Gets or sets the FROM clause expression.
    /// </summary>
    public SqlTable From 
    {
      get { return from;}
      set { from = value; }
    }

    /// <summary>
    /// Gets or sets the LIMIT clause expression
    /// </summary>
    public SqlExpression Limit
    {
      get { return limit; }
      set { limit = value; }
    }

    internal override object Clone(SqlNodeCloneContext context)
    {
      if (context.NodeMapping.ContainsKey(this))
        return context.NodeMapping[this];

      SqlDelete clone = new SqlDelete();
      if (Delete!=null)
        clone.Delete = (SqlTableRef)Delete.Clone(context);
      if (from!=null)
        clone.From = (SqlQueryRef)from.Clone(context);
      if (!where.IsNullReference())
        clone.Where = (SqlExpression) where.Clone(context);

      if (Hints.Count>0)
        foreach (SqlHint hint in Hints)
          clone.Hints.Add((SqlHint)hint.Clone(context));

      context.NodeMapping[this] = clone;
      return clone;
    }

    // Constructor

    internal SqlDelete(): base(SqlNodeType.Delete)
    {
    }

    internal SqlDelete(SqlTableRef table) : this()
    {
      delete = table;
    }

    public override void AcceptVisitor(ISqlVisitor visitor)
    {
      visitor.Visit(this);
    }
  }
}

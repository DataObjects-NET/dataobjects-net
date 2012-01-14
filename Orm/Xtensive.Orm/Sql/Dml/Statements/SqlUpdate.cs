// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.

using System;
using System.Collections.Generic;

namespace Xtensive.Sql.Dml
{
  [Serializable]
  public class SqlUpdate : SqlQueryStatement, ISqlCompileUnit
  {
    private SqlExpression where;
    private readonly Dictionary<ISqlLValue, SqlExpression> values = new Dictionary<ISqlLValue, SqlExpression>();
    private SqlTable from;
    private SqlTableRef update;

    /// <summary>
    /// Gets or sets the table.
    /// </summary>
    /// <value>The table to change.</value>
    public SqlTableRef Update
    {
      get { return update; }
      set { update = value; }
    }

    /// <summary>
    /// Gets the values.
    /// </summary>
    /// <value>The values.</value>
    public Dictionary<ISqlLValue, SqlExpression> Values {
      get {
        return values;
      }
    }

    /// <summary>
    /// Gets or sets the WHERE clause expression.
    /// </summary>
    /// <value>The WHERE clause expression.</value>
    public SqlExpression Where {
      get { return where; }
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

    internal override object Clone(SqlNodeCloneContext context)
    {
      if (context.NodeMapping.ContainsKey(this))
        return context.NodeMapping[this];

      var clone = new SqlUpdate();
      if (update!=null)
        clone.Update = (SqlTableRef)Update.Clone(context);
      if (from!=null)
        clone.From = (SqlQueryRef)from.Clone(context);
      foreach (KeyValuePair<ISqlLValue, SqlExpression> p in values)
        clone.Values[(ISqlLValue) ((SqlExpression) p.Key).Clone(context)] =
          p.Value.IsNullReference() ? null : (SqlExpression) p.Value.Clone(context);
      if (!where.IsNullReference())
        clone.Where = (SqlExpression)where.Clone(context);

      if (Hints.Count>0)
        foreach (SqlHint hint in Hints)
          clone.Hints.Add((SqlHint)hint.Clone(context));

      context.NodeMapping[this] = clone;
      return clone;
    }

    // Constructor

    internal SqlUpdate(): base(SqlNodeType.Update)
    {
    }

    internal SqlUpdate(SqlTableRef table) : this()
    {
      update = table;
    }

    public override void AcceptVisitor(ISqlVisitor visitor)
    {
      visitor.Visit(this);
    }
  }
}

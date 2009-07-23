// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.

using System;
using System.Collections.Generic;

namespace Xtensive.Sql.Dml
{
  [Serializable]
  public class SqlInsert : SqlQueryStatement, ISqlCompileUnit
  {
    private Dictionary<SqlColumn, SqlExpression> values = new Dictionary<SqlColumn, SqlExpression>();
    private SqlTableRef into;
    private SqlTable from;

    /// <summary>
    /// Gets or sets the table.
    /// </summary>
    /// <value>The table to change.</value>
    public SqlTableRef Into
    {
      get { return into; }
      set { into = value; }
    }

    /// <summary>
    /// Gets the values.
    /// </summary>
    /// <value>The values.</value>
    public Dictionary<SqlColumn, SqlExpression> Values {
      get {
        return values;
      }
    }

    /// <summary>
    /// Gets or sets the FROM clause expression.
    /// </summary>
    public SqlTable From {
      get {
        return from;
      }
      set {
        from = value;
      }
    }

    internal override object Clone(SqlNodeCloneContext context)
    {
      if (context.NodeMapping.ContainsKey(this))
        return context.NodeMapping[this];

      SqlInsert clone = new SqlInsert();
      if (Into!=null)
        clone.Into = (SqlTableRef)Into.Clone(context);
      if (from!=null)
        clone.From = (SqlTable)from.Clone(context);
      clone.Limit = Limit;
      foreach (KeyValuePair<SqlColumn, SqlExpression> p in values)
        clone.Values[(SqlTableColumn) p.Key.Clone(context)] =
          p.Value.IsNullReference() ? null : (SqlExpression) p.Value.Clone(context);

      if (Hints.Count>0)
        foreach (SqlHint hint in Hints)
          clone.Hints.Add((SqlHint)hint.Clone(context));

      context.NodeMapping[this] = clone;
      return clone;
    }

    // Constructor

    internal SqlInsert(): base(SqlNodeType.Insert)
    {
    }

    internal SqlInsert(SqlTableRef tableRef) : this()
    {
      into = tableRef;
    }

    public override void AcceptVisitor(ISqlVisitor visitor)
    {
      visitor.Visit(this);
    }
  }
}

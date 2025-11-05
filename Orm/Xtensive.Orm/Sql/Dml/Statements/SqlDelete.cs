// Copyright (C) 2009-2024 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.

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
        if (value is not null && value.GetType()!=typeof(SqlCursor))
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

    internal override SqlDelete Clone(SqlNodeCloneContext context) =>
      context.GetOrAdd(this, static (t, c) => {
        SqlDelete clone = new SqlDelete();
        if (t.Delete != null)
          clone.Delete = t.Delete.Clone(c);
        if (t.from != null)
          clone.From = (SqlQueryRef) t.from.Clone(c);
        if (t.where is not null)
          clone.Where = t.where.Clone(c);

        if (t.Hints.Count > 0)
          foreach (SqlHint hint in t.Hints)
            clone.Hints.Add((SqlHint) hint.Clone(c));

        return clone;
      });

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

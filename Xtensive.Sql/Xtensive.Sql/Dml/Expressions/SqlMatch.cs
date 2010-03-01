// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.

using System;
using Xtensive.Core;

namespace Xtensive.Sql.Dml
{
  /// <summary>
  /// Represents MATCH SQL statement.
  /// </summary>
  [Serializable]
  public class SqlMatch: SqlExpression
  {
    private SqlExpression value;
    private SqlSubQuery subQuery;
    private bool unique = false;
    private SqlMatchType matchType = SqlMatchType.None;

    /// <summary>
    /// Gets the value which will be tested for matching.
    /// </summary>
    public SqlExpression Value
    {
      get { return value; }
    }

    /// <summary>
    /// Gets the sub query to search of matching.
    /// </summary>
    public SqlSubQuery SubQuery
    {
      get { return subQuery; }
    }

    /// <summary>
    /// Gets a value indicating whether unique sub query rows for search matching will be used only.
    /// </summary>
    public bool Unique
    {
      get { return unique; }
    }

    /// <summary>
    /// Gets the type of the match.
    /// </summary>
    public SqlMatchType MatchType
    {
      get { return matchType; }
    }

    public override void ReplaceWith(SqlExpression expression)
    {
      ArgumentValidator.EnsureArgumentNotNull(expression, "expression");
      ArgumentValidator.EnsureArgumentIs<SqlMatch>(expression, "expression");
      SqlMatch replacingExpression = expression as SqlMatch;
      value = replacingExpression.Value;
      subQuery = replacingExpression.SubQuery;
      matchType = replacingExpression.MatchType;
      unique = replacingExpression.Unique;
    }

    internal override object Clone(SqlNodeCloneContext context)
    {
      if (context.NodeMapping.ContainsKey(this))
        return context.NodeMapping[this];

      SqlMatch clone = new SqlMatch((SqlExpression)value.Clone(context),
                                    (SqlSubQuery)subQuery.Clone(context),
                                    unique,
                                    matchType);
      context.NodeMapping[this] = clone;
      return clone;
    }

    internal SqlMatch(SqlExpression value, SqlSubQuery subQuery, bool unique, SqlMatchType matchType)
      : base(SqlNodeType.Match)
    {
      this.value = value;
      this.subQuery = subQuery;
      this.unique = unique;
      this.matchType = matchType;
    }

    public override void AcceptVisitor(ISqlVisitor visitor)
    {
      visitor.Visit(this);
    }
  }
}
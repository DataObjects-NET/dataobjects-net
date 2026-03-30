// Copyright (C) 2009-2024 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.

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
      var replacingExpression = ArgumentValidator.EnsureArgumentIs<SqlMatch>(expression);
      value = replacingExpression.Value;
      subQuery = replacingExpression.SubQuery;
      matchType = replacingExpression.MatchType;
      unique = replacingExpression.Unique;
    }

    internal override SqlMatch Clone(SqlNodeCloneContext context) =>
      context.GetOrAdd(this, static (t, c) =>
        new SqlMatch(t.value.Clone(c),
                                    t.subQuery.Clone(c),
                                    t.unique,
                                    t.matchType));

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
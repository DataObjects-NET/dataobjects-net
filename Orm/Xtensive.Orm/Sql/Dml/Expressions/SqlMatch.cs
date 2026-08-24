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
  public class SqlMatch: SqlExpression
  {
    /// <summary>
    /// Gets the value which will be tested for matching.
    /// </summary>
    public SqlExpression Value { get; private set; }

    /// <summary>
    /// Gets the sub query to search of matching.
    /// </summary>
    public SqlSubQuery SubQuery { get; private set; }

    /// <summary>
    /// Gets a value indicating whether unique sub query rows for search matching will be used only.
    /// </summary>
    public bool Unique { get; private set; } = false;

    /// <summary>
    /// Gets the type of the match.
    /// </summary>
    public SqlMatchType MatchType { get; private set; } = SqlMatchType.None;

    public override void ReplaceWith(SqlExpression expression)
    {
      var replacingExpression = ArgumentValidator.EnsureArgumentIs<SqlMatch>(expression);
      Value = replacingExpression.Value;
      SubQuery = replacingExpression.SubQuery;
      MatchType = replacingExpression.MatchType;
      Unique = replacingExpression.Unique;
    }

    internal override SqlMatch Clone(SqlNodeCloneContext context) =>
      context.GetOrAdd(this, static (t, c) =>
        new SqlMatch(t.Value.Clone(c),
                                    t.SubQuery.Clone(c),
                                    t.Unique,
                                    t.MatchType));

    internal SqlMatch(SqlExpression value, SqlSubQuery subQuery, bool unique, SqlMatchType matchType)
      : base(SqlNodeType.Match)
    {
      Value = value;
      SubQuery = subQuery;
      Unique = unique;
      MatchType = matchType;
    }

    public override void AcceptVisitor(ISqlVisitor visitor)
    {
      visitor.Visit(this);
    }
  }
}
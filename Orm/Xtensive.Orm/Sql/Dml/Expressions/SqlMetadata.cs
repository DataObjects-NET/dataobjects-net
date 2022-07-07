// Copyright (C) 2022 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.

using Xtensive.Core;

namespace Xtensive.Sql.Dml
{
  /// <summary>
  /// Arbitrary metadata that could be attached to SQL expression tree.
  /// </summary>
  public class SqlMetadata : SqlExpression
  {
    public SqlExpression Expression { get; private set; }

    public object Value { get; private set; }

    public override void ReplaceWith(SqlExpression expression)
    {
      ArgumentValidator.EnsureArgumentNotNull(expression, nameof(expression));
      ArgumentValidator.EnsureArgumentIs<SqlMetadata>(expression, nameof(expression));
      var source = (SqlMetadata) expression;
      NodeType = source.NodeType;
      Expression = source.Expression;
      Value = source.Value;
    }

    internal override SqlMetadata Clone(SqlNodeCloneContext context) =>
      context.TryGet(this) ?? context.Add(this,
        new SqlMetadata(Expression.Clone(context), Value));

    public override void AcceptVisitor(ISqlVisitor visitor) => visitor.Visit(this);

    // Constructors

    internal SqlMetadata(SqlExpression expression, object value) : base(SqlNodeType.Metadata)
    {
      Expression = expression;
      Value = value;
    }
  }
}
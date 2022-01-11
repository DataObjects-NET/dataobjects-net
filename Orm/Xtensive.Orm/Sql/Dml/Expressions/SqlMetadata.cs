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

    internal override object Clone(SqlNodeCloneContext context) =>
      context.NodeMapping.TryGetValue(this, out var clone)
        ? clone
        : context.NodeMapping[this] = new SqlMetadata((SqlExpression) Expression.Clone(context), Value);

    public override void AcceptVisitor(ISqlVisitor visitor) => visitor.Visit(this);

    // Constructors

    internal SqlMetadata(SqlExpression expression, object value) : base(SqlNodeType.Metadata)
    {
      Expression = expression;
      Value = value;
    }
  }
}
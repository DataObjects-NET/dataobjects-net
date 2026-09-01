using System;
using System.Linq.Expressions;
using Xtensive.Orm.SerializableExpressions.Internals;

namespace Xtensive.Orm.SerializableExpressions
{
  /// <summary>
  /// Contains extensions related to <see cref="Expression"/>s
  /// </summary>
  public static class ExpressionExtensions
  {
    /// <summary>
    /// Converts specified <see cref="Expression"/> to <see cref="SerializableExpression"/>.
    /// </summary>
    /// <param name="expression">The expression to convert.</param>
    /// <returns>Serializable expression that represents <paramref name="expression"/>.</returns>
    public static SerializableExpression ToSerializableExpression(this Expression expression)
    {
      return new ExpressionToSerializableExpressionConverter(expression).Convert();
    }

    /// <summary>
    /// Converts specified <see cref="SerializableExpression"/> to <see cref="Expression"/>.
    /// </summary>
    /// <param name="expression">The expression to convert.</param>
    /// <returns>Expression that represents given <see cref="SerializableExpression"/>.</returns>
    public static Expression ToExpression(this SerializableExpression expression)
    {
      return new SerializableExpressionToExpressionConverter(expression).Convert();
    }
  }
}

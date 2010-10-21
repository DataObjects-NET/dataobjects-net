using System;
using System.Linq.Expressions;
using Xtensive.Core.Linq;
using ExpressionVisitor = Xtensive.Core.Linq.ExpressionVisitor;
using Xtensive.Core;

namespace Xtensive.Storage.Linq.Rewriters
{
  internal class CoalesceRewriter : ExpressionVisitor
  {
    public static Expression Rewrite(Expression e)
    {
      return new CoalesceRewriter().Visit(e);
    }

    protected override Expression VisitUnknown(Expression e)
    {
      return e;
    }

    protected override Expression VisitConditional(ConditionalExpression c)
    {
        var test = Visit(c.Test);
        var ifTrue = Visit(c.IfTrue);
        var ifFalse = Visit(c.IfFalse);

      if (test.NodeType.In(ExpressionType.Equal, ExpressionType.NotEqual)) {

        var binaryExpression = (BinaryExpression) test;
        var left = binaryExpression.Left.StripCasts();
        var right = binaryExpression.Right.StripCasts();
        if ((IsEntity(left) && IsNull(right)) || (IsEntity(right) && IsNull(left))) {
          var nullPart = c.Test.NodeType==ExpressionType.Equal ? ifTrue : ifFalse;
          var memberAccessPart = c.Test.NodeType==ExpressionType.Equal ? ifFalse : ifTrue;
          if (IsNull(nullPart) && memberAccessPart.StripCasts().NodeType==ExpressionType.MemberAccess) {
            var memberAccess = (MemberExpression) memberAccessPart.StripCasts();
            if (new ExpressionComparer().AreEqual(memberAccess.Expression, IsNull(right) ? left : right))
              return memberAccessPart;
          }
        }
      }

      if (!ReferenceEquals(test, c.Test) || !ReferenceEquals(ifTrue, c.IfTrue) || !ReferenceEquals(ifFalse, c.IfFalse))
        return Expression.Condition(test, ifTrue, ifFalse);

      return c;
    }

    private static bool IsNull(Expression expression)
    {
      return expression.NodeType==ExpressionType.Constant && ((ConstantExpression) expression).Value==null;
    }

    private static bool IsEntity(Expression expression)
    {
      return expression.Type.IsSubclassOf(typeof (Entity));
    }
  }
}

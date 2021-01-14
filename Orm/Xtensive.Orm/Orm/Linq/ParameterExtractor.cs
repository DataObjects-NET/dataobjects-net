// Copyright (C) 2008-2020 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Alexey Kochetov
// Created:    2008.12.18

using System.Linq.Expressions;
using ExpressionVisitor = Xtensive.Linq.ExpressionVisitor;

namespace Xtensive.Orm.Linq
{
  internal sealed class ParameterExtractor : ExpressionVisitor
  {
    private readonly ExpressionEvaluator evaluator;
    private bool isParameter;

    public bool IsParameter(Expression e)
    {
      if (!evaluator.CanBeEvaluated(e)) {
        return false;
      }

      isParameter = false;
      Visit(e);
      return isParameter;
    }

    protected override Expression VisitMemberAccess(MemberExpression m)
    {
      isParameter = true;
      return base.VisitMemberAccess(m);
    }

    protected override Expression VisitUnknown(Expression e) => e;

    protected override Expression VisitConstant(ConstantExpression c)
    {
      isParameter = c.GetMemberType() switch {
        MemberType.Entity => true,
        MemberType.Structure => true,
        _ => isParameter
      };
      return c;
    }

    // Constructors

    public ParameterExtractor(ExpressionEvaluator evaluator)
    {
      this.evaluator = evaluator;
    }
  }
}
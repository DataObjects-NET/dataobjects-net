// Copyright (C) 2019-2020 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.

using System;
using System.Linq;
using System.Linq.Expressions;
using Xtensive.Linq;
using Xtensive.Reflection;

namespace Xtensive.Orm.BulkOperations
{
  internal static class ExpressionExtensions
  {
    public static bool IsContainsQuery(this Expression expression)
    {
      bool b = false;
      expression.Visit(
        delegate(Expression e) {
          if (e.Type.IsOfGenericInterface(typeof (IQueryable<>)))
            b = true;
          return e;
        });
      return b;
    }

    #region Non-public methods

    internal static object Invoke(this Expression expression)
    {
      return FastExpression.Lambda(
        WellKnownMembers.FuncOfTResultType.CachedMakeGenericType(expression.Type), expression).Compile().DynamicInvoke();
    }

    internal static Expression Visit<T>(this Expression exp, Func<T, Expression> visitor) where T : Expression
    {
      return ExpressionVisitor<T>.Visit(exp, visitor);
    }

    #endregion
  }
}

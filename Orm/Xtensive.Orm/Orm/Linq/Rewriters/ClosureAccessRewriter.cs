// Copyright (C) 2009-2020 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Alexey Gamzov
// Created:    2009.10.09

using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Xtensive.Orm.Internals;
using Xtensive.Reflection;
using ExpressionVisitor = Xtensive.Linq.ExpressionVisitor;

namespace Xtensive.Orm.Linq.Rewriters
{
  internal class ClosureAccessRewriter : ExpressionVisitor
  {
    private readonly CompiledQueryProcessingScope compiledQueryScope;

    protected override Expression VisitUnknown(Expression e) => e;

    protected override Expression VisitMemberAccess(MemberExpression memberExpression)
    {
      if (memberExpression.Type.IsOfGenericInterface(WellKnownInterfaces.QueryableOfT)
        && memberExpression.Expression != null
        && memberExpression.Expression.NodeType == ExpressionType.Constant
        && memberExpression.Member != null
        && memberExpression.Member.ReflectedType.IsClosure()
        && memberExpression.Member.MemberType == MemberTypes.Field) {
        var fieldInfo = (FieldInfo) memberExpression.Member;
        if (!fieldInfo.FieldType.IsOfGenericType(WellKnownOrmTypes.EntitySetOfT)) {
          if (compiledQueryScope != null) {
            throw new InvalidOperationException(string.Format(Strings.ExUnableToUseIQueryableXInQueryExecuteStatement,
              fieldInfo.Name));
          }

          var constantValue = ((ConstantExpression) memberExpression.Expression).Value;
          var queryable = (IQueryable) fieldInfo.GetValue(constantValue);
          if (queryable.Expression.Type.IsOfGenericInterface(WellKnownInterfaces.QueryableOfT)) {
            return Visit(queryable.Expression);
          }

          return queryable.Expression;
        }
      }

      return base.VisitMemberAccess(memberExpression);
    }

    public static Expression Rewrite(Expression e, CompiledQueryProcessingScope compiledQueryScope) =>
      new ClosureAccessRewriter(compiledQueryScope).Visit(e);

    // Constructors

    private ClosureAccessRewriter(CompiledQueryProcessingScope compiledQueryScope)
    {
      this.compiledQueryScope = compiledQueryScope;
    }
  }
}
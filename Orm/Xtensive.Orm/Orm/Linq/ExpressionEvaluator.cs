// Copyright (C) 2008-2020 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Alexey Kochetov
// Created:    2008.12.18

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Xtensive.Core;
using Xtensive.Linq;
using Xtensive.Orm.Internals;
using Xtensive.Reflection;
using ExpressionVisitor = Xtensive.Linq.ExpressionVisitor;

namespace Xtensive.Orm.Linq
{
  /// <summary>
  /// Expression visitor that checks ability to evaluate expression to <see cref="ConstantExpression"/>.
  /// </summary>
  internal sealed class ExpressionEvaluator : ExpressionVisitor
  {
    private readonly HashSet<Expression> candidates = new HashSet<Expression>();
    private bool couldBeEvaluated;


    /// <summary>
    /// Determines whether specified <paramref name="e"/> can be evaluated.
    /// </summary>
    /// <param name="e">The expression.</param>
    /// <returns>
    ///   <see langword="true" /> if <paramref name="e"/> can be evaluated; otherwise, <see langword="false" />.
    /// </returns>
    public bool CanBeEvaluated(Expression e) => candidates.Contains(e);

    /// <summary>
    /// Evaluates the specified <paramref name="e"/> into <see cref="ConstantExpression"/>.
    /// </summary>
    /// <param name="e">The expression.</param>
    public static ConstantExpression Evaluate(Expression e)
    {
      if (e == null) {
        return null;
      }

      if (e.NodeType == ExpressionType.Constant) {
        return (ConstantExpression) e;
      }

      var type = e.Type;
      if (type.IsValueType) {
        e = Expression.Convert(e, WellKnownTypes.Object);
      }

      var lambda = FastExpression.Lambda<Func<object>>(e);
      var func = lambda.CachingCompile();
      return Expression.Constant(func(), type);
    }

    /// <inheritdoc/>
    protected override Expression Visit(Expression e)
    {
      if (e != null) {
        var saved = couldBeEvaluated;
        couldBeEvaluated = true;
        base.Visit(e);
        if (couldBeEvaluated) {
          if (CanEvaluateExpression(e)) {
            candidates.Add(e);
          }
          else {
            couldBeEvaluated = false;
          }
        }

        couldBeEvaluated &= saved;
      }

      return e;
    }

    /// <inheritdoc/>
    protected override Expression VisitUnknown(Expression e) => e;

    // Private methods

    private static bool CanEvaluateExpression(Expression expression)
    {
      if (expression.Type == WellKnownOrmTypes.ApplyParameter) {
        return false;
      }

      if (expression is ConstantExpression cex) {
        return !(cex.Value is IQueryable);
      }

      var expressionNodeType = expression.NodeType;
      if (expressionNodeType == ExpressionType.MemberAccess) {
        var ma = (MemberExpression) expression;
        var maExpression = ma.Expression;
        if (maExpression == null) {
          return !WellKnownInterfaces.Queryable.IsAssignableFrom(ma.Type);
        }

        if (maExpression.Type.IsNullable() && ma.Member.Name == "Value") {
          return false;
        }

        if (maExpression.NodeType == ExpressionType.Constant
            && (ma.Member as FieldInfo)?.FieldType is Type fieldType
            && fieldType.IsGenericType
            && WellKnownInterfaces.Queryable.IsAssignableFrom(fieldType)) {
          return false;
        }
      }

      if (expression is MethodCallExpression mc) {
        var methodInfo = mc.Method;
        var declaringType = methodInfo.DeclaringType;
        if (declaringType == WellKnownTypes.Enumerable ||
            declaringType == WellKnownTypes.Queryable ||
            declaringType == WellKnownOrmTypes.Query && methodInfo.IsGenericMethod) {
          return false;
        }
      }

      if (expressionNodeType == ExpressionType.Convert && expression.Type == WellKnownTypes.Object) {
        return true;
      }

      return !(expressionNodeType is ExpressionType.Parameter or ExpressionType.Lambda);
    }


    // Constructors

    /// <summary>
    /// Initializes a new instance of this class.
    /// </summary>
    public ExpressionEvaluator(Expression e)
    {
      couldBeEvaluated = true;
      Visit(e);
    }
  }
}
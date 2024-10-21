// Copyright (C) 2009-2024 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Alexey Gamzov
// Created:    2009.12.22

using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Xtensive.Orm.Internals;
using Xtensive.Reflection;
using ExpressionVisitor = Xtensive.Linq.ExpressionVisitor;

namespace Xtensive.Orm.Linq.Rewriters
{
  internal class PersistentIndexerRewriter : ExpressionVisitor
  {
    private readonly TranslatorContext context;

    /// <exception cref="InvalidOperationException"><c>InvalidOperationException</c>.</exception>
    protected override Expression VisitBinary(BinaryExpression binaryExpression)
    {
      if (binaryExpression.NodeType is ExpressionType.NotEqual or ExpressionType.Equal) {
        Expression leftExpression = null;
        Expression rightExpression = null;
        if (IsIndexerAccessor(binaryExpression.Left))
          leftExpression = GetMemberExpression((MethodCallExpression) binaryExpression.Left);
        if (IsIndexerAccessor(binaryExpression.Right))
          rightExpression = GetMemberExpression((MethodCallExpression) binaryExpression.Right);

        if (leftExpression!=null && rightExpression!=null && rightExpression.Type!=leftExpression.Type)
          throw new InvalidOperationException(String.Format(Strings.ExBothPartsOfBinaryExpressionXAreOfTheDifferentType, binaryExpression));

        if (leftExpression!=null) {
          if (rightExpression==null)
            rightExpression = binaryExpression.Right;
          if (leftExpression.Type!=rightExpression.Type)
            rightExpression = Expression.Convert(rightExpression, leftExpression.Type);
        }
        else if (rightExpression!=null) {
          leftExpression = binaryExpression.Left;
          if (leftExpression.Type!=rightExpression.Type)
            leftExpression = Expression.Convert(leftExpression, rightExpression.Type);
        }
        else
          return base.VisitBinary(binaryExpression);
        binaryExpression = Expression.MakeBinary(binaryExpression.NodeType, leftExpression, rightExpression, binaryExpression.IsLiftedToNull, binaryExpression.Method);
      }
      return base.VisitBinary(binaryExpression);
    }

    protected override Expression VisitMethodCall(MethodCallExpression mc)
    {
      if (IsIndexerAccessor(mc)) {
        var memberExpression = GetMemberExpression(mc);
        return Expression.Convert(memberExpression, WellKnownTypes.Object);
      }
      return base.VisitMethodCall(mc);
    }

    protected override Expression VisitUnknown(Expression e)
    {
      return e;
    }

    /// <exception cref="InvalidOperationException"><c>InvalidOperationException</c>.</exception>
    private Expression GetMemberExpression(MethodCallExpression mc)
    {
      var name = (string) ExpressionEvaluator.Evaluate(mc.Arguments[0]).Value;
      var visitedObject = Visit(mc.Object);
      if (mc.Object != visitedObject)
        mc = Expression.Call(visitedObject, mc.Method, mc.Arguments);
      
      var propertyInfo = mc.Object.Type.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
        .SingleOrDefault(property => property.Name==name);
      if (propertyInfo!=null)
        return Expression.MakeMemberAccess(mc.Object, propertyInfo);

      var indexerProperty = mc.Object.Type.GetProperty(Reflection.WellKnown.IndexerPropertyName);
      if (indexerProperty!=null)
        return Expression.MakeIndex(mc.Object, indexerProperty, new[] {Expression.Constant(name)});
      throw new InvalidOperationException(String.Format(Strings.ExFieldXNotFoundInTypeX, name, mc.Object.Type));
    }

    private bool IsIndexerAccessor(Expression expression)
    {
      if (expression.NodeType != ExpressionType.Call) {
        return false;
      }
      var methodCallExpression = (MethodCallExpression) expression;
      if (methodCallExpression.Object == null) {
        return false;
      }
      var method = methodCallExpression.Method;
      return method.Name == Reflection.WellKnown.IndexerPropertyGetterName
        && method.DeclaringType switch { var declaringType => declaringType == WellKnownOrmTypes.Persistent || declaringType == WellKnownOrmInterfaces.Entity }
        && context.Evaluator.CanBeEvaluated(methodCallExpression.Arguments[0]);
    }

    public static Expression Rewrite(Expression query, TranslatorContext context)
    {
      return new PersistentIndexerRewriter(context).Visit(query);
    }

    // Constructors

    private PersistentIndexerRewriter(TranslatorContext context)
    {
      this.context = context;
    }
  }
}
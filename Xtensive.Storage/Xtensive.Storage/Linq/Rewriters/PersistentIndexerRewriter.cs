// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2009.12.22

using System;
using System.Linq;
using System.Linq.Expressions;
using Xtensive.Core.Linq;

namespace Xtensive.Storage.Linq.Rewriters
{
  internal class PersistentIndexerRewriter : ExpressionVisitor
  {
    protected override Expression VisitBinary(BinaryExpression binaryExpression)
    {
      if (binaryExpression.NodeType.In(ExpressionType.NotEqual, ExpressionType.Equal)) {
        Expression leftExpression = null;
        Expression rightExpression = null;
        if (IsIndexerAccessor(binaryExpression.Left))
          leftExpression = GetMemberExpression((MethodCallExpression) binaryExpression.Left);
        if (IsIndexerAccessor(binaryExpression.Right))
          rightExpression = GetMemberExpression((MethodCallExpression) binaryExpression.Right);

        if (leftExpression!=null
          && rightExpression!=null
            && rightExpression.Type!=leftExpression.Type)
          throw new InvalidOperationException(String.Format(Xtensive.Storage.Resources.Strings.ExBothPartsOfBinaryExpressionXAreOfTheDifferentType, binaryExpression));

        if (leftExpression!=null) {
          leftExpression = leftExpression;
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
        MemberExpression memberExpression = GetMemberExpression(mc);
        return Expression.Convert(memberExpression, typeof (object));
      }
      return base.VisitMethodCall(mc);
    }

    protected override Expression VisitUnknown(Expression e)
    {
      return e;
    }

    private MemberExpression GetMemberExpression(MethodCallExpression mc)
    {
      var name = (string) ((ConstantExpression) mc.Arguments[0]).Value;
      var propertyInfo = mc.Object.Type.GetProperties().SingleOrDefault(property => property.Name==name);
      if (propertyInfo!=null)
        return Expression.MakeMemberAccess(mc.Object, propertyInfo);
      throw new InvalidOperationException(String.Format(Xtensive.Storage.Resources.Strings.ExFieldXNotFoundInTypeX, name, mc.Object.Type));
    }

    private bool IsIndexerAccessor(Expression expression)
    {
      if (expression.NodeType!=ExpressionType.Call)
        return false;
      var methodCallExpression = (MethodCallExpression) expression;
      return methodCallExpression.Object!=null
        && methodCallExpression.Method.Name=="get_Item"
          && methodCallExpression.Method.DeclaringType==typeof (Persistent)
            && methodCallExpression.Arguments[0].NodeType==ExpressionType.Constant;
    }

    public static Expression Rewrite(Expression query)
    {
      return new PersistentIndexerRewriter().Visit(query);
    }

    private PersistentIndexerRewriter()
    {
    }
  }
}
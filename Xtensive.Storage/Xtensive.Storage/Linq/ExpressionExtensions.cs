// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2008.12.02

using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using Xtensive.Core.Parameters;
using Xtensive.Core.Reflection;
using Xtensive.Core.Tuples;
using Xtensive.Storage.Linq.Expressions;

namespace Xtensive.Storage.Linq
{
  internal static class ExpressionExtensions
  {
    public static LambdaExpression StripQuotes(this Expression expression)
    {
      while (expression.NodeType==ExpressionType.Quote)
        expression = ((UnaryExpression) expression).Operand;
      return (LambdaExpression) expression;
    }

    public static Expression StripCasts(this Expression expression)
    {
      while (expression.NodeType == ExpressionType.Convert)
        expression = ((UnaryExpression)expression).Operand;
      return expression;
    }

    public static bool IsQuery(this Expression expression)
    {
      var type = expression.Type;

      if (type.IsInterface)
        return type.IsGenericType && type.GetGenericTypeDefinition()==typeof (IQueryable<>);

      return type.GetInterfaces()
        .Where(t => t.IsGenericType && t.GetGenericTypeDefinition()==typeof (IQueryable<>))
        .Any();
    }

    public static bool IsResult(this Expression expression)
    {
      return (ExtendedExpressionType) expression.NodeType==ExtendedExpressionType.Result;
    }

    public static bool IsGrouping(this Expression expression)
    {
      expression = expression.StripCasts();
      if (expression.NodeType==ExpressionType.New)
        return expression.Type.IsOfGenericType(typeof(Grouping<,>));
      return false;
    }

    public static Type GetGroupingKeyType(this Expression expression)
    {
      var newExpression = (NewExpression)expression.StripCasts();
      return newExpression.Type.GetGenericArguments()[0];
    }

    public static Type GetGroupingElementType(this Expression expression)
    {
      var newExpression = (NewExpression)expression.StripCasts();
      return newExpression.Type.GetGenericArguments()[1];
    }

    public static Parameter<Tuple> GetGroupingParameter(this Expression expression)
    {
      var newExpression = (NewExpression)expression.StripCasts();
      return (Parameter<Tuple>) ((ConstantExpression) newExpression.Arguments[3]).Value;
    }

    public static ResultExpression GetGroupingItemsResult(this Expression expression)
    {
      var newExpression = (NewExpression)expression.StripCasts();
      return (ResultExpression) ((ConstantExpression) newExpression.Arguments[2]).Value;
    }

    public static MemberType GetMemberType(this Expression e)
    {
      var type = e.Type;
      if (typeof (Key).IsAssignableFrom(type))
        return MemberType.Key;
      if (typeof (IEntity).IsAssignableFrom(type))
        return MemberType.Entity;
      if (typeof (Structure).IsAssignableFrom(type))
        return MemberType.Structure;
      if (typeof (EntitySetBase).IsAssignableFrom(type))
        return MemberType.EntitySet;
      if (Attribute.IsDefined(type, typeof (CompilerGeneratedAttribute), false)
        && type.BaseType==typeof (object)
          && type.Name.Contains("AnonymousType")
            && (type.Name.StartsWith("<>") || type.Name.StartsWith("VB$"))
              && (type.Attributes & TypeAttributes.NotPublic)==TypeAttributes.NotPublic)
        return MemberType.Anonymous;
      return MemberType.Unknown;
    }
 }
}
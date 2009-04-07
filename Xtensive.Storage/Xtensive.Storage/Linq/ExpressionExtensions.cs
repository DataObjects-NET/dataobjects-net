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

namespace Xtensive.Storage.Linq
{
  public static class ExpressionExtensions
  {
    public static LambdaExpression StripQuotes(this Expression expression)
    {
      while (expression.NodeType == ExpressionType.Quote)
        expression = ((UnaryExpression)expression).Operand;
      return (LambdaExpression)expression;
    }

    public static bool IsQuery(this Expression expression)
    {
      var type = expression.Type;

      if (type.IsInterface)
        return type.IsGenericType && type.GetGenericTypeDefinition()==typeof (IQueryable<>);

      return type.GetInterfaces()
        .Where(t => t.IsGenericType && t.GetGenericTypeDefinition()==typeof(IQueryable<>))
        .Any();
    }


    public static bool IsResult(this Expression expression)
    {
      return (ExtendedExpressionType)expression.NodeType == ExtendedExpressionType.Result;
    }

    public static bool IsGrouping(this Expression expression)
    {
      if (expression.NodeType==ExpressionType.New) {
          var newExpression = (NewExpression) expression;
          if (newExpression.Type.IsGenericType 
            && newExpression.Type.GetGenericTypeDefinition()==typeof (Grouping<,>))
            return true;
      }
      return false;
    }

    public static MemberType GetMemberType(this Expression e)
    {
      var type = e.Type;
      if (typeof(Key).IsAssignableFrom(type))
        return MemberType.Key;
      if (typeof(IEntity).IsAssignableFrom(type))
        return MemberType.Entity;
      if (typeof(Structure).IsAssignableFrom(type))
        return MemberType.Structure;
      if (typeof(EntitySetBase).IsAssignableFrom(type))
        return MemberType.EntitySet;
      if (Attribute.IsDefined(type, typeof(CompilerGeneratedAttribute), false)
          && type.BaseType == typeof(object)
          && type.Name.Contains("AnonymousType")
          && (type.Name.StartsWith("<>") || type.Name.StartsWith("VB$"))
          && (type.Attributes & TypeAttributes.NotPublic) == TypeAttributes.NotPublic)
        return MemberType.Anonymous;
      return MemberType.Unknown;
    }

    public static Expression KeyAccess(this Expression entity)
    {
      return Expression.Property(entity, "Key");
    }
  }
}
// Copyright (C) 2011 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2011.10.07

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using Xtensive.Core;
using Xtensive.Storage.Linq;
using Xtensive.Storage.Model;
using Xtensive.Storage.Resources;
using ExpressionVisitor = Xtensive.Core.Linq.ExpressionVisitor;
using FieldInfo = Xtensive.Storage.Model.FieldInfo;

namespace Xtensive.Storage.Building.Builders
{
  internal class PartialIndexFilterBuilder : ExpressionVisitor
  {
    private readonly TypeInfo declaringType;
    private readonly TypeInfo reflectedType;
    private readonly IndexInfo index;
    private readonly ParameterExpression parameter;
    private readonly List<FieldInfo> usedFields = new List<FieldInfo>();

    public static void BuildFilter(IndexInfo index)
    {
      ArgumentValidator.EnsureArgumentNotNull(index, "index");
      var parameter = Expression.Parameter(typeof (Core.Tuples.Tuple), "tuple");
      var builder = new PartialIndexFilterBuilder(index, parameter);
      var body = builder.Visit(index.FilterExpression.Body);
      var filter = new PartialIndexFilter();
      filter.Fields.AddRange(builder.usedFields);
      filter.Expression = Expression.Lambda(body, parameter);
      index.Filter = filter;
    }

    protected override Expression VisitMemberAccess(MemberExpression originalMemberAccess)
    {
      // Try to collapse series of member access expressions.
      // Finally we should reach original parameter.

      var memberAccess = originalMemberAccess;
      var memberAccessSequence = new List<MemberExpression>();
      for (;;) {
        if (!IsPersistentFieldAccess(memberAccess))
          break;
        memberAccessSequence.Add(memberAccess);
        if (memberAccess.Expression.NodeType!=ExpressionType.MemberAccess)
          break;
        memberAccess = (MemberExpression) memberAccess.Expression;
      }
      if (memberAccessSequence.Count==0)
        return base.VisitMemberAccess(originalMemberAccess);
      var isEntityParameter = memberAccess.Expression.NodeType==ExpressionType.Parameter
        && declaringType.UnderlyingType.IsAssignableFrom(memberAccess.Expression.Type);
      if (!isEntityParameter)
        return base.VisitMemberAccess(originalMemberAccess);
      memberAccessSequence.Reverse();
      var fields = reflectedType.Fields;
      FieldInfo field = null;
      foreach (var item in memberAccessSequence) {
        field = fields[item.Member.Name];
        fields = field.Fields;
      }
      // Field should be mapped to single column.
      if (field==null || field.Column==null)
        throw UnableToTranslate(originalMemberAccess, Strings.MemberAccessSequenceContainsNonPersistentFields);
      var fieldIndex = usedFields.Count;
      usedFields.Add(field);
      return Expression.Call(parameter,
        WellKnownMembers.Tuple.GenericAccessor.MakeGenericMethod(originalMemberAccess.Type),
        Expression.Constant(fieldIndex));
    }

    private static bool IsPersistentFieldAccess(MemberExpression expression)
    {
      if (!(expression.Member is PropertyInfo))
        return false;
      var ownerType = expression.Expression.Type;
      return typeof (IEntity).IsAssignableFrom(ownerType)
        || typeof (Structure).IsAssignableFrom(ownerType);
    }

    protected override Expression VisitParameter(ParameterExpression p)
    {
      // Parameters should be wiped in VisitMemberAccess.
      // If they are not for some reason fail here.
      throw UnableToTranslate(p, string.Format(Strings.ParametersOfTypeOtherThanXAreNotSupported, declaringType.UnderlyingType));
    }

    protected override Expression VisitLambda(LambdaExpression l)
    {
      throw UnableToTranslate(l, Strings.LambdaExpressionsAreNotSupported);
    }

    private InvalidOperationException UnableToTranslate(Expression expression, string reason)
    {
      return new InvalidOperationException(string.Format(Strings.ExUnableToTranslateXInPartialIndexDefinitionForIndexYReasonZ, expression, index, reason));
    }

    private PartialIndexFilterBuilder(IndexInfo index, ParameterExpression parameter)
    {
      this.index = index;
      this.parameter = parameter;
      declaringType = index.DeclaringType;
      reflectedType = index.ReflectedType;
    }
  }
}
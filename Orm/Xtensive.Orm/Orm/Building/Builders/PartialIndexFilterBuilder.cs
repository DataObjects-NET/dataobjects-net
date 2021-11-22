// Copyright (C) 2011-2021 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Denis Krjuchkov
// Created:    2011.10.07

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Xtensive.Core;
using Xtensive.Linq;
using Xtensive.Orm;
using Xtensive.Reflection;
using Xtensive.Orm.Linq;
using Xtensive.Orm.Model;

using ExpressionVisitor = Xtensive.Linq.ExpressionVisitor;
using FieldInfo = Xtensive.Orm.Model.FieldInfo;
using TypeInfo = Xtensive.Orm.Model.TypeInfo;

namespace Xtensive.Orm.Building.Builders
{
  internal class PartialIndexFilterBuilder : ExpressionVisitor
  {
    private readonly TypeInfo declaringType;
    private readonly TypeInfo reflectedType;
    private readonly IndexInfo index;
    private readonly ParameterExpression parameter;
    private readonly List<FieldInfo> usedFields = new List<FieldInfo>();
    private readonly Dictionary<Expression, FieldInfo> entityAccessMap = new Dictionary<Expression, FieldInfo>();

    public static void BuildFilter(IndexInfo index)
    {
      ArgumentValidator.EnsureArgumentNotNull(index, "index");
      var parameter = Expression.Parameter(typeof (Tuples.Tuple), "tuple");
      var builder = new PartialIndexFilterBuilder(index, parameter);
      var body = builder.Visit(index.FilterExpression.Body);
      var filter = new PartialIndexFilterInfo {
        Expression = FastExpression.Lambda(body, parameter),
        Fields = builder.usedFields,
      };
      index.Filter = filter;
    }

    protected override Expression VisitBinary(BinaryExpression b)
    {
      if (EnumRewritableOperations(b)) {
        var rawLeft = b.Left.StripCasts();
        var rawLeftType = rawLeft.Type.StripNullable();
        var rawRight = b.Right.StripCasts();
        var rawRightType = rawRight.Type.StripNullable();

        if (rawLeftType.IsEnum && rawRight.NodeType == ExpressionType.Constant) {
          var typeToCast = rawLeft.Type.IsNullable()
            ? rawLeftType.GetEnumUnderlyingType().ToNullable()
            : rawLeft.Type.GetEnumUnderlyingType();

          return base.VisitBinary(Expression.MakeBinary(
            b.NodeType,
            Expression.Convert(rawLeft, typeToCast),
            Expression.Convert(b.Right, typeToCast)));
        }
        else if (rawRightType.IsEnum && rawLeft.NodeType == ExpressionType.Constant) {
          var typeToCast = rawRight.Type.IsNullable()
            ? rawRightType.GetEnumUnderlyingType().ToNullable()
            : rawRight.Type.GetEnumUnderlyingType();

          return base.VisitBinary(Expression.MakeBinary(
            b.NodeType,
            Expression.Convert(rawRight, typeToCast),
            Expression.Convert(b.Left, typeToCast)));
        }
      }

      // Detect f!=null and f==null for entity fields

      if (!b.NodeType.In(ExpressionType.Equal, ExpressionType.NotEqual))
        return base.VisitBinary(b);

      var left = Visit(b.Left);
      var right = Visit(b.Right);

      if (entityAccessMap.TryGetValue(left, out var field) && IsNull(right))
        return BuildEntityCheck(field, b.NodeType);
      if (entityAccessMap.TryGetValue(right, out field) && IsNull(left))
        return BuildEntityCheck(field, b.NodeType);

      return base.VisitBinary(b);

      static bool EnumRewritableOperations(BinaryExpression b)
      {
        var nt = b.NodeType;
        return nt == ExpressionType.Equal || nt == ExpressionType.NotEqual
          || nt == ExpressionType.GreaterThan || nt == ExpressionType.GreaterThanOrEqual
          || nt == ExpressionType.LessThan || nt == ExpressionType.LessThanOrEqual;
      }
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
      if (memberAccessSequence.Count==0 || !IsEntityParameter(memberAccess.Expression))
        return base.VisitMemberAccess(originalMemberAccess);
      memberAccessSequence.Reverse();
      var fieldName = StringExtensions.Join(".", memberAccessSequence.Select(item => item.Member.Name));
      var field = reflectedType.Fields[fieldName];
      if (field==null)
        throw UnableToTranslate(originalMemberAccess, Strings.MemberAccessSequenceContainsNonPersistentFields);
      if (field.IsEntity) {
        EnsureCanBeUsedInFilter(originalMemberAccess, field);
        entityAccessMap[originalMemberAccess] = field;
        return originalMemberAccess;
      }
      if (field.IsPrimitive) {
        EnsureCanBeUsedInFilter(originalMemberAccess, field);
        return BuildFieldAccess(field, false);
      }
      throw UnableToTranslate(originalMemberAccess, Strings.OnlyPrimitiveAndReferenceFieldsAreSupported);
    }

    private void EnsureCanBeUsedInFilter(Expression expression, FieldInfo field)
    {
      var canBeUsed = field.ReflectedType == field.DeclaringType
        || field.IsPrimaryKey
        || field.DeclaringType.Hierarchy.InheritanceSchema!=InheritanceSchema.ClassTable;
      if (!canBeUsed)
        throw UnableToTranslate(expression, string.Format(Strings.FieldXDoesNotExistInTableForY, field.Name, field.ReflectedType));
    }

    private Expression BuildFieldAccess(FieldInfo field, bool addNullability)
    {
      var fieldIndex = usedFields.Count;
      var valueType = addNullability ? field.ValueType.ToNullable() : field.ValueType;
      usedFields.Add(field);
      return Expression.Call(parameter,
        WellKnownMembers.Tuple.GenericAccessor.MakeGenericMethod(valueType),
        Expression.Constant(fieldIndex));
    }

    private Expression BuildFieldCheck(FieldInfo field, ExpressionType nodeType)
    {
      return Expression.MakeBinary(nodeType, BuildFieldAccess(field, true), Expression.Constant(null, field.ValueType.ToNullable()));
    }

    private Expression BuildEntityCheck(FieldInfo field, ExpressionType nodeType)
    {
      var fields = field.Fields.Where(f => f.Column!=null).ToList();
      if (fields.Count==0)
        throw new InvalidOperationException();
      return fields
        .Skip(1)
        .Aggregate(BuildFieldCheck(fields[0], nodeType), (c, f) => Expression.AndAlso(c, BuildFieldCheck(f, nodeType)));
    }

    private bool IsNull(Expression expression)
    {
      return expression.NodeType==ExpressionType.Constant && ((ConstantExpression) expression).Value==null;
    }

    private bool IsEntityParameter(Expression expression)
    {
      return expression.NodeType==ExpressionType.Parameter
        && declaringType.UnderlyingType.IsAssignableFrom(expression.Type);
    }

    private bool IsPersistentFieldAccess(MemberExpression expression)
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
      throw UnableToTranslate(l);
    }

    private DomainBuilderException UnableToTranslate(Expression expression, string reason)
    {
      return new DomainBuilderException(string.Format(Strings.ExUnableToTranslateXInPartialIndexDefinitionForIndexYReasonZ, expression, index, reason));
    }

    private DomainBuilderException UnableToTranslate(Expression expression)
    {
      return UnableToTranslate(expression, string.Format(Strings.ExpressionsOfTypeXAreNotSupported, expression.NodeType));
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
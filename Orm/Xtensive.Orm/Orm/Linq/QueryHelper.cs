// Copyright (C) 2009-2024 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Denis Krjuchkov
// Created:    2009.04.02

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Xtensive.Core;
using Xtensive.Linq;
using Xtensive.Orm.Internals;
using Xtensive.Orm.Model;
using Xtensive.Reflection;
using Activator = System.Activator;
using FieldInfo = Xtensive.Orm.Model.FieldInfo;
using Tuple = Xtensive.Tuples.Tuple;

namespace Xtensive.Orm.Linq
{
  internal static class QueryHelper
  {
    private sealed class OwnerWrapper<TOwner>
    {
      public static readonly Type GenericDef = typeof(OwnerWrapper<>);

      public TOwner Owner { get; set; }

      public OwnerWrapper(TOwner owner)
      {
        Owner = owner;
      }
    }

    public static readonly ParameterExpression TupleParameter = Expression.Parameter(WellKnownOrmTypes.Tuple, "tuple");

    private static readonly PropertyInfo TupleValueProperty = WellKnownOrmTypes.ParameterOfTuple
      .GetProperty(nameof(Parameter<Tuple>.Value), WellKnownOrmTypes.Tuple);

    public static Expression<Func<Tuple, bool>> BuildFilterLambda(int startIndex, IReadOnlyList<Type> keyColumnTypes, Parameter<Tuple> keyParameter)
    {
      Expression filterExpression = null;
      var keyValue = Expression.Property(Expression.Constant(keyParameter), TupleValueProperty);
      for (int i = 0, count = keyColumnTypes.Count; i < count; i++) {
        var getValueMethod = WellKnownMembers.Tuple.GenericAccessor.CachedMakeGenericMethod(keyColumnTypes[i]);
        var tupleParameterFieldAccess = Expression.Call(
          TupleParameter,
          getValueMethod,
          Expression.Constant(startIndex + i));
        var keyParameterFieldAccess = Expression.Call(
          keyValue,
          getValueMethod,
          Expression.Constant(i));
        if (filterExpression==null)
          filterExpression = Expression.Equal(tupleParameterFieldAccess, keyParameterFieldAccess);
        else
          filterExpression = Expression.And(filterExpression,
            Expression.Equal(tupleParameterFieldAccess, keyParameterFieldAccess));
      }
      return FastExpression.Lambda<Func<Tuple, bool>>(filterExpression, TupleParameter);
    }

    private static Expression CreateEntityQuery(Type elementType, Domain domain)
    {
      return domain.RootCallExpressionsCache.GetOrAdd(elementType, (t) => Expression.Call(null, WellKnownMembers.Query.All.MakeGenericMethod(elementType)));
    }

    public static bool IsDirectEntitySetQuery(Expression entitySet)
    {
      if (entitySet.NodeType!=ExpressionType.MemberAccess)
        return false;
      var owner = ((MemberExpression) entitySet).Expression;
      if (owner.NodeType!=ExpressionType.MemberAccess)
        return false;
      var wrapper = ((MemberExpression) owner).Expression;
      return wrapper.NodeType==ExpressionType.Constant
        && wrapper.Type.IsOwnerWrapper();
    }

    public static Expression CreateDirectEntitySetQuery(EntitySetBase entitySet)
    {
      // A hack making expression to look like regular parameter
      // (ParameterExtractor.IsParameter => true)
      var owner = entitySet.Owner;
      var ownerType = owner.TypeInfo.UnderlyingType;
      var wrapper = Activator.CreateInstance(
        OwnerWrapper<int>.GenericDef.CachedMakeGenericType(ownerType), owner);
      var wrappedOwner = Expression.Property(Expression.Constant(wrapper), nameof(OwnerWrapper<int>.Owner));
      if (!entitySet.Field.IsDynamicallyDefined) {
        return Expression.Property(wrappedOwner, entitySet.Field.UnderlyingProperty);
      }
      //fast way to get indexer getter method
      var indexerGetter = ownerType.GetMethod(Reflection.WellKnown.IndexerPropertyGetterName, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
      if (indexerGetter.Attributes.HasFlag(MethodAttributes.SpecialName)
          && (indexerGetter.DeclaringType == WellKnownOrmTypes.Persistent || indexerGetter.DeclaringType == WellKnownOrmTypes.Entity))
        return Expression.Convert(Expression.Call(Expression.Constant(owner), indexerGetter, new[] { Expression.Constant(entitySet.Field.Name) }), entitySet.Field.ValueType);

      // old-fashion slow way, if something went wrong
      var indexers = ownerType.GetProperties(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)
        .Where(p => p.GetIndexParameters().Any())
        .Select(p => p.GetGetMethod());
      return Expression.Convert(Expression.Call(Expression.Constant(owner),indexers.Single(), new []{Expression.Constant(entitySet.Field.Name)}), entitySet.Field.ValueType);
    }

    public static Expression CreateEntitySetQuery(Expression ownerEntity, FieldInfo field, Domain domain)
    {
      if (!field.IsDynamicallyDefined && !field.UnderlyingProperty.PropertyType.IsOfGenericType(WellKnownOrmTypes.EntitySetOfT)) {
        throw Exceptions.InternalError(Strings.ExFieldMustBeOfEntitySetType, OrmLog.Instance);
      }
      if (field.IsDynamicallyDefined && !field.ValueType.IsOfGenericType(WellKnownOrmTypes.EntitySetOfT)) {
        throw Exceptions.InternalError(Strings.ExFieldMustBeOfEntitySetType, OrmLog.Instance);
      }

      var elementType = field.ItemType;
      var association = field.Associations.Last();
      if (association.Multiplicity==Multiplicity.OneToMany) {
        var targetField = association.TargetType.Fields[association.Reversed.OwnerField.Name];
        var whereParameter = Expression.Parameter(elementType, "p");
        var expression = BuildExpressionForFieldRecursivly(targetField, whereParameter);
        var whereExpression = Expression.Equal(
          Expression.Property(
            expression,
            WellKnownMembers.IEntityKey),
          Expression.Property(
            ownerEntity,
            WellKnownMembers.IEntityKey)
          );
        return Expression.Call(
          WellKnownMembers.Queryable.Where.CachedMakeGenericMethod(elementType),
          CreateEntityQuery(elementType, domain),
          FastExpression.Lambda(whereExpression, whereParameter)
          );
      }

      var connectorType = association.AuxiliaryType.UnderlyingType;
      var referencingField = association.IsMaster
        ? association.AuxiliaryType.Fields[WellKnown.SlaveFieldName]
        : association.AuxiliaryType.Fields[WellKnown.MasterFieldName];
      var referencedField = association.IsMaster
        ? association.AuxiliaryType.Fields[WellKnown.MasterFieldName]
        : association.AuxiliaryType.Fields[WellKnown.SlaveFieldName];

      var filterParameter = Expression.Parameter(connectorType, "t");
      var filterExpression = Expression.Equal(
        Expression.MakeMemberAccess(
          Expression.MakeMemberAccess(filterParameter, referencedField.UnderlyingProperty),
          WellKnownMembers.IEntityKey),
        Expression.MakeMemberAccess(
          ownerEntity,
          WellKnownMembers.IEntityKey)
        );

      var outerQuery = Expression.Call(
        WellKnownMembers.Queryable.Where.CachedMakeGenericMethod(connectorType),
        CreateEntityQuery(connectorType, domain),
        FastExpression.Lambda(filterExpression, filterParameter)
        );

      var outerSelectorParameter = Expression.Parameter(connectorType, "o");
      var outerSelector = FastExpression.Lambda(
        Expression.MakeMemberAccess(outerSelectorParameter, referencingField.UnderlyingProperty),
        outerSelectorParameter);
      var innerSelectorParameter = Expression.Parameter(elementType, "i");
      var innerSelector = FastExpression.Lambda(innerSelectorParameter, innerSelectorParameter);
      var resultSelector = FastExpression.Lambda(innerSelectorParameter, outerSelectorParameter, innerSelectorParameter);

      var innerQuery = CreateEntityQuery(elementType, domain);
      var joinMethodInfo = WellKnownMembers.Queryable.Join
        .MakeGenericMethod(new[] {
          connectorType,
          elementType,
          outerSelector.Body.Type,
          resultSelector.Body.Type
        });
      return Expression.Call(joinMethodInfo,
        outerQuery,
        innerQuery,
        outerSelector,
        innerSelector,
        resultSelector);
    }

    public static void TryAddConvarianceCast(ref Expression source, Type baseType)
    {
      var elementType = GetSequenceElementType(source.Type);
      if (elementType==null)
        return;
      if (!baseType.IsAssignableFrom(elementType) || baseType==elementType)
        return;
      var castMethod = source.Type.IsOfGenericInterface(WellKnownInterfaces.QueryableOfT)
        ? WellKnownMembers.Queryable.Cast
        : WellKnownMembers.Enumerable.Cast;
      source = Expression.Call(castMethod.CachedMakeGenericMethod(baseType), source);
    }

    public static Type GetSequenceElementType(Type type)
    {
      var sequenceType = type.GetGenericInterface(WellKnownInterfaces.EnumerableOfT);
      return sequenceType?.GetGenericArguments()[0];
    }

    private static Expression BuildExpressionForFieldRecursivly(FieldInfo field, Expression parameter)
    {
      if (field.IsNested) {
        var expression = BuildExpressionForFieldRecursivly(field.Parent, parameter);
        return Expression.Property(expression, field.DeclaringField.UnderlyingProperty);
      }
      return Expression.Property(parameter, field.DeclaringField.UnderlyingProperty);
    }

    private static bool IsOwnerWrapper(this Type type) =>
      (type.MetadataToken ^ OwnerWrapper<int>.GenericDef.MetadataToken) == 0
        && ReferenceEquals(type.Module, OwnerWrapper<int>.GenericDef.Module);
  }
}

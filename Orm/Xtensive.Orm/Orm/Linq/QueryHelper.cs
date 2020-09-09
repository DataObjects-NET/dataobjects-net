// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
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
using Xtensive.Orm.Model;
using Xtensive.Reflection;
using FieldInfo = Xtensive.Orm.Model.FieldInfo;
using Tuple = Xtensive.Tuples.Tuple;

namespace Xtensive.Orm.Linq
{
  internal static class QueryHelper
  {
    private sealed class OwnerWrapper<TOwner>
    {
      public TOwner Owner { get; set; }

      public OwnerWrapper(TOwner owner)
      {
        Owner = owner;
      }
    }

    public static Expression<Func<Tuple, bool>> BuildFilterLambda(int startIndex, IReadOnlyList<Type> keyColumnTypes, Parameter<Tuple> keyParameter)
    {
      Expression filterExpression = null;
      var tupleParameter = Expression.Parameter(typeof (Tuple), "tuple");
      var valueProperty = typeof (Parameter<Tuple>).GetProperty("Value", typeof (Tuple));
      var keyValue = Expression.Property(Expression.Constant(keyParameter), valueProperty);
      for (var i = 0; i < keyColumnTypes.Count; i++) {
        var getValueMethod = WellKnownMembers.Tuple.GenericAccessor.MakeGenericMethod(keyColumnTypes[i]);
        var tupleParameterFieldAccess = Expression.Call(
          tupleParameter,
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
      return FastExpression.Lambda<Func<Tuple, bool>>(filterExpression, tupleParameter);
    }

    private static Expression CreateEntityQuery(Type elementType)
    {
      var queryAll = WellKnownMembers.Query.All.MakeGenericMethod(elementType);
      return Expression.Call(null, queryAll);
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
        && wrapper.Type.IsGenericType
        && wrapper.Type.GetGenericTypeDefinition()==typeof (OwnerWrapper<>);
    }

    public static Expression CreateDirectEntitySetQuery(EntitySetBase entitySet)
    {
      // A hack making expression to look like regular parameter 
      // (ParameterExtractor.IsParameter => true)
      var owner = entitySet.Owner;
      var wrapper = Activator.CreateInstance(
        typeof (OwnerWrapper<>).MakeGenericType(owner.GetType()), owner);
      var wrappedOwner = Expression.Property(Expression.Constant(wrapper), "Owner");
      if (!entitySet.Field.IsDynalicallyDefined)
        return Expression.Property(wrappedOwner, entitySet.Field.UnderlyingProperty);
      
      var indexers = owner.GetType().GetProperties(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)
        .Where(p => p.GetIndexParameters().Any())
        .Select(p => p.GetGetMethod());
      return Expression.Convert(Expression.Call(Expression.Constant(owner),indexers.Single(), new []{Expression.Constant(entitySet.Field.Name)}), entitySet.Field.ValueType);
    }

    public static Expression CreateEntitySetQuery(Expression ownerEntity, FieldInfo field)
    {
      if (!field.IsDynalicallyDefined && !field.UnderlyingProperty.PropertyType.IsOfGenericType(typeof (EntitySet<>)))
        throw Exceptions.InternalError(Strings.ExFieldMustBeOfEntitySetType, OrmLog.Instance);
      if (field.IsDynalicallyDefined && !field.ValueType.IsOfGenericType(typeof (EntitySet<>)))
        throw Exceptions.InternalError(Strings.ExFieldMustBeOfEntitySetType, OrmLog.Instance);

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
          WellKnownMembers.Queryable.Where.MakeGenericMethod(elementType),
          CreateEntityQuery(elementType),
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
        WellKnownMembers.Queryable.Where.MakeGenericMethod(connectorType),
        CreateEntityQuery(connectorType),
        FastExpression.Lambda(filterExpression, filterParameter)
        );

      var outerSelectorParameter = Expression.Parameter(connectorType, "o");
      var outerSelector = FastExpression.Lambda(
        Expression.MakeMemberAccess(outerSelectorParameter, referencingField.UnderlyingProperty),
        outerSelectorParameter);
      var innerSelectorParameter = Expression.Parameter(elementType, "i");
      var innerSelector = FastExpression.Lambda(innerSelectorParameter, innerSelectorParameter);
      var resultSelector = FastExpression.Lambda(innerSelectorParameter, outerSelectorParameter, innerSelectorParameter);

      var innerQuery = CreateEntityQuery(elementType);
      var joinMethodInfo = typeof (Queryable).GetMethods()
        .Single(mi => mi.Name==Xtensive.Reflection.WellKnown.Queryable.Join && mi.IsGenericMethod && mi.GetParameters().Length==5)
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
      var castMethod = source.Type.IsOfGenericInterface(typeof (IQueryable<>))
        ? WellKnownMembers.Queryable.Cast
        : WellKnownMembers.Enumerable.Cast;
      source = Expression.Call(castMethod.MakeGenericMethod(baseType), source);
    }

    public static Type GetSequenceElementType(Type type)
    {
      var sequenceType = type.GetGenericInterface(typeof (IEnumerable<>));
      return sequenceType!=null ? sequenceType.GetGenericArguments()[0] : null;
    }

    private static Expression BuildExpressionForFieldRecursivly(FieldInfo field, Expression parameter)
    {
      if (field.IsNested) {
        var expression = BuildExpressionForFieldRecursivly(field.Parent, parameter);
        return Expression.Property(expression, field.DeclaringField.UnderlyingProperty);
      }
      return Expression.Property(parameter, field.DeclaringField.UnderlyingProperty);
    }
  }
}

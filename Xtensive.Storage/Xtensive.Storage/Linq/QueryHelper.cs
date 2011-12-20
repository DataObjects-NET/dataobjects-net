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
using Xtensive.Core.Linq;
using Xtensive.Core.Parameters;
using Xtensive.Core.Reflection;
using Xtensive.Core.Tuples;
using Tuple = Xtensive.Core.Tuples.Tuple;
using Xtensive.Storage.Model;
using Xtensive.Storage.Resources;
using FieldInfo = Xtensive.Storage.Model.FieldInfo;

namespace Xtensive.Storage.Linq
{
  internal static class QueryHelper
  {
    public static Expression<Func<Tuple,bool>> BuildFilterLambda(int startIndex, IList<Type> keyColumnTypes, Parameter<Tuple> keyParameter)
    {
      Expression filterExpression = null;
      var tupleParameter = Expression.Parameter(typeof(Tuple), "tuple");
      var valueProperty = typeof(Parameter<Tuple>).GetProperty("Value", typeof(Tuple));
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
        if (filterExpression == null)
          filterExpression = Expression.Equal(tupleParameterFieldAccess, keyParameterFieldAccess);
        else
          filterExpression = Expression.And(filterExpression,
            Expression.Equal(tupleParameterFieldAccess, keyParameterFieldAccess));
      }
      return Expression.Lambda<Func<Tuple, bool>>(filterExpression, tupleParameter);
    }

    public static Expression CreateEntityQueryExpression(Type elementType)
    {
      var queryAll = WellKnownMembers.Query.All.MakeGenericMethod(elementType);
      return Expression.Call(null, queryAll);
    }

    public static Expression CreateEntitySetQueryExpression(Expression ownerEntity, FieldInfo field)
    {
      if (!field.UnderlyingProperty.PropertyType.IsOfGenericType(typeof(EntitySet<>)))
        throw Exceptions.InternalError(Strings.ExFieldMustBeOfEntitySetType, Log.Instance);

      var elementType = field.ItemType;
      var association = field.Associations.Last();
      if (association.Multiplicity == Multiplicity.OneToMany) {
        var whereParameter = Expression.Parameter(elementType, "p");
        var whereExpression = Expression.Equal(
          Expression.Property(
            Expression.Property(whereParameter, association.Reversed.OwnerField.Name),
            WellKnownMembers.IEntityKey),
          Expression.Property(
            ownerEntity,
            WellKnownMembers.IEntityKey)
          );
        return Expression.Call(
          WellKnownMembers.Queryable.Where.MakeGenericMethod(elementType),
          CreateEntityQueryExpression(elementType),
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
        CreateEntityQueryExpression(connectorType),
        FastExpression.Lambda(filterExpression, filterParameter)
        );

      var outerSelectorParameter = Expression.Parameter(connectorType, "o");
      var outerSelector = FastExpression.Lambda(
        Expression.MakeMemberAccess(outerSelectorParameter, referencingField.UnderlyingProperty),
        outerSelectorParameter);
      var innerSelectorParameter = Expression.Parameter(elementType, "i");
      var innerSelector = FastExpression.Lambda(innerSelectorParameter, innerSelectorParameter);
      var resultSelector = FastExpression.Lambda(innerSelectorParameter, outerSelectorParameter, innerSelectorParameter);

      var innerQuery = CreateEntityQueryExpression(elementType);
      var joinMethodInfo = typeof (Queryable).GetMethods()
        .Single(mi => mi.Name == Core.Reflection.WellKnown.Queryable.Join && mi.IsGenericMethod && mi.GetParameters().Length == 5)
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

    [Conditional("NET40")]
    public static void TryAddConvarianceCast(ref Expression source, Type baseType)
    {
      var elementType = GetSequenceElementType(source.Type);
      if (elementType == null)
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
      var sequenceType =  type.GetGenericType(typeof (IEnumerable<>))
        ?? type.GetInterfaces().Select(i => i.GetGenericType(typeof (IEnumerable<>))).FirstOrDefault(i => i!=null);
      return sequenceType != null ? sequenceType.GetGenericArguments()[0] : null;
    }
  }
}

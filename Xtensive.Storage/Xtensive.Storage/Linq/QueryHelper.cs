// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.04.02

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Xtensive.Core;
using Xtensive.Core.Linq;
using Xtensive.Core.Parameters;
using Xtensive.Core.Reflection;
using Xtensive.Core.Tuples;
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
      var query = WellKnownMembers.Query.All
        .MakeGenericMethod(elementType)
        .Invoke(null, null);
      return Expression.Constant(query, typeof(IQueryable<>).MakeGenericType(elementType));
    }

    public static Expression CreateEntitySetQueryExpression(Expression ownerEntity, FieldInfo field)
    {
      if (!field.UnderlyingProperty.PropertyType.IsOfGenericType(typeof(EntitySet<>)))
        throw Exceptions.InternalError(Strings.ExFieldMustBeOfEntitySetType, Log.Instance);

      var elementType = field.ItemType;
      var association = field.Association;
      if (association.Multiplicity == Multiplicity.OneToMany) {
        var whereParameter = Expression.Parameter(elementType, "p");
        var whereExpression = Expression.Equal(
          Expression.Property(
            Expression.Property(whereParameter, association.Reversed.OwnerField.Name),
            WellKnown.KeyFieldName),
          Expression.Property(
            ownerEntity,
            WellKnown.KeyFieldName)
          );
        return Expression.Call(
          WellKnownMembers.Queryable.Where.MakeGenericMethod(elementType),
          CreateEntityQueryExpression(elementType),
          FastExpression.Lambda(whereExpression, whereParameter)
          );
      }

      var connectorType = association.AuxiliaryType.UnderlyingType;
      var referencedType = association.IsMaster
        ? association.OwnerType
        : association.TargetType;
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
  }
}

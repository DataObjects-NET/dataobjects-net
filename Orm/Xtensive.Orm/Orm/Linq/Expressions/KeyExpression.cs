// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2009.05.05

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Xtensive.Collections;
using Xtensive.Core;
using Xtensive.Orm.Model;

namespace Xtensive.Orm.Linq.Expressions
{
  internal class KeyExpression : PersistentFieldExpression
  {
    public TypeInfo EntityType { get; private set; }
    public System.Collections.ObjectModel.ReadOnlyCollection<FieldExpression> KeyFields { get; private set; }

    public override Expression Remap(int offset, Dictionary<Expression, Expression> processedExpressions)
    {
      if (!CanRemap)
        return this;

      Expression value;
      if (processedExpressions.TryGetValue(this, out value))
        return value;

      var mapping = new Segment<int>(Mapping.Offset + offset, Mapping.Length);
      var fields = KeyFields
        .Select(f => (FieldExpression)f.Remap(offset, processedExpressions))
        .ToList()
        .AsReadOnly();
      var result = new KeyExpression(EntityType, fields, mapping, UnderlyingProperty, OuterParameter, DefaultIfEmpty);

      processedExpressions.Add(this, result);
      return result;
    }

    public override Expression Remap(int[] map, Dictionary<Expression, Expression> processedExpressions)
    {
      if (!CanRemap)
        return this;

      Expression value;
      if (processedExpressions.TryGetValue(this, out value))
        return value;

      var segment = new Segment<int>(map.IndexOf(Mapping.Offset), Mapping.Length);
      System.Collections.ObjectModel.ReadOnlyCollection<FieldExpression> fields;
      using (new SkipOwnerCheckScope()) {
        fields = KeyFields
          .Select(f => f.Remap(map, processedExpressions))
          .Where(f => f != null)
          .Cast<FieldExpression>()
          .ToList()
          .AsReadOnly();
      }
      if (fields.Count != KeyFields.Count) {
        if (SkipOwnerCheckScope.IsActive) {
          processedExpressions.Add(this, null);
          return null;
        }
        throw Exceptions.InternalError(Strings.ExUnableToRemapKeyExpression, Log.Instance);
      }
      var result = new KeyExpression(EntityType, fields, segment, UnderlyingProperty, OuterParameter, DefaultIfEmpty);

      processedExpressions.Add(this, result);
      return result;
    }

    public override Expression BindParameter(ParameterExpression parameter, Dictionary<Expression, Expression> processedExpressions)
    {
      Expression value;
      if (processedExpressions.TryGetValue(this, out value))
        return value;

      var fields = KeyFields
        .Select(f => (FieldExpression)f.BindParameter(parameter, processedExpressions))
        .ToList()
        .AsReadOnly();
      var result = new KeyExpression(EntityType, fields, Mapping, UnderlyingProperty, parameter, DefaultIfEmpty);

      processedExpressions.Add(this, result);
      return result;
    }

    public override Expression RemoveOuterParameter(Dictionary<Expression, Expression> processedExpressions)
    {
      Expression value;
      if (processedExpressions.TryGetValue(this, out value))
        return value;

      var fields = KeyFields
        .Select(f => (FieldExpression)f.RemoveOuterParameter(processedExpressions))
        .ToList()
        .AsReadOnly();
      var result = new KeyExpression(EntityType, fields, Mapping, UnderlyingProperty, null, DefaultIfEmpty);

      processedExpressions.Add(this, result);
      return result;
    }

    public static KeyExpression Create(TypeInfo entityType, int offset)
    {
      var mapping = new Segment<int>(offset, entityType.Key.TupleDescriptor.Count);
      var fields = entityType.Columns
        .Where(c => c.IsPrimaryKey)
        .OrderBy(c => c.Field.MappingInfo.Offset)
        .Select(c => FieldExpression.CreateField(c.Field, offset))
        .ToList()
        .AsReadOnly();
      return new KeyExpression(entityType, fields, mapping,WellKnownMembers.IEntityKey, null, false);
    }


    // Constructors

    private KeyExpression(
      TypeInfo entityType, 
      System.Collections.ObjectModel.ReadOnlyCollection<FieldExpression> keyFields, 
      Segment<int> segment, 
      PropertyInfo underlyingProperty, 
      ParameterExpression parameterExpression, 
      bool defaultIfEmpty)
      : base(ExtendedExpressionType.Key, WellKnown.KeyFieldName, typeof(Key), segment, underlyingProperty, parameterExpression, defaultIfEmpty)
    {
      EntityType = entityType;
      KeyFields = keyFields;
    }
  }
}
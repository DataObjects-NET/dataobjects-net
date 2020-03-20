// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2009.05.05

using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Xtensive.Core;
using Xtensive.Orm.Model;
using TypeInfo = Xtensive.Orm.Model.TypeInfo;

namespace Xtensive.Orm.Linq.Expressions
{
  internal class KeyExpression : PersistentFieldExpression
  {
    public TypeInfo EntityType { get; }
    public IReadOnlyList<FieldExpression> KeyFields { get; }

    public override Expression Remap(int offset, Dictionary<Expression, Expression> processedExpressions)
    {
      if (!CanRemap) {
        return this;
      }

      return processedExpressions.TryGetValue(this, out var value)
        ? value
        : RemapWithNoCheck(offset, processedExpressions);
    }

    // Having this code as a separate method helps to avoid closure allocation during Remap call
    // in case processedExpressions dictionary already contains a result.
    private Expression RemapWithNoCheck(int offset, Dictionary<Expression, Expression> processedExpressions)
    {
      var mapping = new Segment<int>(Mapping.Offset + offset, Mapping.Length);

      FieldExpression Remap(FieldExpression f) => (FieldExpression) f.Remap(offset, processedExpressions);

      var fields = KeyFields.Select(Remap).ToArray(KeyFields.Count);
      var result = new KeyExpression(EntityType, fields, mapping, UnderlyingProperty, OuterParameter, DefaultIfEmpty);

      processedExpressions.Add(this, result);
      return result;
    }

    public override Expression Remap(int[] map, Dictionary<Expression, Expression> processedExpressions)
    {
      if (!CanRemap) {
        return this;
      }

      if (processedExpressions.TryGetValue(this, out var value)) {
        return value;
      }

      var segment = new Segment<int>(map.IndexOf(Mapping.Offset), Mapping.Length);
      var fields = new FieldExpression[KeyFields.Count];
      using (new SkipOwnerCheckScope()) {
        for (var index = 0; index < fields.Length; index++) {
          var field = (FieldExpression)KeyFields[index].Remap(map, processedExpressions);
          if (field == null) {
            if (SkipOwnerCheckScope.IsActive) {
              processedExpressions.Add(this, null);
              return null;
            }
            throw Exceptions.InternalError(Strings.ExUnableToRemapKeyExpression, OrmLog.Instance);
          }

          fields[index] = field;
        }
      }
      var result = new KeyExpression(EntityType, fields, segment, UnderlyingProperty, OuterParameter, DefaultIfEmpty);

      processedExpressions.Add(this, result);
      return result;
    }

    public override Expression BindParameter(
      ParameterExpression parameter, Dictionary<Expression, Expression> processedExpressions)
    {
      if (processedExpressions.TryGetValue(this, out var value)) {
        return value;
      }

      return BindParameterWithNoCheck(parameter, processedExpressions);
    }

    // Having this code as a separate method helps to avoid closure allocation during BindParameter call
    // in case processedExpressions dictionary already contains a result.
    private Expression BindParameterWithNoCheck(
      ParameterExpression parameter, Dictionary<Expression, Expression> processedExpressions)
    {
      FieldExpression BindParameter(FieldExpression f)
        => (FieldExpression) f.BindParameter(parameter, processedExpressions);

      var fields = KeyFields.Select(BindParameter).ToArray(KeyFields.Count);
      var result = new KeyExpression(EntityType, fields, Mapping, UnderlyingProperty, parameter, DefaultIfEmpty);

      processedExpressions.Add(this, result);
      return result;
    }

    public override Expression RemoveOuterParameter(Dictionary<Expression, Expression> processedExpressions)
    {
      if (processedExpressions.TryGetValue(this, out var value)) {
        return value;
      }

      return RemoveOuterParameterWithNoCheck(processedExpressions);
    }

    // Having this code as a separate method helps to avoid closure allocation during RemoveOuterParameter call
    // in case processedExpressions dictionary already contains a result.
    private Expression RemoveOuterParameterWithNoCheck(Dictionary<Expression, Expression> processedExpressions)
    {
      FieldExpression RemoveOuterParameter(FieldExpression f)
        => (FieldExpression) f.RemoveOuterParameter(processedExpressions);

      var fields = KeyFields.Select(RemoveOuterParameter).ToArray(KeyFields.Count);
      var result = new KeyExpression(EntityType, fields, Mapping, UnderlyingProperty, null, DefaultIfEmpty);

      processedExpressions.Add(this, result);
      return result;
    }

    public static KeyExpression Create(TypeInfo entityType, int offset)
    {
      var mapping = new Segment<int>(offset, entityType.Key.TupleDescriptor.Count);

      FieldExpression CreateField(ColumnInfo c) => FieldExpression.CreateField(c.Field, offset);

      var fields = entityType.IsLocked
        ? entityType.Key.Columns.Select(CreateField).ToArray(entityType.Key.Columns.Count)
        : entityType.Columns
          .Where(c => c.IsPrimaryKey)
          .OrderBy(c => c.Field.MappingInfo.Offset)
          .Select(CreateField)
          .ToArray();
      return new KeyExpression(entityType, fields, mapping, WellKnownMembers.IEntityKey, null, false);
    }


    // Constructors

    private KeyExpression(
      TypeInfo entityType, 
      IReadOnlyList<FieldExpression> keyFields,
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
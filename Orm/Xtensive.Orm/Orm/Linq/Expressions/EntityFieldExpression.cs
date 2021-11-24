// Copyright (C) 2009-2021 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Alexis Kochetov
// Created:    2009.05.06

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Xtensive.Core;
using Xtensive.Orm.Model;

namespace Xtensive.Orm.Linq.Expressions
{
  internal sealed class EntityFieldExpression : FieldExpression,
    IEntityExpression
  {
    private readonly List<PersistentFieldExpression> fields;

    public TypeInfo PersistentType { get; }
    public List<PersistentFieldExpression> Fields => fields;
    public KeyExpression Key { get; }
    public EntityExpression Entity { get; private set; }

    public bool IsNullable => (Owner != null && Owner.IsNullable) || Field.IsNullable;

    public void RegisterEntityExpression(int offset)
    {
      Entity = EntityExpression.Create(this, offset);
      Entity.IsNullable = IsNullable;
    }

    public override Expression Remap(int offset, Dictionary<Expression, Expression> processedExpressions)
    {
      if (!CanRemap) {
        return this;
      }

      if (processedExpressions.TryGetValue(this, out var result)) {
        return result;
      }

      var newFields = new List<PersistentFieldExpression>(fields.Count);
      foreach (var field in fields) {
        // Do not convert to LINQ. We want to avoid a closure creation here.
        newFields.Add((PersistentFieldExpression) field.Remap(offset, processedExpressions));
      }

      var keyExpression = (KeyExpression) Key.Remap(offset, processedExpressions);
      var entity = (EntityExpression) Entity?.Remap(offset, processedExpressions);
      result = new EntityFieldExpression(
        PersistentType, Field, newFields, keyExpression.Mapping, keyExpression, entity, OuterParameter, DefaultIfEmpty);
      if (Owner == null) {
        return result;
      }

      processedExpressions.Add(this, result);
      Owner.Remap(offset, processedExpressions);
      return result;
    }

    public override Expression Remap(IReadOnlyList<int> map, Dictionary<Expression, Expression> processedExpressions)
    {
      if (!CanRemap) {
        return this;
      }

      if (processedExpressions.TryGetValue(this, out var result)) {
        return result;
      }

      var newFields = new List<PersistentFieldExpression>(fields.Count);
      using (new SkipOwnerCheckScope()) {
        foreach (var field in fields) {
          // Do not convert to LINQ. We want to avoid a closure creation here.
          var mappedField = (PersistentFieldExpression) field.Remap(map, processedExpressions);
          if (mappedField == null) {
            continue;
          }

          newFields.Add(mappedField);
        }
      }

      if (newFields.Count != Fields.Count) {
        processedExpressions.Add(this, null);
        return null;
      }

      var keyExpression = (KeyExpression) Key.Remap(map, processedExpressions);
      EntityExpression entity;
      using (new SkipOwnerCheckScope()) {
        entity = (EntityExpression) Entity?.Remap(map, processedExpressions);
      }

      result = new EntityFieldExpression(
        PersistentType, Field, newFields, keyExpression.Mapping, keyExpression, entity, OuterParameter, DefaultIfEmpty);
      if (Owner == null) {
        return result;
      }

      processedExpressions.Add(this, result);
      Owner.Remap(map, processedExpressions);
      return result;
    }

    public override Expression BindParameter(
      ParameterExpression parameter, Dictionary<Expression, Expression> processedExpressions)
    {
      if (processedExpressions.TryGetValue(this, out var result)) {
        return result;
      }

      var newFields = new List<PersistentFieldExpression>(fields.Count);
      foreach (var field in fields) {
        // Do not convert to LINQ. We want to avoid a closure creation here.
        newFields.Add((PersistentFieldExpression) field.BindParameter(parameter, processedExpressions));
      }
      var keyExpression = (KeyExpression) Key.BindParameter(parameter, processedExpressions);
      var entity = (EntityExpression) Entity?.BindParameter(parameter, processedExpressions);
      result = new EntityFieldExpression(
        PersistentType, Field, newFields, Mapping, keyExpression, entity, parameter, DefaultIfEmpty);
      if (Owner == null) {
        return result;
      }

      processedExpressions.Add(this, result);
      Owner.BindParameter(parameter, processedExpressions);
      return result;
    }

    public override Expression RemoveOuterParameter(Dictionary<Expression, Expression> processedExpressions)
    {
      if (processedExpressions.TryGetValue(this, out var result)) {
        return result;
      }

      var newFields = new List<PersistentFieldExpression>(fields.Count);
      foreach (var field in fields) {
        // Do not convert to LINQ. We want to avoid a closure creation here.
        newFields.Add((PersistentFieldExpression) field.RemoveOuterParameter(processedExpressions));
      }
      var keyExpression = (KeyExpression) Key.RemoveOuterParameter(processedExpressions);
      var entity = (EntityExpression) Entity?.RemoveOuterParameter(processedExpressions);
      result = new EntityFieldExpression(
        PersistentType, Field, newFields, Mapping, keyExpression, entity, null, DefaultIfEmpty);
      if (Owner == null) {
        return result;
      }

      processedExpressions.Add(this, result);
      Owner.RemoveOuterParameter(processedExpressions);
      return result;
    }

    public override FieldExpression RemoveOwner() =>
      new EntityFieldExpression(PersistentType, Field, Fields, Mapping, Key, Entity, OuterParameter, DefaultIfEmpty);

    public static EntityFieldExpression CreateEntityField(FieldInfo entityField, int offset)
    {
      if (!entityField.IsEntity) {
        throw new ArgumentException(string.Format(Strings.ExFieldXIsNotEntity, entityField.Name), nameof(entityField));
      }

      var entityType = entityField.ValueType;
      var persistentType = entityField.ReflectedType.Model.Types[entityType];

      var mappingInfo = entityField.MappingInfo;
      var mapping = new Segment<int>(mappingInfo.Offset + offset, mappingInfo.Length);
      var keyFields = persistentType.Key.Fields;
      var keyExpression = KeyExpression.Create(persistentType, offset + mappingInfo.Offset);
      var fields = new List<PersistentFieldExpression>(keyFields.Count + 1) { keyExpression };
      foreach (var field in keyFields) {
        // Do not convert to LINQ. We want to avoid a closure creation here.
        fields.Add(BuildNestedFieldExpression(field, offset + mappingInfo.Offset));
      }

      return new EntityFieldExpression(persistentType, entityField, fields, mapping, keyExpression, null, null, false);
    }

    private static PersistentFieldExpression BuildNestedFieldExpression(FieldInfo nestedField, int offset)
    {
      if (nestedField.IsPrimitive) {
        return CreateField(nestedField, offset);
      }

      if (nestedField.IsEntity) {
        return CreateEntityField(nestedField, offset);
      }

      throw new NotSupportedException(string.Format(Strings.ExNestedFieldXIsNotSupported, nestedField.Attributes));
    }


    // Constructors

    private EntityFieldExpression(
      TypeInfo persistentType,
      FieldInfo field,
      List<PersistentFieldExpression> fields,
      in Segment<int> mapping,
      KeyExpression key,
      EntityExpression entity,
      ParameterExpression parameterExpression,
      bool defaultIfEmpty)
      : base(ExtendedExpressionType.EntityField, field, mapping, parameterExpression, defaultIfEmpty)
    {
      PersistentType = persistentType;
      this.fields = fields;
      Key = key;
      Entity = entity;
    }
  }
}
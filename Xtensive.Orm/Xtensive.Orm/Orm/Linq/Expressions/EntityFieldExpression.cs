// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2009.05.06

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Xtensive.Core;
using Xtensive.Orm.Model;

namespace Xtensive.Orm.Linq.Expressions
{
  internal sealed class EntityFieldExpression : FieldExpression,
    IEntityExpression
  {
    public TypeInfo PersistentType { get; private set; }
    public List<PersistentFieldExpression> Fields { get; private set; }
    public KeyExpression Key { get; private set; }
    public EntityExpression Entity { get; private set; }

    public bool IsNullable 
    { 
      get { return Owner != null && Owner.IsNullable || Field.IsNullable; }
    }

    public void RegisterEntityExpression(int offset)
    {
      Entity = EntityExpression.Create(this, offset);
      Entity.IsNullable = IsNullable;
    }

    public override Expression Remap(int offset, Dictionary<Expression, Expression> processedExpressions)
    {
      if (!CanRemap)
        return this;

      Expression result;
      if (processedExpressions.TryGetValue(this, out result))
        return result;

      var fields = Fields
        .Select(f => f.Remap(offset, processedExpressions))
        .Cast<PersistentFieldExpression>()
        .ToList();
      var keyExpression = (KeyExpression) Key.Remap(offset, processedExpressions);
      var entity = Entity!=null
        ? (EntityExpression) Entity.Remap(offset, processedExpressions)
        : null;
      result = new EntityFieldExpression(PersistentType, Field, fields, keyExpression.Mapping, keyExpression, entity, OuterParameter, DefaultIfEmpty);
      if (Owner==null)
        return result;

      processedExpressions.Add(this, result);
      Owner.Remap(offset, processedExpressions);
      return result;
    }

    public override Expression Remap(int[] map, Dictionary<Expression, Expression> processedExpressions)
    {
      if (!CanRemap)
        return this;

      Expression result;
      if (processedExpressions.TryGetValue(this, out result))
        return result;

      List<PersistentFieldExpression> fields;
      using (new SkipOwnerCheckScope()) {
        fields = Fields
          .Select(f => f.Remap(map, processedExpressions))
          .Where(f => f!=null)
          .Cast<PersistentFieldExpression>()
          .ToList();
      }
      if (fields.Count!=Fields.Count) {
        processedExpressions.Add(this, null);
        return null;
      }
      var keyExpression = (KeyExpression) Key.Remap(map, processedExpressions);
      EntityExpression entity;
      using (new SkipOwnerCheckScope())
        entity = Entity!=null
          ? (EntityExpression) Entity.Remap(map, processedExpressions)
          : null;
      result = new EntityFieldExpression(PersistentType, Field, fields, keyExpression.Mapping, keyExpression, entity, OuterParameter, DefaultIfEmpty);
      if (Owner==null)
        return result;

      processedExpressions.Add(this, result);
      Owner.Remap(map, processedExpressions);
      return result;
    }

    public override Expression BindParameter(ParameterExpression parameter, Dictionary<Expression, Expression> processedExpressions)
    {
      Expression result;
      if (processedExpressions.TryGetValue(this, out result))
        return result;

      var fields = Fields
        .Select(f => f.BindParameter(parameter, processedExpressions))
        .Cast<PersistentFieldExpression>()
        .ToList();
      var keyExpression = (KeyExpression) Key.BindParameter(parameter, processedExpressions);
      var entity = Entity!=null
        ? (EntityExpression) Entity.BindParameter(parameter, processedExpressions)
        : null;
      result = new EntityFieldExpression(PersistentType, Field, fields, Mapping, keyExpression, entity, parameter, DefaultIfEmpty);
      if (Owner==null)
        return result;

      processedExpressions.Add(this, result);
      Owner.BindParameter(parameter, processedExpressions);
      return result;
    }

    public override Expression RemoveOuterParameter(Dictionary<Expression, Expression> processedExpressions)
    {
      Expression result;
      if (processedExpressions.TryGetValue(this, out result))
        return result;

      var fields = Fields
        .Select(f => f.RemoveOuterParameter(processedExpressions))
        .Cast<PersistentFieldExpression>()
        .ToList();
      var keyExpression = (KeyExpression) Key.RemoveOuterParameter(processedExpressions);
      var entity = Entity!=null
        ? (EntityExpression) Entity.RemoveOuterParameter(processedExpressions)
        : null;
      result = new EntityFieldExpression(PersistentType, Field, fields, Mapping, keyExpression, entity, null, DefaultIfEmpty);
      if (Owner==null)
        return result;

      processedExpressions.Add(this, result);
      Owner.RemoveOuterParameter(processedExpressions);
      return result;
    }

    public override FieldExpression RemoveOwner()
    {
      return new EntityFieldExpression(PersistentType, Field, Fields, Mapping, Key, Entity, OuterParameter, DefaultIfEmpty);
    }

    public static EntityFieldExpression CreateEntityField(FieldInfo entityField, int offset)
    {
      if (!entityField.IsEntity)
        throw new ArgumentException(string.Format(Resources.Strings.ExFieldXIsNotEntity, entityField.Name), "entityField");
      var entityType = entityField.ValueType;
      var persistentType = entityField.ReflectedType.Model.Types[entityType];
      var mapping = new Segment<int>(entityField.MappingInfo.Offset + offset, entityField.MappingInfo.Length);
      var fields = new List<PersistentFieldExpression>();
      var keyExpression = KeyExpression.Create(persistentType, offset + entityField.MappingInfo.Offset);
      fields.Add(keyExpression);
      foreach (var keyField in persistentType.Fields.Where(f => f.IsPrimaryKey))
        fields.Add(BuildNestedFieldExpression(keyField, offset + entityField.MappingInfo.Offset));
      return new EntityFieldExpression(persistentType, entityField, fields, mapping, keyExpression, null, null, false);
    }

    private static PersistentFieldExpression BuildNestedFieldExpression(FieldInfo nestedField, int offset)
    {
      if (nestedField.IsPrimitive)
        return CreateField(nestedField, offset);
      if (nestedField.IsEntity)
        return CreateEntityField(nestedField, offset);
      throw new NotSupportedException(string.Format(Resources.Strings.ExNestedFieldXIsNotSupported, nestedField.Attributes));
    }


    // Constructors

    private EntityFieldExpression(
      TypeInfo persistentType, 
      FieldInfo field, 
      List<PersistentFieldExpression> fields,
      Segment<int> mapping, 
      KeyExpression key, 
      EntityExpression entity, 
      ParameterExpression parameterExpression, 
      bool defaultIfEmpty)
      : base(ExtendedExpressionType.EntityField, field, mapping, parameterExpression, defaultIfEmpty)
    {
      PersistentType = persistentType;
      Fields = fields;
      Key = key;
      Entity = entity;
    }
  }
}
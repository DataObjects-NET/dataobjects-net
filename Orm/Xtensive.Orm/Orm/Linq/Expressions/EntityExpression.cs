// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2009.05.05

using System;
using System.Diagnostics;
using System.Linq;
using System.Collections.Generic;
using System.Linq.Expressions;
using Xtensive.Core;
using Xtensive.Orm.Model;

namespace Xtensive.Orm.Linq.Expressions
{
  internal class EntityExpression : ParameterizedExpression,
    IEntityExpression
  {
    private List<PersistentFieldExpression> fields;

    public TypeInfo PersistentType { get; }

    public KeyExpression Key { get; }

    public List<PersistentFieldExpression> Fields
    {
      get => fields;
      private set {
        fields = value;
        foreach (var fieldExpression in fields.OfType<FieldExpression>()) {
          fieldExpression.Owner = this;
        }
      }
    }

    public bool IsNullable { get; set; }

    public Expression Remap(int offset, Dictionary<Expression, Expression> processedExpressions)
    {
      if (!CanRemap) {
        return this;
      }

      if (processedExpressions.TryGetValue(this, out var value)) {
        return value;
      }

      var keyExpression = (KeyExpression) Key.Remap(offset, processedExpressions);
      var result = new EntityExpression(PersistentType, keyExpression, OuterParameter, DefaultIfEmpty);
      processedExpressions.Add(this, result);
      result.IsNullable = IsNullable;
      var processedFields = new List<PersistentFieldExpression>(fields.Count);
      foreach (var field in fields) {
        // Do not convert to LINQ. We want to avoid a closure creation here.
        processedFields.Add((PersistentFieldExpression) field.Remap(offset, processedExpressions));
      }

      result.Fields = processedFields;
      return result;
    }

    public Expression Remap(int[] map, Dictionary<Expression, Expression> processedExpressions)
    {
      if (!CanRemap) {
        return this;
      }

      if (processedExpressions.TryGetValue(this, out var value)) {
        return value;
      }

      var keyExpression = (KeyExpression) Key.Remap(map, processedExpressions);
      if (keyExpression==null) {
        return null;
      }

      var result = new EntityExpression(PersistentType, keyExpression, OuterParameter, DefaultIfEmpty);
      processedExpressions.Add(this, result);
      result.IsNullable = IsNullable;
      var processedFields = new List<PersistentFieldExpression>(fields.Count);
      foreach (var field in fields) {
        // Do not convert to LINQ. We want to avoid a closure creation here.
        var mappedField = (PersistentFieldExpression) field.Remap(map, processedExpressions);
        if (mappedField == null) {
          continue;
        }

        processedFields.Add(mappedField);
      }

      result.Fields = processedFields;
      return result;
    }

    public Expression BindParameter(ParameterExpression parameter, Dictionary<Expression, Expression> processedExpressions)
    {
      if (processedExpressions.TryGetValue(this, out var value)) {
        return value;
      }

      var keyExpression = (KeyExpression) Key.BindParameter(parameter, processedExpressions);
      var result = new EntityExpression(PersistentType, keyExpression, parameter, DefaultIfEmpty);
      processedExpressions.Add(this, result);
      var processedFields = new List<PersistentFieldExpression>(fields.Count);
      foreach (var field in fields) {
        // Do not convert to LINQ. We want to avoid a closure creation here.
        processedFields.Add((PersistentFieldExpression) field.BindParameter(parameter, processedExpressions));
      }

      result.Fields = processedFields;
      return result;
    }

    public Expression RemoveOuterParameter(Dictionary<Expression, Expression> processedExpressions)
    {
      if (processedExpressions.TryGetValue(this, out var value)) {
        return value;
      }

      var keyExpression = (KeyExpression) Key.RemoveOuterParameter(processedExpressions);
      var result = new EntityExpression(PersistentType, keyExpression, null, DefaultIfEmpty);
      processedExpressions.Add(this, result);
      var processedFields = new List<PersistentFieldExpression>(fields.Count);
      foreach (var field in fields) {
        // Do not convert to LINQ. We want to avoid a closure creation here.
        processedFields.Add((PersistentFieldExpression) field.RemoveOuterParameter(processedExpressions));
      }

      result.Fields = processedFields;
      return result;
    }

    public static void Fill(EntityExpression entityExpression, int offset)
    {
      using (new RemapScope()) {
        entityExpression.Remap(offset, new Dictionary<Expression, Expression>());
      }
      var typeInfo = entityExpression.PersistentType;
      foreach (var nestedField in typeInfo.Fields.Except(entityExpression.Fields.OfType<FieldExpression>().Select(field=>field.Field))) {
        var nestedFieldExpression = BuildNestedFieldExpression(nestedField, offset);
        var fieldExpression = nestedFieldExpression as FieldExpression;
        if (fieldExpression!=null)
          fieldExpression.Owner = entityExpression;
        entityExpression.fields.Add(nestedFieldExpression);
      }
    }

    public static EntityExpression Create(TypeInfo typeInfo, int offset, bool keyFieldsOnly)
    {
      if (!typeInfo.IsEntity && !typeInfo.IsInterface) {
        throw new ArgumentException(
          string.Format(Strings.ExPersistentTypeXIsNotEntityOrPersistentInterface, typeInfo.Name), nameof(typeInfo));
      }

      var keyExpression = KeyExpression.Create(typeInfo, offset);

      List<PersistentFieldExpression> fields;
      var result = new EntityExpression(typeInfo, keyExpression, null, false);
      if (keyFieldsOnly) {
        fields = new List<PersistentFieldExpression>(keyExpression.KeyFields.Count + 1) {keyExpression};
        // Add key fields to field collection
        foreach (var keyField in keyExpression.KeyFields) {
          // Do not convert to LINQ. We want to avoid a closure creation here.
          fields.Add(FieldExpression.CreateField(keyField.Field, offset));
        }
      }
      else {
        fields = new List<PersistentFieldExpression>(typeInfo.Fields.Count + 1) {keyExpression};
        foreach (var nestedField in typeInfo.Fields) {
          // Do not convert to LINQ. We want to avoid a closure creation here.
          fields.Add(BuildNestedFieldExpression(nestedField, offset));
        }
      }

      result.Fields = fields;
      return result;
    }

    public static EntityExpression Create(EntityFieldExpression entityFieldExpression, int offset)
    {
      var typeInfo = entityFieldExpression.PersistentType;
      var keyExpression = KeyExpression.Create(typeInfo, offset);
      var fields = new List<PersistentFieldExpression>(typeInfo.Fields.Count + 1) {keyExpression};
      foreach (var nestedField in typeInfo.Fields) {
        // Do not convert to LINQ. We want to avoid a closure creation here.
        fields.Add(BuildNestedFieldExpression(nestedField, offset));
      }

      var result = new EntityExpression(typeInfo, keyExpression, null, entityFieldExpression.DefaultIfEmpty) {
        Fields = fields
      };
      if (entityFieldExpression.OuterParameter == null) {
        return result;
      }

      return (EntityExpression) result.BindParameter(
        entityFieldExpression.OuterParameter, new Dictionary<Expression, Expression>());
    }

    private static PersistentFieldExpression BuildNestedFieldExpression(FieldInfo nestedField, int offset)
    {
      if (nestedField.IsPrimitive)
        return FieldExpression.CreateField(nestedField, offset);
      if (nestedField.IsStructure)
        return StructureFieldExpression.CreateStructure(nestedField, offset);
      if (nestedField.IsEntity)
        return EntityFieldExpression.CreateEntityField(nestedField, offset);
      if (nestedField.IsEntitySet)
          return EntitySetExpression.CreateEntitySet(nestedField);
      throw new NotSupportedException(string.Format(Strings.ExNestedFieldXIsNotSupported, nestedField.Attributes));
    }

    public override string ToString() => $"{base.ToString()} {PersistentType.Name}";

    // Constructors

    private EntityExpression(
      TypeInfo entityType, 
      KeyExpression key, 
      ParameterExpression parameterExpression, 
      bool defaultIfEmpty)
      : base(ExtendedExpressionType.Entity, entityType.UnderlyingType, parameterExpression, defaultIfEmpty)
    {
      PersistentType = entityType;
      Key = key;
    }
  }
}
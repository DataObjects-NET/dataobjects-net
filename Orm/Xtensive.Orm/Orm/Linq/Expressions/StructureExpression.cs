// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2009.09.29

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Xtensive.Core;
using Xtensive.Orm.Model;
using System.Linq;

namespace Xtensive.Orm.Linq.Expressions
{
  internal sealed class StructureExpression : ParameterizedExpression,
    IPersistentExpression
  {
    private List<PersistentFieldExpression> fields;
    private bool isNullable;

    internal Segment<int> Mapping;
    public TypeInfo PersistentType { get; }

    public bool IsNullable => isNullable;

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

    public Expression Remap(int offset, Dictionary<Expression, Expression> processedExpressions)
    {
      if (!CanRemap) {
        return this;
      }

      if (processedExpressions.TryGetValue(this, out var value)) {
        return value;
      }

      var mapping = new Segment<int>(Mapping.Offset + offset, Mapping.Length);
      var result = new StructureExpression(PersistentType, mapping);
      processedExpressions.Add(this, result);
      var processedFields = new List<PersistentFieldExpression>(fields.Count);
      foreach (var field in fields) {
        // Do not convert to LINQ. We intentionally avoiding closure creation here
        processedFields.Add((PersistentFieldExpression) field.Remap(offset, processedExpressions));
      }
      result.Fields = processedFields;
      result.isNullable = isNullable;
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

      var result = new StructureExpression(PersistentType, default);
      processedExpressions.Add(this, result);
      var processedFields = new List<PersistentFieldExpression>(fields.Count);
      var offset = int.MaxValue;
      foreach (var field in fields) {
        var mappedField = (PersistentFieldExpression) field.Remap(map, processedExpressions);
        if (mappedField == null) {
          continue;
        }

        var mappingOffset = mappedField.Mapping.Offset;
        if (mappingOffset < offset) {
          offset = mappingOffset;
        }

        processedFields.Add(mappedField);
      }

      if (processedFields.Count == 0) {
        processedExpressions[this] = null;
        return null;
      }

      result.Mapping = new Segment<int>(offset, processedFields.Count);
      result.Fields = processedFields;
      result.isNullable = isNullable;
      return result;
    }

    public Expression BindParameter(ParameterExpression parameter, Dictionary<Expression, Expression> processedExpressions)
    {
      if (processedExpressions.TryGetValue(this, out var value)) {
        return value;
      }

      var result = new StructureExpression(PersistentType, Mapping);
      processedExpressions.Add(this, result);
      var processedFields = new List<PersistentFieldExpression>(fields.Count);
      foreach (var field in fields) {
        // Do not convert to LINQ. We intentionally avoiding closure creation here
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

      var result = new StructureExpression(PersistentType, Mapping);
      processedExpressions.Add(this, result);
      var processedFields = new List<PersistentFieldExpression>(fields.Count);
      foreach (var field in fields) {
        // Do not convert to LINQ. We intentionally avoiding closure creation here
        processedFields.Add((PersistentFieldExpression) field.RemoveOuterParameter(processedExpressions));
      }

      result.Fields = processedFields;
      return result;
    }

    public static StructureExpression CreateLocalCollectionStructure(TypeInfo typeInfo, in Segment<int> mapping)
    {
      if (!typeInfo.IsStructure) {
        throw new ArgumentException(string.Format(Strings.ExTypeXIsNotStructure, typeInfo.Name));
      }

      var sourceFields = typeInfo.Fields;
      var destinationFields = new List<PersistentFieldExpression>(sourceFields.Count);
      var result = new StructureExpression(typeInfo, mapping) {Fields = destinationFields};
      foreach (var field in sourceFields) {
        // Do not convert to LINQ. We intentionally avoiding closure creation here
        destinationFields.Add(BuildNestedFieldExpression(field, mapping.Offset));
      }
      return result;
    }

    private static PersistentFieldExpression BuildNestedFieldExpression(FieldInfo nestedField, int offset)
    {
      if (nestedField.IsPrimitive)
        return FieldExpression.CreateField(nestedField, offset);
      if (nestedField.IsStructure)
        return StructureFieldExpression.CreateStructure(nestedField, offset);
      if (nestedField.IsEntity)
        return EntityFieldExpression.CreateEntityField(nestedField, offset);
      throw new NotSupportedException(string.Format(Strings.ExNestedFieldXIsNotSupported, nestedField.Attributes));
    }

    // Constructors

    private StructureExpression(
      TypeInfo persistentType, 
      in Segment<int> mapping)
      : base(ExtendedExpressionType.Structure, persistentType.UnderlyingType, null, false)
    {
      Mapping = mapping;
      PersistentType = persistentType;
    }
  }
}

// Copyright (C) 2009-2021 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Alexis Kochetov
// Created:    2009.05.05

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Xtensive.Core;
using Xtensive.Orm.Model;
using System.Linq;

namespace Xtensive.Orm.Linq.Expressions
{
  internal sealed class StructureFieldExpression : FieldExpression,
    IPersistentExpression
  {
    private List<PersistentFieldExpression> fields;
    public TypeInfo PersistentType { get; }

    public bool IsNullable => Owner != null && Owner.IsNullable;

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

    public override Expression Remap(int offset, Dictionary<Expression, Expression> processedExpressions)
    {
      if (!CanRemap) {
        return this;
      }

      if (processedExpressions.TryGetValue(this, out var value)) {
        return value;
      }

      var newMapping = new Segment<int>(Mapping.Offset + offset, Mapping.Length);
      var result = new StructureFieldExpression(PersistentType, Field, newMapping, OuterParameter, DefaultIfEmpty);
      processedExpressions.Add(this, result);
      var processedFields = new List<PersistentFieldExpression>(fields.Count);
      foreach (var field in fields) {
        // Do not convert to LINQ. We want to avoid a closure creation here.
        processedFields.Add((PersistentFieldExpression) field.Remap(offset, processedExpressions));
      }

      if (Owner == null) {
        result.fields = processedFields;
        return result;
      }

      result.Fields = processedFields;
      Owner.Remap(offset, processedExpressions);
      return result;
    }

    public override Expression Remap(IReadOnlyList<int> map, Dictionary<Expression, Expression> processedExpressions)
    {
      if (!CanRemap) {
        return this;
      }

      if (processedExpressions.TryGetValue(this, out var value)) {
        return value;
      }

      var result = new StructureFieldExpression(PersistentType, Field, default, OuterParameter, DefaultIfEmpty);
      processedExpressions.Add(this, result);
      var offset = int.MaxValue;
      var processedFields = new List<PersistentFieldExpression>(fields.Count);
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
      if (Owner == null) {
        result.fields = processedFields;
        return result;
      }

      result.Fields = processedFields;
      Owner.Remap(map, processedExpressions);
      return result;
    }

    public override Expression BindParameter(ParameterExpression parameter, Dictionary<Expression, Expression> processedExpressions)
    {
      if (processedExpressions.TryGetValue(this, out var value)) {
        return value;
      }

      var result = new StructureFieldExpression(PersistentType, Field, Mapping, OuterParameter, DefaultIfEmpty);
      processedExpressions.Add(this, result);
      var processedFields = new List<PersistentFieldExpression>(fields.Count);
      foreach (var field in fields) {
        // Do not convert to LINQ. We want to avoid a closure creation here.
        processedFields.Add((PersistentFieldExpression) field.BindParameter(parameter, processedExpressions));
      }

      if (Owner == null) {
        result.fields = processedFields;
        return result;
      }

      result.Fields = processedFields;
      Owner.BindParameter(parameter, processedExpressions);
      return result;
    }

    public override Expression RemoveOuterParameter(Dictionary<Expression, Expression> processedExpressions)
    {
      if (processedExpressions.TryGetValue(this, out var value)) {
        return value;
      }

      var result = new StructureFieldExpression(PersistentType, Field, Mapping, OuterParameter, DefaultIfEmpty);
      processedExpressions.Add(this, result);
      var processedFields = new List<PersistentFieldExpression>(fields.Count);
      foreach (var field in fields) {
        // Do not convert to LINQ. We want to avoid a closure creation here.
        processedFields.Add((PersistentFieldExpression) field.RemoveOuterParameter(processedExpressions));
      }

      if (Owner == null) {
        result.fields = processedFields;
        return result;
      }

      result.Fields = processedFields;
      Owner.RemoveOuterParameter(processedExpressions);
      return result;
    }

    public override FieldExpression RemoveOwner()
    {
      if (Owner == null) {
        return this;
      }

      var result = new StructureFieldExpression(PersistentType, Field, Mapping, OuterParameter, DefaultIfEmpty) {
        fields = new List<PersistentFieldExpression>(fields.Count)
      };
      foreach (var field in fields) {
        result.fields.Add(((FieldExpression) field).RemoveOwner());
      }

      return result;
    }

    public static StructureFieldExpression CreateStructure(FieldInfo structureField, int offset)
    {
      if (!structureField.IsStructure) {
        throw new ArgumentException(string.Format(Strings.ExFieldIsNotStructure, structureField.Name));
      }

      var persistentType = structureField.ReflectedType.Model.Types[structureField.ValueType];
      var fieldMappingInfo = structureField.MappingInfo;
      var mapping = new Segment<int>(offset + fieldMappingInfo.Offset, fieldMappingInfo.Length);
      var result = new StructureFieldExpression(persistentType, structureField, mapping, null, false);
      var processedFields = new List<PersistentFieldExpression>(persistentType.Fields.Count);
      foreach (var field in persistentType.Fields) {
        // Do not convert to LINQ. We want to avoid a closure creation here.
        processedFields.Add(BuildNestedFieldExpression(field, offset + fieldMappingInfo.Offset));
      }

      result.Fields = processedFields;
      return result;
    }

// ReSharper disable RedundantNameQualifier
    private static PersistentFieldExpression BuildNestedFieldExpression(FieldInfo nestedField, int offset)
    {
      if (nestedField.IsPrimitive) {
        return FieldExpression.CreateField(nestedField, offset);
      }

      if (nestedField.IsStructure) {
        return StructureFieldExpression.CreateStructure(nestedField, offset);
      }

      if (nestedField.IsEntity) {
        return EntityFieldExpression.CreateEntityField(nestedField, offset);
      }

      throw new NotSupportedException(string.Format(Strings.ExNestedFieldXIsNotSupported, nestedField.Attributes));
    }
// ReSharper restore RedundantNameQualifier


    // Constructors

    private StructureFieldExpression(
      TypeInfo persistentType, 
      FieldInfo structureField, 
      in Segment<int> mapping,
      ParameterExpression parameterExpression, 
      bool defaultIfEmpty)
      : base(ExtendedExpressionType.StructureField, structureField, mapping, parameterExpression, defaultIfEmpty)
    {
      PersistentType = persistentType;
    }
  }
}
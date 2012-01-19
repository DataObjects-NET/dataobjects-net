// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2009.05.05

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Xtensive.Collections;
using Xtensive.Core;
using Xtensive.Orm.Model;
using System.Linq;
using Xtensive.Orm.Resources;

namespace Xtensive.Orm.Linq.Expressions
{
  internal sealed class StructureFieldExpression : FieldExpression,
    IPersistentExpression
  {
    private List<PersistentFieldExpression> fields;
    public TypeInfo PersistentType { get; private set; }

    public bool IsNullable
    {
      get { return Owner != null && Owner.IsNullable; }
    }

    public List<PersistentFieldExpression> Fields
    {
      get { return fields; }
      private set
      {
        fields = value;
        // Set owner only for non dynamic added properties
        foreach (var fieldExpression in fields.OfType<FieldExpression>().Where(f => f.UnderlyingProperty != null))
          fieldExpression.Owner = this;
      }
    }

    public override Expression Remap(int offset, Dictionary<Expression, Expression> processedExpressions)
    {
      if (!CanRemap)
        return this;

      Expression value;
      if (processedExpressions.TryGetValue(this, out value))
        return value;

      var mapping = new Segment<int>(Mapping.Offset + offset, Mapping.Length);
      var result = new StructureFieldExpression(PersistentType, Field, mapping, OuterParameter, DefaultIfEmpty);
      processedExpressions.Add(this, result);
      var processedFields = Fields
        .Select(f => f.Remap(offset, processedExpressions))
        .Cast<PersistentFieldExpression>()
        .ToList();
      if (Owner == null) {
        result.fields = processedFields;
        return result;
      }

      result.Fields = processedFields;
      Owner.Remap(offset, processedExpressions);
      return result;
    }

    public override Expression Remap(int[] map, Dictionary<Expression, Expression> processedExpressions)
    {
      if (!CanRemap)
        return this;

      Expression value;
      if (processedExpressions.TryGetValue(this, out value))
        return value;

      var result = new StructureFieldExpression(PersistentType, Field, default(Segment<int>), OuterParameter, DefaultIfEmpty);
      processedExpressions.Add(this, result);
      var processedFields = Fields
        .Select(f => f.Remap(map, processedExpressions))
        .Where(f => f != null)
        .Cast<PersistentFieldExpression>()
        .ToList();
      if (processedFields.Count == 0) {
        processedExpressions[this] = null;
        return null;
      }
      var length = processedFields.Select(f => f.Mapping.Offset).Distinct().Count();
      var offset = processedFields.Min(f => f.Mapping.Offset);
      result.Mapping = new Segment<int>(offset, length);
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
      Expression value;
      if (processedExpressions.TryGetValue(this, out value))
        return value;

      var result = new StructureFieldExpression(PersistentType, Field, Mapping, OuterParameter, DefaultIfEmpty);
      processedExpressions.Add(this, result);
      var processedFields = Fields
        .Select(f => f.BindParameter(parameter, processedExpressions))
        .Cast<PersistentFieldExpression>()
        .ToList();
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
      Expression value;
      if (processedExpressions.TryGetValue(this, out value))
        return value;

      var result = new StructureFieldExpression(PersistentType, Field, Mapping, OuterParameter, DefaultIfEmpty);
      processedExpressions.Add(this, result);
      var processedFields = Fields
        .Select(f => f.RemoveOuterParameter(processedExpressions))
        .Cast<PersistentFieldExpression>()
        .ToList();
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
      if (Owner==null)
        return this;
      var result = new StructureFieldExpression(PersistentType, Field, Mapping, OuterParameter, DefaultIfEmpty);
      result.fields = fields
        .Cast<FieldExpression>()
        .Select(f => (PersistentFieldExpression)f.RemoveOwner())
        .ToList();
      return result;
    }

    public static StructureFieldExpression CreateStructure(FieldInfo structureField, int offset)
    {
      if (!structureField.IsStructure)
        throw new ArgumentException(string.Format(Strings.ExFieldIsNotStructure, structureField.Name));
      var persistentType = structureField.ReflectedType.Model.Types[structureField.ValueType];
      var mapping = new Segment<int>(offset, structureField.MappingInfo.Length);
      var result = new StructureFieldExpression(persistentType, structureField, mapping, null, false);
      result.Fields = persistentType.Fields
        .Select(f => BuildNestedFieldExpression(f, offset + structureField.MappingInfo.Offset))
        .ToList();
      return result;
    }

// ReSharper disable RedundantNameQualifier
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
// ReSharper restore RedundantNameQualifier


    // Constructors

    private StructureFieldExpression(
      TypeInfo persistentType, 
      FieldInfo structureField, 
      Segment<int> mapping, 
      ParameterExpression parameterExpression, 
      bool defaultIfEmpty)
      : base(ExtendedExpressionType.StructureField, structureField, mapping, parameterExpression, defaultIfEmpty)
    {
      PersistentType = persistentType;
    }
  }
}
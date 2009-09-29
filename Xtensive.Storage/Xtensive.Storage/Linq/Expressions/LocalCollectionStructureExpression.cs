// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2009.09.29

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Xtensive.Core;
using Xtensive.Core.Collections;
using Xtensive.Storage.Model;
using System.Linq;
using Xtensive.Storage.Resources;

namespace Xtensive.Storage.Linq.Expressions
{
  internal sealed class LocalCollectionStructureExpression : PersistentFieldExpression,
    IPersistentExpression
  {
    private List<PersistentFieldExpression> fields;
    public TypeInfo PersistentType { get; private set; }

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
      var result = new LocalCollectionStructureExpression(PersistentType, mapping);
      processedExpressions.Add(this, result);
      var processedFields = Fields
        .Select(f => f.Remap(offset, processedExpressions))
        .Cast<PersistentFieldExpression>()
        .ToList();
      result.Fields = processedFields;
      return result;
    }

    public override Expression Remap(int[] map, Dictionary<Expression, Expression> processedExpressions)
    {
      if (!CanRemap)
        return this;

      Expression value;
      if (processedExpressions.TryGetValue(this, out value))
        return value;

      var result = new LocalCollectionStructureExpression(PersistentType, default(Segment<int>));
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
        result.Fields = processedFields;
      return result;
    }

    public override Expression BindParameter(ParameterExpression parameter, Dictionary<Expression, Expression> processedExpressions)
    {
      Expression value;
      if (processedExpressions.TryGetValue(this, out value))
        return value;

      var result = new LocalCollectionStructureExpression(PersistentType, Mapping);
      processedExpressions.Add(this, result);
      var processedFields = Fields
        .Select(f => f.BindParameter(parameter, processedExpressions))
        .Cast<PersistentFieldExpression>()
        .ToList();
      result.Fields = processedFields;
      return result;
    }

    public override Expression RemoveOuterParameter(Dictionary<Expression, Expression> processedExpressions)
    {
      Expression value;
      if (processedExpressions.TryGetValue(this, out value))
        return value;

      var result = new LocalCollectionStructureExpression(PersistentType, Mapping);
      processedExpressions.Add(this, result);
      var processedFields = Fields
        .Select(f => f.RemoveOuterParameter(processedExpressions))
        .Cast<PersistentFieldExpression>()
        .ToList();
      result.Fields = processedFields;
      return result;
    }

    public static LocalCollectionStructureExpression CreateLocalCollectionStructure(TypeInfo typeInfo, Segment<int> mapping)
    {
      if (!typeInfo.IsStructure)
        throw new ArgumentException(string.Format(Strings.ExTypeXIsNotStructure, typeInfo.Name));
      var result = new LocalCollectionStructureExpression(typeInfo, mapping);
      result.Fields = typeInfo.Fields
        .Select(f => BuildNestedFieldExpression(f, mapping.Offset))
        .ToList();
      return result;
    }

// ReSharper disable RedundantNameQualifier
    private static PersistentFieldExpression BuildNestedFieldExpression(FieldInfo nestedField, int offset)
    {
      if (nestedField.IsPrimitive)
        return FieldExpression.CreateField(nestedField, offset);
      if (nestedField.IsStructure)
        return StructureExpression.CreateStructure(nestedField, offset);
      if (nestedField.IsEntity)
        return EntityFieldExpression.CreateEntityField(nestedField, offset);
      throw new NotSupportedException(string.Format(Strings.ExNestedFieldXIsNotSupported, nestedField.Attributes));
    }
// ReSharper restore RedundantNameQualifier


    // Constructors

    private LocalCollectionStructureExpression(
      TypeInfo persistentType, 
      Segment<int> mapping)
      : base(ExtendedExpressionType.LocalCollectionStructure, null, persistentType.UnderlyingType, mapping, null, null, false)
    {
      PersistentType = persistentType;
    }
  }
}

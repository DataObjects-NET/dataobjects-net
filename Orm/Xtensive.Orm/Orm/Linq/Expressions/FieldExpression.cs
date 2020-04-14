// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2009.05.05

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using Xtensive.Core;
using FieldInfo=Xtensive.Orm.Model.FieldInfo;
using Xtensive.Collections;

namespace Xtensive.Orm.Linq.Expressions
{
  internal class FieldExpression : PersistentFieldExpression
  {
    private IPersistentExpression owner;

    public new FieldInfo Field { get; }

    public virtual IPersistentExpression Owner
    {
      get => owner;
      internal set
      {
        if (owner!=null) {
          throw Exceptions.AlreadyInitialized("Owner");
        }

        owner = value;
      }
    }

    public override Expression Remap(int offset, Dictionary<Expression, Expression> processedExpressions)
    {
      if (!CanRemap) {
        return this;
      }

      if (processedExpressions.TryGetValue(this, out var result)) {
        return result;
      }

      var mapping = new Segment<int>(Mapping.Offset + offset, Mapping.Length);
      result = new FieldExpression(ExtendedExpressionType.Field, Field, mapping, OuterParameter, DefaultIfEmpty);
      if (owner == null) {
        return result;
      }

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

      var offset = map.IndexOf(Mapping.Offset);
      if (offset < 0) {
        if (owner == null && !SkipOwnerCheckScope.IsActive)
          throw new InvalidOperationException(Strings.ExUnableToRemapFieldExpression);
        processedExpressions.Add(this, null);
        if (owner != null)
          Owner.Remap(map, processedExpressions);
        return null;
      }
      var mapping = new Segment<int>(offset, Mapping.Length);
      result = new FieldExpression(ExtendedExpressionType.Field, Field, mapping, OuterParameter, DefaultIfEmpty);
      if (owner == null)
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

      result = new FieldExpression(ExtendedExpressionType.Field, Field, Mapping, parameter, DefaultIfEmpty);
      if (owner == null)
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

      result = new FieldExpression(ExtendedExpressionType.Field, Field, Mapping, null, DefaultIfEmpty);
      if (owner == null)
        return result;

      processedExpressions.Add(this, result);
      Owner.RemoveOuterParameter(processedExpressions);
      return result;
    }

    public virtual FieldExpression RemoveOwner()
    {
      return new FieldExpression(ExtendedExpressionType.Field, Field, Mapping, OuterParameter, DefaultIfEmpty);
    }

    public static FieldExpression CreateField(FieldInfo field, int offset)
    {
      if (!field.IsPrimitive)
        throw new ArgumentException(string.Format(Strings.ExFieldXIsNotPrimitive, field.Name), "field");
      var mapping = new Segment<int>(field.MappingInfo.Offset + offset, field.MappingInfo.Length);
      return new FieldExpression(ExtendedExpressionType.Field, field, mapping, null, false);
    }

    // Constructors

    protected FieldExpression(
      ExtendedExpressionType expressionType, 
      FieldInfo field, 
      Segment<int> mapping, 
      ParameterExpression parameterExpression, 
      bool defaultIfEmpty)
      : base(expressionType, field.Name, field.ValueType, mapping, field.UnderlyingProperty, parameterExpression, defaultIfEmpty)
    {
      Field = field;
    }
  }
}

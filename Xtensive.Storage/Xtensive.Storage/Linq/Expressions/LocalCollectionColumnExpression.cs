// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2009.09.15

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using Xtensive.Core;
using Xtensive.Core.Collections;

namespace Xtensive.Storage.Linq.Expressions
{
  [Serializable]
  internal class LocalCollectionColumnExpression : ParameterizedExpression,
    IMappedExpression
  {
    public Segment<int> Mapping { get; private set; }

    public PropertyInfo PropertyInfo{ get; private set;}

    public Expression Remap(int offset, Dictionary<Expression, Expression> processedExpressions)
    {
      if (!CanRemap)
        return this;
      var mapping = new Segment<int>(Mapping.Offset + offset, 1);
      return new LocalCollectionColumnExpression(Type, mapping, OuterParameter, PropertyInfo,DefaultIfEmpty);
    }

    public Expression Remap(int[] map, Dictionary<Expression, Expression> processedExpressions)
    {
      if (!CanRemap)
        return this;
      var mapping = new Segment<int>(map.IndexOf(Mapping.Offset), 1);
      return new LocalCollectionColumnExpression(Type, mapping, OuterParameter, PropertyInfo,DefaultIfEmpty);
    }

    public Expression BindParameter(ParameterExpression parameter, Dictionary<Expression, Expression> processedExpressions)
    {
      return new LocalCollectionColumnExpression(Type, Mapping, parameter, PropertyInfo,DefaultIfEmpty);
    }

    public Expression RemoveOuterParameter(Dictionary<Expression, Expression> processedExpressions)
    {
      return new LocalCollectionColumnExpression(Type, Mapping, null, PropertyInfo, DefaultIfEmpty);
    }

    public override string ToString()
    {
      return string.Format("{0}, Offset: {1}", base.ToString(), Mapping.Offset);
    }


    // Constructors

    public LocalCollectionColumnExpression(
      Type type, 
      Segment<int> mapping, 
      ParameterExpression parameterExpression, 
      PropertyInfo propertyInfo,
      bool defaultIfEmpty)
      : base(ExtendedExpressionType.Column, type, parameterExpression, defaultIfEmpty)
    {
      Mapping = mapping;
      PropertyInfo = propertyInfo;
    }
  }
}

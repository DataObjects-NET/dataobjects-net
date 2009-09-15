// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2009.09.15

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Xtensive.Core;
using Xtensive.Core.Collections;

namespace Xtensive.Storage.Linq.Expressions
{
  [Serializable]
  internal class LocalCollectionColumn : ParameterizedExpression,
    IMappedExpression
  {
    public Segment<int> Mapping { get; private set; }

    public Expression MaterializationExpression{ get; private set;}

    public Expression Remap(int offset, Dictionary<Expression, Expression> processedExpressions)
    {
      if (!CanRemap)
        return this;
      var mapping = new Segment<int>(Mapping.Offset + offset, 1);
      return new LocalCollectionColumn(Type, mapping, OuterParameter, MaterializationExpression,DefaultIfEmpty);
    }

    public Expression Remap(int[] map, Dictionary<Expression, Expression> processedExpressions)
    {
      if (!CanRemap)
        return this;
      var mapping = new Segment<int>(map.IndexOf(Mapping.Offset), 1);
      return new LocalCollectionColumn(Type, mapping, OuterParameter, MaterializationExpression,DefaultIfEmpty);
    }

    public Expression BindParameter(ParameterExpression parameter, Dictionary<Expression, Expression> processedExpressions)
    {
      return new LocalCollectionColumn(Type, Mapping, parameter, MaterializationExpression,DefaultIfEmpty);
    }

    public Expression RemoveOuterParameter(Dictionary<Expression, Expression> processedExpressions)
    {
      return new LocalCollectionColumn(Type, Mapping, null, MaterializationExpression, DefaultIfEmpty);
    }

    public override string ToString()
    {
      return string.Format("{0}, Offset: {1}", base.ToString(), Mapping.Offset);
    }


    // Constructors

    protected LocalCollectionColumn(
      Type type, 
      Segment<int> mapping, 
      ParameterExpression parameterExpression, 
      Expression materializationExpression,
      bool defaultIfEmpty)
      : base(ExtendedExpressionType.Column, type, parameterExpression, defaultIfEmpty)
    {
      Mapping = mapping;
      MaterializationExpression = materializationExpression;
    }
  }
}

// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2009.05.06

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq.Expressions;
using Xtensive.Core;
using Xtensive.Core.Collections;

namespace Xtensive.Storage.Linq.Expressions
{
  internal class ColumnExpression : ParameterizedExpression,
    IMappedExpression
  {
    public Segment<int> Mapping { get; private set; }

    public Expression Remap(int offset, Dictionary<Expression, Expression> processedExpressions)
    {
      if (!CanRemap)
        return this;
      var mapping = new Segment<int>(Mapping.Offset + offset, 1);
      return new ColumnExpression(Type, mapping, OuterParameter, DefaultIfEmpty, LoadMode);
    }

    public Expression Remap(int[] map, Dictionary<Expression, Expression> processedExpressions)
    {
      if (!CanRemap)
        return this;
      var mapping = new Segment<int>(map.IndexOf(Mapping.Offset), 1);
      return new ColumnExpression(Type, mapping, OuterParameter, DefaultIfEmpty, LoadMode);
    }

    public Expression BindParameter(ParameterExpression parameter, Dictionary<Expression, Expression> processedExpressions)
    {
      return new ColumnExpression(Type, Mapping, parameter, DefaultIfEmpty, LoadMode);
    }

    public Expression RemoveOuterParameter(Dictionary<Expression, Expression> processedExpressions)
    {
      return new ColumnExpression(Type, Mapping, null, DefaultIfEmpty, LoadMode);
    }

    public static ColumnExpression Create(Type type, int columnIndex)
    {
      var mapping = new Segment<int>(columnIndex, 1);
      return new ColumnExpression(type, mapping, null, false, FieldLoadMode.Standard);
    }

    public override string ToString()
    {
      return string.Format("{0}, Offset: {1}", base.ToString(), Mapping.Offset);
    }


    // Constructors

    private ColumnExpression(
      Type type, 
      Segment<int> mapping, 
      ParameterExpression parameterExpression, 
      bool defaultIfEmpty,
      FieldLoadMode loadMode)
      : base(ExtendedExpressionType.Column, type, parameterExpression, defaultIfEmpty, loadMode)
    {
      Mapping = mapping;
    }
  }
}
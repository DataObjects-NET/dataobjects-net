// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2009.05.06

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Xtensive.Core;

namespace Xtensive.Orm.Linq.Expressions
{
  internal class ColumnExpression : ParameterizedExpression,
    IMappedExpression
  {
    internal readonly Segment<int> Mapping;

    public Expression Remap(int offset, Dictionary<Expression, Expression> processedExpressions)
    {
      if (!CanRemap)
        return this;
      var newMapping = new Segment<int>(Mapping.Offset + offset, 1);
      return new ColumnExpression(Type, newMapping, OuterParameter, DefaultIfEmpty);
    }

    public Expression Remap(int[] map, Dictionary<Expression, Expression> processedExpressions)
    {
      if (!CanRemap)
        return this;
      var newMapping = new Segment<int>(map.IndexOf(Mapping.Offset), 1);
      return new ColumnExpression(Type, newMapping, OuterParameter, DefaultIfEmpty);
    }

    public Expression BindParameter(ParameterExpression parameter)
    {
      return BindParameter(parameter, new Dictionary<Expression, Expression>());
    }

    public Expression BindParameter(ParameterExpression parameter, Dictionary<Expression, Expression> processedExpressions)
    {
      return new ColumnExpression(Type, Mapping, parameter, DefaultIfEmpty);
    }

    public Expression RemoveOuterParameter(Dictionary<Expression, Expression> processedExpressions)
    {
      return new ColumnExpression(Type, Mapping, null, DefaultIfEmpty);
    }

    public static ColumnExpression Create(Type type, int columnIndex)
    {
      var mapping = new Segment<int>(columnIndex, 1);
      return new ColumnExpression(type, mapping, null, false);
    }

    public override string ToString()
    {
      return string.Format("{0}, Offset: {1}", base.ToString(), Mapping.Offset);
    }


    // Constructors

    protected ColumnExpression(
      Type type,
      in Segment<int> mapping,
      ParameterExpression parameterExpression,
      bool defaultIfEmpty)
      : base(ExtendedExpressionType.Column, type, parameterExpression, defaultIfEmpty)
    {
      this.Mapping = mapping;
    }
  }
}
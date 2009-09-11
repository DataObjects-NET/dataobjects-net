// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2009.09.09

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Xtensive.Core;
using System.Reflection;

namespace Xtensive.Storage.Linq.Expressions
{
  [Serializable]
  internal class LocalCollectionItemExpression : ParameterizedExpression, 
    IMappedExpression
  {
    public Segment<int> Mapping{ get; private set;}
    public Dictionary<PropertyInfo, IMappedExpression> Fields{ get; private set;}


    public Expression BindParameter(ParameterExpression parameter, Dictionary<Expression, Expression> processedExpressions)
    {
      throw new NotImplementedException();
    }

    public Expression RemoveOuterParameter(Dictionary<Expression, Expression> processedExpressions)
    {
      throw new NotImplementedException();
    }

    public Expression Remap(int offset, Dictionary<Expression, Expression> processedExpressions)
    {
      throw new NotImplementedException();
    }

    public Expression Remap(int[] map, Dictionary<Expression, Expression> processedExpressions)
    {
      throw new NotImplementedException();
    }

    public LocalCollectionItemExpression(
      Type type,
      Segment<int> mapping,
      ParameterExpression parameterExpression, 
      bool defaultIfEmpty)
      : base(ExtendedExpressionType.LocalCollectionItem, type, parameterExpression, defaultIfEmpty)
    {
      Mapping = mapping;
      Fields = new Dictionary<PropertyInfo, IMappedExpression>();
    }
  }
}
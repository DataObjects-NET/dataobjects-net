// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2009.12.28

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Xtensive.Core;
using Xtensive.Storage.Model;

namespace Xtensive.Storage.Linq.Expressions
{
  [Serializable]
  internal class FreeTextExpression : ParameterizedExpression,
    IMappedExpression
  {
    public FullTextIndexInfo FullTextIndex { get; private set; }

    public Segment<int> Mapping
    {
      get { throw new NotImplementedException(); }
    }

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

    public FreeTextExpression(FullTextIndexInfo fullTextIndex)
      : base(ExtendedExpressionType.FreeText, fullTextIndex.PrimaryIndex.ReflectedType.UnderlyingType, null, false)
    {
      FullTextIndex = fullTextIndex;
    }
  }
}
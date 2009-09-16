// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2009.09.09

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Xtensive.Core;
using Xtensive.Core.Collections;

namespace Xtensive.Storage.Linq.Expressions
{
  [Serializable]
  internal class LocalCollectionExpression : ParameterizedExpression, 
    IMappedExpression
  {
    public Segment<int> Mapping{ get{ return default(Segment<int>);}}
    public IEnumerable<IMappedExpression> Fields{ get; set;}
    public Expression MaterializationExpression{ get; private set;}

    public Func<object, int, object> Converter
    {
      get { throw new NotImplementedException(); }
    }


    public Expression Remap(int offset, Dictionary<Expression, Expression> processedExpressions)
    {
      if (!CanRemap)
        return this;
      Expression value;
      if (processedExpressions.TryGetValue(this, out value))
        return value;

      var result = new LocalCollectionExpression(Type, OuterParameter, MaterializationExpression, DefaultIfEmpty);
      processedExpressions.Add(this, result);
      result.Fields = Fields
        .Select(f => f.Remap(offset, processedExpressions))
        .Cast<IMappedExpression>();
      return result;
    }

    public Expression Remap(int[] map, Dictionary<Expression, Expression> processedExpressions)
    {
      if (!CanRemap)
        return this;
      Expression value;
      if (processedExpressions.TryGetValue(this, out value))
        return value;

      var result = new LocalCollectionExpression(Type, OuterParameter, MaterializationExpression, DefaultIfEmpty);
      processedExpressions.Add(this, result);
      result.Fields = Fields
        .Select(f => Remap(map, processedExpressions))
        .Cast<IMappedExpression>();
      return result;
    }

    public Expression BindParameter(ParameterExpression parameter, Dictionary<Expression, Expression> processedExpressions)
    {
      Expression value;
      if (processedExpressions.TryGetValue(this, out value))
        return value;

      var result = new LocalCollectionExpression(Type, OuterParameter, MaterializationExpression, DefaultIfEmpty);
      processedExpressions.Add(this, result);
      result.Fields = Fields
        .Select(f => f.BindParameter(parameter, processedExpressions))
        .Cast<IMappedExpression>();
      return result;
    }

    public Expression RemoveOuterParameter(Dictionary<Expression, Expression> processedExpressions)
    {
      Expression value;
      if (processedExpressions.TryGetValue(this, out value))
        return value;

      var result = new LocalCollectionExpression(Type, OuterParameter, MaterializationExpression, DefaultIfEmpty);
      processedExpressions.Add(this, result);
      result.Fields = Fields
        .Select(f => f.RemoveOuterParameter(processedExpressions))
        .Cast<IMappedExpression>();
      return result;
    }

    public LocalCollectionExpression(
      Type type,
      ParameterExpression parameterExpression, 
      Expression materializationExpression,
      bool defaultIfEmpty)
      : base(ExtendedExpressionType.LocalCollectionItem, type, parameterExpression, defaultIfEmpty)
    {
      MaterializationExpression = materializationExpression;
      Fields = EnumerableUtils<IMappedExpression>.Empty;
    }
  }
}
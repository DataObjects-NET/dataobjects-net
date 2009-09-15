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
using System.Reflection;

namespace Xtensive.Storage.Linq.Expressions
{
  [Serializable]
  internal class LocalCollectionExpression : ParameterizedExpression, 
    IMappedExpression
  {
    public Segment<int> Mapping{ get{ return default(Segment<int>);}}
    public Dictionary<PropertyInfo, IMappedExpression> Fields{ get; private set;}
    public Expression MaterializationExpression{ get; private set;}


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
        .Select(f => new{f.Key, Value = f.Value.Remap(offset, processedExpressions)})
        .ToDictionary(a=>a.Key, a=>(IMappedExpression)a.Value);
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
        .Select(f => new{f.Key, Value = f.Value.Remap(map, processedExpressions)})
        .ToDictionary(a=>a.Key, a=>(IMappedExpression)a.Value);
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
        .Select(f => new{f.Key, Value = f.Value.BindParameter(parameter, processedExpressions)})
        .ToDictionary(a=>a.Key, a=>(IMappedExpression)a.Value);
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
        .Select(f => new{f.Key, Value = f.Value.RemoveOuterParameter(processedExpressions)})
        .ToDictionary(a=>a.Key, a=>(IMappedExpression)a.Value);
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
      Fields = new Dictionary<PropertyInfo, IMappedExpression>();
    }
  }
}
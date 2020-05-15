// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2009.09.09

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Xtensive.Collections;
using Xtensive.Core;

namespace Xtensive.Orm.Linq.Expressions
{
  [Serializable]
  internal class LocalCollectionExpression : ParameterizedExpression,
    IMappedExpression
  {
    public Expression SourceExpression { get;private set; }
    public IDictionary<MemberInfo, IMappedExpression> Fields { get; set; }
    public Expression MaterializationExpression { get; private set; }
    public MemberInfo MemberInfo { get; private set; }

    public IEnumerable<ColumnExpression> Columns
    {
      get
      {
        return Fields
          .SelectMany(field => field.Value is ColumnExpression
            ? EnumerableUtils.One(field.Value)
            : ((LocalCollectionExpression) field.Value).Columns.Cast<IMappedExpression>())
          .Cast<ColumnExpression>();
      }
    }

    public Expression Remap(int offset, Dictionary<Expression, Expression> processedExpressions)
    {
      if (!CanRemap)
        return this;
      Expression value;
      if (processedExpressions.TryGetValue(this, out value))
        return value;

      var result = new LocalCollectionExpression(Type, MemberInfo, SourceExpression);
      processedExpressions.Add(this, result);
      result.Fields = Fields.ToDictionary(f=>f.Key, f=>(IMappedExpression)f.Value.Remap(offset, processedExpressions));
      return result;
    }

    public Expression Remap(int[] map, Dictionary<Expression, Expression> processedExpressions)
    {
      if (!CanRemap)
        return this;
      Expression value;
      if (processedExpressions.TryGetValue(this, out value))
        return value;

      var result = new LocalCollectionExpression(Type, MemberInfo, SourceExpression);
      processedExpressions.Add(this, result);
      result.Fields = Fields.ToDictionary(f=>f.Key, f=>(IMappedExpression)f.Value.Remap(map, processedExpressions));
      return result;
    }

    public Expression BindParameter(ParameterExpression parameter, Dictionary<Expression, Expression> processedExpressions)
    {
      Expression value;
      if (processedExpressions.TryGetValue(this, out value))
        return value;

      var result = new LocalCollectionExpression(Type, MemberInfo, SourceExpression);
      processedExpressions.Add(this, result);
      result.Fields = Fields.ToDictionary(f=>f.Key, f=>(IMappedExpression)f.Value.BindParameter(parameter, processedExpressions));
      return result;
    }

    public Expression RemoveOuterParameter(Dictionary<Expression, Expression> processedExpressions)
    {
      Expression value;
      if (processedExpressions.TryGetValue(this, out value))
        return value;

      var result = new LocalCollectionExpression(Type, MemberInfo, SourceExpression);
      processedExpressions.Add(this, result);
      result.Fields = Fields.ToDictionary(f=>f.Key, f=>(IMappedExpression)f.Value.RemoveOuterParameter(processedExpressions));
      return result;
    }

    public LocalCollectionExpression(Type type, MemberInfo memberInfo, Expression sourceExpression)
      : base(ExtendedExpressionType.LocalCollection, type, null, true)
    {
      Fields = new Dictionary<MemberInfo, IMappedExpression>();
      MemberInfo = memberInfo;
      SourceExpression = sourceExpression;
    }

  }
}
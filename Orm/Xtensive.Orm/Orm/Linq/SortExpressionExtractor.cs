// Copyright (C) 2013 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2013.04.29

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Xtensive.Collections;
using Xtensive.Core;
using Xtensive.Linq;

namespace Xtensive.Orm.Linq
{
  internal sealed class SortExpressionExtractor
  {
    public DirectionCollection<LambdaExpression> SortExpressions { get; private set; }

    public Expression BaseExpression { get; private set; }

    public bool Extract(Expression expression)
    {
      var result = new Stack<Pair<LambdaExpression, Direction>>();

      while (true) {
        if (expression.NodeType==ExpressionType.Call) {
          var call = (MethodCallExpression) expression;
          var method = QueryableVisitor.GetQueryableMethod(call);
          if (method!=null && IsSort(method.Value)) {
            var source = call.Arguments[0];
            var projection = call.Arguments[1].StripQuotes();
            var direction = GetDirection(method.Value);
            result.Push(new Pair<LambdaExpression, Direction>(projection, direction));
            expression = source;
            if (IsThenBy(method.Value))
              continue;
          }
        }
        break;
      }

      if (result.Count==0) {
        BaseExpression = null;
        SortExpressions = null;
        return false;
      }

      BaseExpression = expression;
      SortExpressions = new DirectionCollection<LambdaExpression>();
      foreach (var item in result)
        SortExpressions.Add(item.First, item.Second);
      return true;
    }

    private static bool IsSort(QueryableMethodKind method)
    {
      return method==QueryableMethodKind.OrderBy
        || method==QueryableMethodKind.OrderByDescending
        || method==QueryableMethodKind.ThenBy
        || method==QueryableMethodKind.ThenByDescending;
    }

    private static bool IsThenBy(QueryableMethodKind method)
    {
      return method==QueryableMethodKind.ThenBy || method==QueryableMethodKind.ThenByDescending;
    }

    private static Direction GetDirection(QueryableMethodKind method)
    {
      switch (method) {
      case QueryableMethodKind.OrderBy:
      case QueryableMethodKind.ThenBy:
        return Direction.Positive;
      case QueryableMethodKind.OrderByDescending:
      case QueryableMethodKind.ThenByDescending:
        return Direction.Negative;
      default:
        throw new ArgumentOutOfRangeException("method");
      }
    }
  }
}
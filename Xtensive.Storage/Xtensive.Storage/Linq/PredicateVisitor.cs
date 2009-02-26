// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Elena Vakhtina
// Created:    2009.02.18

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Xtensive.Core.Linq;
using Xtensive.Core.Tuples;

namespace Xtensive.Storage.Linq
{
  [Serializable]
  internal class PredicateVisitor : ExpressionVisitor
  {
    private List<int> map = new List<int>();
    private bool isReplacing;
    private List<int> group;

    
    protected override Expression VisitMethodCall(MethodCallExpression mc)
    {
      var result = mc;
      if (mc.Method.Name == "GetValueOrDefault")
        if (!isReplacing)
          map.Add((int)((ConstantExpression)mc.Arguments[0]).Value);
        else {
          var value = (int) ((ConstantExpression) mc.Arguments[0]).Value;
          result = Expression.Call(mc.Object, mc.Method, Expression.Constant(map.IndexOf(value)));
        }
      else if (mc.Method.Name == "get_Item" && isReplacing && group != null) {
        var value = (int)((ConstantExpression)mc.Arguments[0]).Value;
        result = Expression.Call(mc.Object, mc.Method, Expression.Constant(group.IndexOf(value)));
      }
      return base.VisitMethodCall(result);
    }

    public List<int> ProcessPredicate(Expression predicate)
    {
      try {
        isReplacing = false;
        map = new List<int>();
        group = null;
        Visit(predicate);
        return map;
      }
      finally {
        map = null;
      }
    }

    public Expression ReplaceMappings(Expression predicate, List<int> mapping, List<int> groupMap)
    {
      isReplacing = true;
      group = groupMap;
      map = mapping;
      return Visit(predicate);
    }
  }
}
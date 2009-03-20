// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Elena Vakhtina
// Created:    2009.03.20

using System;
using System.Linq.Expressions;
using Xtensive.Core.Reflection;
using Xtensive.Core.Tuples;

namespace Xtensive.Storage.Rse
{
  [Serializable]
  public static class TupleExpressionHelper
  {
    public static MethodCallExpression AsTupleAccess(this Expression e)
    {
      if (e.NodeType == ExpressionType.Call) {
        var mc = (MethodCallExpression)e;
        if (mc.Object != null && mc.Object.Type == typeof(Tuple))
          if (mc.Method.Name == WellKnown.Tuple.GetValue || mc.Method.Name == WellKnown.Tuple.GetValueOrDefault)
            return mc;
      }
      return null;
    }

    public static int GetTupleAccessArgument(this Expression e)
    {
      var mc = e.AsTupleAccess();
      if (mc != null)
        try {
          return mc.Arguments[0].NodeType==ExpressionType.Constant
            ? ((int) ((ConstantExpression) mc.Arguments[0]).Value)
            : Expression.Lambda<Func<int>>(mc.Arguments[0]).Compile().Invoke();
        }
      catch {}
      return -1;
    }
  }
}
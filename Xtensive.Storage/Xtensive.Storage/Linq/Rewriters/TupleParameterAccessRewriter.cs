// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2009.04.25

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq.Expressions;
using Xtensive.Core.Parameters;
using Xtensive.Core.Tuples;
using Xtensive.Storage.Rse.Providers;
using Xtensive.Core.Linq;

namespace Xtensive.Storage.Linq.Rewriters
{
  [Serializable]
  public class TupleParameterAccessRewriter : ExpressionVisitor
  {
    private readonly List<int> mappings = new List<int>();
    private readonly Parameter<Tuple> tupleParameter;

    public static CompilableProvider Rewrite(CompilableProvider provider, Parameter<Tuple> tupleParameter, List<int> mappings)
    {
      var expressionAnalyzer = new TupleParameterAccessRewriter(tupleParameter, mappings);
      var providerAnalyzer = new CompilableProviderVisitor(expressionAnalyzer.Rewrite);
      return providerAnalyzer.VisitCompilable(provider);
    }
    
    protected override Expression VisitMethodCall(MethodCallExpression mc)
    {
      if (mc.Object.NodeType==ExpressionType.MemberAccess) {
        var m = (MemberExpression)mc.Object;
        if (m.Member==WellKnownMembers.ParameterOfTupleValue 
          && m.Expression.NodeType==ExpressionType.Constant 
          && ((ConstantExpression) m.Expression).Value==tupleParameter) {
          var oldIndex = (int)((ConstantExpression)mc.Arguments[0]).Value;
          var newIndex = mappings.IndexOf(oldIndex);
          return Expression.Call(mc.Object, mc.Method, Expression.Constant(newIndex));
        }
      }
      return base.VisitMethodCall(mc);
    }

    private Expression Rewrite(Provider provider, Expression expression)
    {
      return Visit(expression);
    }

    // Constructor

    private TupleParameterAccessRewriter(Parameter<Tuple> tupleParameter, List<int> mappings)
    {
      this.tupleParameter = tupleParameter;
      this.mappings = mappings;
    }  
  }
}
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
using Xtensive.Storage.Rse;
using Xtensive.Storage.Rse.Providers;
using Xtensive.Core.Linq;

namespace Xtensive.Storage.Linq.Rewriters
{
  [Serializable]
  internal class TupleParameterMappingAnalyzer : ExpressionVisitor
  {
    private readonly List<int> mapping = new List<int>();
    private readonly Parameter<Tuple> tupleParameter;

    public static List<int> Analyze(CompilableProvider provider, Parameter<Tuple> tupleParameter)
    {
      var expressionAnalyzer = new TupleParameterMappingAnalyzer(tupleParameter);
      var providerAnalyzer = new CompilableProviderVisitor(expressionAnalyzer.Analyze);
      providerAnalyzer.VisitCompilable(provider);
      return expressionAnalyzer.mapping;
    }
    
    protected override Expression VisitMethodCall(MethodCallExpression mc)
    {
      if (mc.Object.NodeType==ExpressionType.MemberAccess) {
        var m = (MemberExpression)mc.Object;
        if (m.Member==WellKnownMembers.ParameterOfTupleValue 
          && m.Expression.NodeType==ExpressionType.Constant 
          && ((ConstantExpression) m.Expression).Value==tupleParameter) {
          mapping.Add((int)((ConstantExpression)mc.Arguments[0]).Value);
        }
      }
      return base.VisitMethodCall(mc);
    }

    private Expression Analyze(Provider provider, Expression expression)
    {
      return Visit(expression);
    }

    // Constructor

    private TupleParameterMappingAnalyzer(Parameter<Tuple> tupleParameter)
    {
      this.tupleParameter = tupleParameter;
    }
  }
}
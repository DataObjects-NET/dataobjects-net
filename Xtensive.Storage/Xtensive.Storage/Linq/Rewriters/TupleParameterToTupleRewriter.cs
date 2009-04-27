// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: 
// Created:    2009.04.28

using System;
using System.Diagnostics;
using System.Linq.Expressions;
using Xtensive.Core.Parameters;
using Xtensive.Core.Reflection;
using Xtensive.Core.Tuples;
using Xtensive.Storage.Rse;
using Xtensive.Storage.Rse.Providers;
using Xtensive.Storage.Rse.Providers.Compilable;
using Xtensive.Core.Linq;

namespace Xtensive.Storage.Linq.Rewriters
{
  [Serializable]
  internal class TupleParameterToTupleRewriter: ExpressionVisitor
  {
    private readonly Parameter<Tuple> parameterOfTuple;
    private readonly Expression tupleExpression;
    
    public static CompilableProvider Rewrite(CompilableProvider provider,
      Parameter<Tuple> parameterOfTuple, Tuple tuple)
    {
      var expressionRewriter = new TupleParameterToTupleRewriter(parameterOfTuple, tuple);
      var providerRewriter = new CompilableProviderVisitor(expressionRewriter.RewriteExpression);
      return providerRewriter.VisitCompilable(provider);
    }

    protected override Expression VisitMemberAccess(MemberExpression m)
    {
      if (m.Member != WellKnownMembers.ParameterOfTupleValue)
        return base.VisitMemberAccess(m);
      if (m.Expression.NodeType != ExpressionType.Constant)
        return base.VisitMemberAccess(m);
      var parameter = ((ConstantExpression) m.Expression).Value;
      if (parameter != parameterOfTuple)
        return base.VisitMemberAccess(m);
      return tupleExpression;
    }

    private Expression RewriteExpression(Provider provider, Expression expression)
    {
      return provider is FilterProvider ? Visit(expression) : expression;
    }

    // Constructor

    private TupleParameterToTupleRewriter(Parameter<Tuple> parameterOfTuple, Tuple tuple)
    {
      this.parameterOfTuple = parameterOfTuple;
      tupleExpression = Expression.Constant(tuple);
    }
  }
}

// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2009.04.24

using System;
using System.Linq.Expressions;
using Xtensive.Core.Linq;
using Xtensive.Core.Parameters;
using Xtensive.Core.Tuples;
using Xtensive.Storage.Rse;
using Xtensive.Storage.Rse.Providers;

namespace Xtensive.Storage.Linq.Rewriters
{
  [Serializable]
  internal class ApplyParameterRewriter : ExpressionVisitor
  {
    private readonly ConstantExpression parameterOfTupleExpression;
    private readonly ApplyParameter applyParameter;

    public static CompilableProvider Rewrite(CompilableProvider provider,
      Parameter<Tuple> parameterOfTuple, ApplyParameter applyParameter)
    {
      var expressionRewriter = new ApplyParameterRewriter(parameterOfTuple, applyParameter);
      var providerRewriter = new CompilableProviderVisitor(expressionRewriter.RewriteExpression);
      return providerRewriter.VisitCompilable(provider);
    }

    protected override Expression VisitMemberAccess(MemberExpression m)
    {
      if (m.Member!=WellKnownMembers.ApplyParameterValue)
        return base.VisitMemberAccess(m);
      if (m.Expression.NodeType!=ExpressionType.Constant)
        return base.VisitMemberAccess(m);
      var parameter = ((ConstantExpression) m.Expression).Value;
      if (parameter!=applyParameter)
        return base.VisitMemberAccess(m);
      return Expression.Property(parameterOfTupleExpression, WellKnownMembers.ParameterOfTupleValue);
    }

    private Expression RewriteExpression(Provider provider, Expression expression)
    {
      return Visit(expression);
    }

    // Constructor

    private ApplyParameterRewriter(Parameter<Tuple> parameterOfTuple, ApplyParameter applyParameter)
    {
      parameterOfTupleExpression = Expression.Constant(parameterOfTuple);
      this.applyParameter = applyParameter;
    }
  }
}
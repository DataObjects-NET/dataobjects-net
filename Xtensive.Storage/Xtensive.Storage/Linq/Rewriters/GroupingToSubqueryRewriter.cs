// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.04.15

using System.Linq.Expressions;
using Xtensive.Core.Linq;
using Xtensive.Core.Parameters;
using Xtensive.Core.Tuples;
using Xtensive.Storage.Rse;
using Xtensive.Storage.Rse.Providers;
using Xtensive.Storage.Rse.Providers.Compilable;

namespace Xtensive.Storage.Linq.Rewriters
{
  internal sealed class GroupingToSubqueryRewriter : ExpressionVisitor
  {
    private readonly Parameter<Tuple> parameterOfTuple;
    private readonly Expression applyParameterExpression;
    
    public static CompilableProvider Rewrite(CompilableProvider provider,
      Parameter<Tuple> parameterOfTuple, ApplyParameter applyParameter)
    {
      var expressionRewriter = new GroupingToSubqueryRewriter(parameterOfTuple, applyParameter);
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
      return Expression.Property(applyParameterExpression, WellKnownMembers.ApplyParameterValue);
    }

    private Expression RewriteExpression(Provider provider, Expression expression)
    {
      return provider is FilterProvider ? Visit(expression) : expression;
    }

    // Constructor

    private GroupingToSubqueryRewriter(Parameter<Tuple> parameterOfTuple, ApplyParameter applyParameter)
    {
      this.parameterOfTuple = parameterOfTuple;
      applyParameterExpression = Expression.Constant(applyParameter);
    }
  }
}

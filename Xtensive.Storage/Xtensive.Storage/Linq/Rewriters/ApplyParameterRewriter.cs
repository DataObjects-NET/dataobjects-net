// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2009.06.09

using System;
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
  public class ApplyParameterRewriter: ExpressionVisitor
  {
    private readonly Expression newApplyParameterValueExpression;
    private readonly ApplyParameter oldApplyParameter;

    public static CompilableProvider Rewrite(CompilableProvider provider,
      ApplyParameter oldParameter, ApplyParameter newParameter)
    {
      var expressionRewriter = new ApplyParameterRewriter(oldParameter, newParameter);
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
      if (parameter!=oldApplyParameter)
        return base.VisitMemberAccess(m);
      return newApplyParameterValueExpression;
    }

    private Expression RewriteExpression(Provider provider, Expression expression)
    {
      return Visit(expression);
    }


    // Constructors

    private ApplyParameterRewriter(ApplyParameter oldParameter, ApplyParameter newParameter)
    {
      newApplyParameterValueExpression = Expression.Property(Expression.Constant(newParameter), WellKnownMembers.ApplyParameterValue);
      this.oldApplyParameter = oldParameter;
    }
  }
}
// Copyright (C) 2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2010.04.27

using System.Linq.Expressions;
using Xtensive.Orm.Rse;
using Xtensive.Orm.Rse.Helpers;
using Xtensive.Orm.Rse.Providers;
using Xtensive.Orm.Rse.Providers.Compilable;
using ExpressionVisitor = Xtensive.Linq.ExpressionVisitor;

namespace Xtensive.Orm.Linq.Rewriters
{
  internal sealed class SubqueryFilterRemover : CompilableProviderVisitor
  {
    private class SubqueryFilterChecker : ExpressionVisitor
    {
      private readonly ApplyParameter filterParameter;
      private bool @continue;
      private bool matchFound;

      public bool Check(Expression expression)
      {
        matchFound = false;
        @continue = true;
        Visit(expression);
        return matchFound && @continue;
      }

      protected override Expression VisitConstant(ConstantExpression c)
      {
        if (c.Value==filterParameter)
          matchFound = true;
        return c;
      }

      protected override Expression VisitBinary(BinaryExpression b)
      {
        if (b.NodeType==ExpressionType.Equal) {
          var leftAccess = b.Left.AsTupleAccess();
          var rightAccess = b.Right.AsTupleAccess();
          @continue &= leftAccess!=null && rightAccess!=null;
          if (@continue) {
            var leftIsParameterBound = leftAccess.Object.NodeType==ExpressionType.Parameter;
            var rightIsParameterBound = rightAccess.Object.NodeType==ExpressionType.Parameter;
            @continue = leftIsParameterBound!=rightIsParameterBound;
          }
        }
        else if (b.NodeType==ExpressionType.AndAlso) {
          @continue &= b.Left is BinaryExpression && b.Right is BinaryExpression;
        }
        else
          @continue = false;
        if (@continue) {
          Visit(b.Left);
          Visit(b.Right);
        }
        return b;
      }


      // Constructors

      public SubqueryFilterChecker(ApplyParameter filterParameter)
      {
        this.filterParameter = filterParameter;
      }
    }

    private readonly SubqueryFilterChecker checker;

    protected override Provider VisitFilter(FilterProvider provider)
    {
      return checker.Check(provider.Predicate.Body)
        ? provider.Source
        : base.VisitFilter(provider);
    }

    public static CompilableProvider Process(CompilableProvider target, ApplyParameter filterParameter)
    {
      return new SubqueryFilterRemover(filterParameter).VisitCompilable(target);
    }


    // Constructors

    private SubqueryFilterRemover(ApplyParameter filterParameter)
    {
      checker = new SubqueryFilterChecker(filterParameter);
    }
  }
}
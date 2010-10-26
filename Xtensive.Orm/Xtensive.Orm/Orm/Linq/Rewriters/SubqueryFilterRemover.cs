// Copyright (C) 2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2010.04.27

using System;
using System.Linq.Expressions;
using Xtensive.Linq;
using Xtensive.Storage.Rse;
using Xtensive.Storage.Rse.Providers;
using Xtensive.Storage.Rse.Providers.Compilable;
using Xtensive.Storage.Rse.Helpers;
using ExpressionVisitor = Xtensive.Linq.ExpressionVisitor;

namespace Xtensive.Orm.Linq.Rewriters
{
  internal sealed class SubqueryFilterRemover : CompilableProviderVisitor
  {
    private readonly SubqueryFilterChecker checker;

    class SubqueryFilterChecker : ExpressionVisitor
    {
      private readonly ApplyParameter applyParameter;
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
        if (c.Value == applyParameter)
          matchFound = true;
        return c;
      }

      protected override Expression VisitBinary(BinaryExpression b)
      {
        if (b.NodeType == ExpressionType.Equal) {
          var leftAccess = b.Left.AsTupleAccess();
          var rightAccess =b.Right.AsTupleAccess();
          @continue &= leftAccess != null && rightAccess != null;
          if (@continue) {
            var leftIsParameterBound = leftAccess.Object.NodeType == ExpressionType.Parameter;
            var rightIsParameterBound = rightAccess.Object.NodeType == ExpressionType.Parameter;
            @continue = leftIsParameterBound != rightIsParameterBound;
          }
        }
        else if (b.NodeType == ExpressionType.AndAlso) {
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

      public SubqueryFilterChecker(ApplyParameter applyParameter)
      {
        this.applyParameter = applyParameter;
      }
    }

    protected override Provider VisitFilter(FilterProvider provider)
    {
      return checker.Check(provider.Predicate.Body) 
        ? provider.Source 
        : base.VisitFilter(provider);
    }


    // Constructors

    public SubqueryFilterRemover(ApplyParameter applyParameter)
    {
      checker = new SubqueryFilterChecker(applyParameter);
    }
  }
}
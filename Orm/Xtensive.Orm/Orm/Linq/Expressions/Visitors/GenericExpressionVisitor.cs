// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2009.05.21

using System;
using System.Diagnostics;
using System.Linq.Expressions;
using Xtensive.Linq;
using ExpressionVisitor = Xtensive.Linq.ExpressionVisitor;

namespace Xtensive.Orm.Linq.Expressions.Visitors
{
  internal class GenericExpressionVisitor<T> : ExpressionVisitor
    where T : class
  {
    private readonly Func<T, Expression> genericProcessor;

    public static Expression Process(Expression target, Func<T, Expression> genericProcessor)
    {
      var visitor = new GenericExpressionVisitor<T>(genericProcessor);
      
      if (RemapScope.CurrentContext!=null)
        return visitor.Visit(target);

      using (new RemapScope())
        return visitor.Visit(target);
    }

    protected override Expression VisitUnknown(Expression e)
    {
      var mapped = e as T;
      if (mapped!=null)
        return VisitGenericExpression(mapped);

      var extendedExpression = e as ExtendedExpression;
      if (extendedExpression != null && extendedExpression.ExtendedType == ExtendedExpressionType.Marker) {
        var marker = (MarkerExpression) e;
        var result = Visit(marker.Target);
        if (result == marker.Target)
          return result;
        return new MarkerExpression(result, marker.MarkerType);
      }

      return base.VisitUnknown(e);
    }

    protected virtual Expression VisitGenericExpression(T generic)
    {
      if (genericProcessor!=null)
        return genericProcessor.Invoke(generic);
      throw new NotSupportedException(Strings.ExUnableToUseBaseImplementationOfVisitGenericExpressionWithoutSpecifyingGenericProcessorDelegate);
    }


    // Constructors

    protected GenericExpressionVisitor()
      : this(null)
    {
    }

    protected GenericExpressionVisitor(Func<T, Expression> mappingProcessor)
    {
      this.genericProcessor = mappingProcessor;
    }
  }
}
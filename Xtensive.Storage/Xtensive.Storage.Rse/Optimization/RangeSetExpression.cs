// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexander Nikolaev
// Created:    2009.03.24

using System;
using System.Linq.Expressions;
using Xtensive.Core;
using Xtensive.Core.Tuples;
using Xtensive.Indexing;

namespace Xtensive.Storage.Rse.Optimization
{ 
  internal class RangeSetExpression : Expression
  {
    public Expression Source { get; private set; }

    public bool AlwaysFull { get; private set; }

    public RangeSetOriginInfo Origin { get; private set; }

    private static Expression ValidateExpression(Expression source)
    {
      ArgumentValidator.EnsureArgumentNotNull(source, "source");
      if (source.Type != typeof(RangeSet<Entire<Tuple>>))
        throw new ArgumentException(String.Format(Resources.Strings.ExExpressionMustReturnValueOfTypeX,
          typeof (RangeSet<Entire<Tuple>>)));
      return source;
    }

    public void Unite(Expression unionResult, RangeSetExpression other)
    {
      ValidateExpression(unionResult);
      Source = unionResult;
      AlwaysFull = AlwaysFull || other.AlwaysFull;
      Origin = null;
    }

    public void Intersect(Expression intersectionResult, RangeSetExpression other)
    {
      ValidateExpression(intersectionResult);
      Source = intersectionResult;
      AlwaysFull = AlwaysFull && other.AlwaysFull;
      Origin = null;
    }

    public void Invert(Expression invertionResult)
    {
      ValidateExpression(invertionResult);
      Source = invertionResult;
      AlwaysFull = false;
      if (Origin != null)
        Origin.ReverseComparison();
    }

    // Constructors

    public RangeSetExpression(Expression source, RangeSetOriginInfo origin, bool alwaysFull)
      : base(ValidateExpression(source).NodeType, typeof(RangeSet<Entire<Tuple>>))
    {
      Source = source;
      Origin = origin;
      AlwaysFull = alwaysFull;
    }
  }
}
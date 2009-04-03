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

namespace Xtensive.Storage.Rse.Optimization.IndexSelection
{
  internal sealed class RangeSetInfo
  {
    public Expression Source { get; private set; }

    public bool AlwaysFull { get; private set; }

    public TupleExpressionInfo Origin { get; private set; }

    private static void ValidateExpression(Expression source)
    {
      ArgumentValidator.EnsureArgumentNotNull(source, "source");
      if (source.Type != typeof(RangeSet<Entire<Tuple>>))
        throw new ArgumentException(String.Format(Resources.Strings.ExExpressionMustReturnValueOfTypeX,
          typeof (RangeSet<Entire<Tuple>>)));
    }

    public void Unite(Expression unionResult, RangeSetInfo other)
    {
      ValidateExpression(unionResult);
      Source = unionResult;
      AlwaysFull = AlwaysFull || other.AlwaysFull;
      Origin = null;
    }

    public void Intersect(Expression intersectionResult, RangeSetInfo other)
    {
      ValidateExpression(intersectionResult);
      Source = intersectionResult;
      AlwaysFull = AlwaysFull && other.AlwaysFull;
      Origin = null;
    }

    public void Invert(Expression inversionResult)
    {
      ValidateExpression(inversionResult);
      Source = inversionResult;
      AlwaysFull = false;
      Origin = null;
    }

    // Constructors

    public RangeSetInfo(Expression source, TupleExpressionInfo origin, bool alwaysFull)
    {
      Source = source;
      Origin = origin;
      AlwaysFull = alwaysFull;
    }
  }
}
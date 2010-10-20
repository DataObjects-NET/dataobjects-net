// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexander Nikolaev
// Created:    2009.03.24

using System;
using System.Linq.Expressions;
using Xtensive.Core;
using Xtensive.Linq;
using Xtensive.Tuples;
using Tuple = Xtensive.Tuples.Tuple;
using Xtensive.Indexing;
using Xtensive.Storage.Rse.Resources;

namespace Xtensive.Storage.Rse.PreCompilation.Optimization.IndexSelection
{
  [Serializable]
  internal sealed class RangeSetInfo
  {
    private Func<RangeSet<Entire<Tuple>>> rangeSetCreator;
    private bool creatorIsStale;

    public Expression Source { get; private set; }

    public bool AlwaysFull { get; private set; }

    public TupleExpressionInfo Origin { get; private set; }

    public void Unite(Expression unionResult, RangeSetInfo other)
    {
      ValidateExpression(unionResult);
      Source = unionResult;
      AlwaysFull = AlwaysFull || other.AlwaysFull;
      Origin = null;
      creatorIsStale = true;
    }

    public void Intersect(Expression intersectionResult, RangeSetInfo other)
    {
      ValidateExpression(intersectionResult);
      Source = intersectionResult;
      AlwaysFull = AlwaysFull && other.AlwaysFull;
      Origin = null;
      creatorIsStale = true;
    }

    public void Invert(Expression inversionResult)
    {
      ValidateExpression(inversionResult);
      Source = inversionResult;
      AlwaysFull = false;
      Origin = null;
      creatorIsStale = true;
    }

    public RangeSet<Entire<Tuple>> GetRangeSet()
    {
      if (rangeSetCreator == null || creatorIsStale) {
        rangeSetCreator = Expression.Lambda<Func<RangeSet<Entire<Tuple>>>>(Source).CachingCompile();
        creatorIsStale = false;
      }
      return rangeSetCreator();
    }

    public Expression<Func<RangeSet<Entire<Tuple>>>> GetSourceAsLambda()
    {
      var lambda = Source as LambdaExpression;
      if (lambda != null)
        return (Expression<Func<RangeSet<Entire<Tuple>>>>)lambda;
      return Expression.Lambda<Func<RangeSet<Entire<Tuple>>>>(Source);
    }

    #region Private \ internal methods
    private static void ValidateExpression(Expression source)
    {
      ArgumentValidator.EnsureArgumentNotNull(source, "source");
      if (source.Type != typeof(RangeSet<Entire<Tuple>>))
        throw new ArgumentException(String.Format(Strings.ExTypeOfExpressionReturnValueIsNotX,
          typeof (RangeSet<Entire<Tuple>>)));
    }
    #endregion


    // Constructors

    public RangeSetInfo(Expression source, TupleExpressionInfo origin, bool alwaysFull)
    {
      Source = source;
      Origin = origin;
      AlwaysFull = alwaysFull;
    }
  }
}
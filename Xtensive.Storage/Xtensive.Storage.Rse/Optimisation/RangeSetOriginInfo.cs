// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexander Nikolaev
// Created:    2009.03.25

using System.Linq.Expressions;
using Xtensive.Core;

namespace Xtensive.Storage.Rse.Optimisation
{
  /// <summary>
  /// Original data was used to create a <see cref="RangeSetExpression"/>.
  /// </summary>
  internal class RangeSetOriginInfo
  {
    private ExpressionType comparison;
    public ExpressionType Comparison {
      get { return comparison;}
      private set { comparison = value; }
    }

    public int TupleField;

    public readonly Expression KeyValue;

    public void ReverseComparison()
    {
      ExtractingVisitor.ReverseOperation(ref comparison);
    }

    public RangeSetOriginInfo(ExpressionType comparison, int tupleField, Expression keyValue)
    {
      ArgumentValidator.EnsureArgumentNotNull(keyValue, "keyValue");
      ArgumentValidator.EnsureArgumentIsInRange(tupleField, 0, int.MaxValue, "tupleField");
      Comparison = comparison;
      TupleField = tupleField;
      KeyValue = keyValue;
    }
  }
}
// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexander Nikolaev
// Created:    2009.03.26

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Xtensive.Core.Linq.Normalization;
using Xtensive.Core.Tuples;
using Xtensive.Indexing;
using Xtensive.Storage.Rse.Optimization.IndexSelection;

namespace Xtensive.Storage.Tests.Rse
{
  internal static class ExpressionHelper
  {
    public static Conjunction<Expression> AddBoolean(this Conjunction<Expression> exp,
      Expression<Func<Tuple, bool>> boolean)
    {
      exp.Operands.Add(boolean.Body);
      return exp;
    }

    public static DisjunctiveNormalized AddCnf(this DisjunctiveNormalized root,
     Conjunction<Expression> exp)
    {
      root.Operands.Add(exp);
      return root;
    }

    public static RangeSet<Entire<Tuple>> GetRangeSetForSingleIndex(
      this Dictionary<Expression, List<RsExtractionResult>> extractionResults)
    {
      RangeSetInfo result = null;
      foreach (var pair in extractionResults)
        foreach (var extractionResult in pair.Value)
          if (result == null)
            result = extractionResult.RangeSetInfo;
          else
            result = RangeSetExpressionBuilder.BuildUnite(result, extractionResult.RangeSetInfo);
      return result.GetRangeSet();
    }
  }
}
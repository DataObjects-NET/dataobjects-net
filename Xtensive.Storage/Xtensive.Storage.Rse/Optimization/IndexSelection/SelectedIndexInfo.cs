// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexander Nikolaev
// Created:    2009.04.03

using System;
using Xtensive.Core;
using Xtensive.Core.Tuples;
using Xtensive.Indexing;
using Xtensive.Storage.Model;

namespace Xtensive.Storage.Rse.Optimization.IndexSelection
{
  [Serializable]
  internal class SelectedIndexInfo
  {
    private readonly Func<RangeSet<Entire<Tuple>>> rangeSetEvaluator;
    public readonly IndexInfo Index;

    public RangeSet<Entire<Tuple>> GetRangeSet()
    {
      throw new NotImplementedException();
    }

    // Constructors

    public SelectedIndexInfo(IndexInfo indexInfo, RsExtractionResult extractionResult)
    {
      ArgumentValidator.EnsureArgumentNotNull(indexInfo, "indexInfo");
      ArgumentValidator.EnsureArgumentNotNull(extractionResult, "extractionResult");
      Index = indexInfo;
      rangeSetEvaluator = (Func<RangeSet<Entire<Tuple>>>)extractionResult.GetResult().Compile();
    }
  }
}
// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexander Nikolaev
// Created:    2009.04.06

using System;
using System.Diagnostics;
using Xtensive.Core;
using Xtensive.Core.Tuples;
using Xtensive.Indexing;
using Xtensive.Storage.Model;

namespace Xtensive.Storage.Rse.Optimization.IndexSelection
{
  [Serializable]
  internal struct CostInfo
  {
    public readonly IndexInfo IndexInfo;
    public readonly RangeSet<Entire<Tuple>> RangeSet;
    public readonly double Cost;

    // Constructors

    public CostInfo(IndexInfo indexInfo, RangeSet<Entire<Tuple>> rangeSet, double cost)
    {
      ArgumentValidator.EnsureArgumentNotNull(indexInfo, "indexInfo");
      ArgumentValidator.EnsureArgumentNotNull(rangeSet, "rangeSet");
      ArgumentValidator.EnsureArgumentIsInRange(cost, 0, double.MaxValue, "cost");
      IndexInfo = indexInfo;
      RangeSet = rangeSet;
      Cost = cost;
    }
  }
}
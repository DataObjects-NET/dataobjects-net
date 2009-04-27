// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexander Nikolaev
// Created:    2009.04.06

using Xtensive.Core.Tuples;
using Xtensive.Indexing;
using Xtensive.Storage.Model;

namespace Xtensive.Storage.Rse.Optimization.IndexSelection
{
  internal interface ICostEvaluator
  {
    CostInfo Evaluate(IndexInfo indexInfo, RangeSet<Entire<Tuple>> rangeSet);
  }
}
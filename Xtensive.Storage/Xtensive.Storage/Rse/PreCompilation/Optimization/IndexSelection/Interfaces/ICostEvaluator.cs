// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexander Nikolaev
// Created:    2009.04.06

using Xtensive.Tuples;
using Tuple = Xtensive.Tuples.Tuple;
using Xtensive.Indexing;
using Xtensive.Storage.Model;

namespace Xtensive.Storage.Rse.PreCompilation.Optimization.IndexSelection
{
  internal interface ICostEvaluator
  {
    CostInfo Evaluate(IndexInfo indexInfo, RangeSet<Entire<Tuple>> rangeSet);
  }
}
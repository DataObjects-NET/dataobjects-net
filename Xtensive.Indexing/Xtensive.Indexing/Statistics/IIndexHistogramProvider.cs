// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexander Nikolaev
// Created:    2009.04.10

namespace Xtensive.Indexing.Statistics
{
  internal interface IIndexHistogramProvider<TKey>
  {
    Histogram<TKey, double> GetCountHistogram(int maxKeyCount);
  }
}
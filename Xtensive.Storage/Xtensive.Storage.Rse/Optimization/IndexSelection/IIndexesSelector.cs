// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexander Nikolaev
// Created:    2009.04.03

using System.Collections.Generic;
using Xtensive.Storage.Model;

namespace Xtensive.Storage.Rse.Optimization.IndexSelection
{
  internal interface IIndexesSelector
  {
    HashSet<SelectedIndexInfo> Select(IEnumerable<IndexInfo> indexes, IEnumerable<RangeSetInfo> rangeSets);
  }
}
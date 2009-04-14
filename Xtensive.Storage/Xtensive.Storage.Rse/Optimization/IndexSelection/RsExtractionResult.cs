// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexander Nikolaev
// Created:    2009.03.23

using System;
using System.Linq.Expressions;
using Xtensive.Core;
using Xtensive.Core.Tuples;
using Xtensive.Indexing;
using Xtensive.Storage.Model;

namespace Xtensive.Storage.Rse.Optimization.IndexSelection
{
  /// <summary>
  /// Result of extraction a <see cref="RangeSet{T}"/> from a predicate.
  /// </summary>
  [Serializable]
  internal sealed class RsExtractionResult
  {
    public readonly RangeSetInfo RangeSetInfo;
    public readonly IndexInfo IndexInfo;


    // Constructors

    public RsExtractionResult(IndexInfo indexInfo, RangeSetInfo rangeSetInfo)
    {
      ArgumentValidator.EnsureArgumentNotNull(indexInfo, "indexInfo");
      ArgumentValidator.EnsureArgumentNotNull(rangeSetInfo, "rangeSetInfo");
      IndexInfo = indexInfo;
      RangeSetInfo = rangeSetInfo;
    }
  }
}
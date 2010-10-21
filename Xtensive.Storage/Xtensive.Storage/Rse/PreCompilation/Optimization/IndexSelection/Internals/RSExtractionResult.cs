// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexander Nikolaev
// Created:    2009.03.23

using System;
using Xtensive.Core;
using Xtensive.Indexing;
using Xtensive.Storage.Model;

namespace Xtensive.Storage.Rse.PreCompilation.Optimization.IndexSelection
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
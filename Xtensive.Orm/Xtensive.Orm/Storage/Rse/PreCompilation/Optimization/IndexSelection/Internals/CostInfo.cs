// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexander Nikolaev
// Created:    2009.04.06

using System;
using Xtensive.Core;

namespace Xtensive.Storage.Rse.PreCompilation.Optimization.IndexSelection
{
  [Serializable]
  internal struct CostInfo
  {
    public readonly double RecordCount;
    public readonly double SeekCount;
    public readonly double TotalCost;


    // Constructors

    public CostInfo(double recordCount, double seekCount)
    {
      ArgumentValidator.EnsureArgumentNotNull(recordCount, "recordCount");
      ArgumentValidator.EnsureArgumentNotNull(seekCount, "seekCount");
      RecordCount = recordCount;
      SeekCount = seekCount;
      TotalCost = RecordCount + SeekCount;
    }
  }
}
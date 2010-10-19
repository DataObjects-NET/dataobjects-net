// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexander Nikolaev
// Created:    2009.04.17

using System;
using System.Diagnostics;
using Xtensive.Internals.DocTemplates;

namespace Xtensive.Indexing.Optimization
{
  /// <summary>
  /// Data collected by an implementor of <see cref="IStatistics{T}"/>.
  /// </summary>
  [Serializable]
  [DebuggerDisplay("RecordCount = {Count}, SeekCount = {SeekCount}")]
  public struct StatisticsData
  {
    /// <summary>
    /// The count of records to be loaded.
    /// </summary>
    public readonly double RecordCount;

    /// <summary>
    /// The count of seeks which are necessary to load data.
    /// </summary>
    public readonly double SeekCount;

    
    // Constrcutors    
    
    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="recordCount">The record count.</param>
    /// <param name="seekCount">The seek count.</param>
    public StatisticsData(double recordCount, double seekCount)
    {
      RecordCount = recordCount;
      SeekCount = seekCount;
    }
  }
}
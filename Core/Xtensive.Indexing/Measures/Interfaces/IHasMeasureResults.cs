// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.02.14

namespace Xtensive.Indexing.Measures
{
  /// <summary>
  /// Indicates that this instance has some measure results associated with it.
  /// </summary>
  public interface IHasMeasureResults<TItem>
  {
    /// <summary>
    /// Gets the measurements.
    /// </summary>
    /// <value>The measurements.</value>
    IMeasureResultSet<TItem> MeasureResults { get; }
  }
}
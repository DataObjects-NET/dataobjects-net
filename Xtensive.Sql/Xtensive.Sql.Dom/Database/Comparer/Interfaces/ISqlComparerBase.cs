// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Aleksey Gamzov
// Created:    2008.08.18

namespace Xtensive.Sql.Dom.Database.Comparer
{
  /// <summary>
  /// Tagging interface for any comparers supported by
  /// <see cref="SqlComparerProvider"/>.
  /// </summary>
  public interface ISqlComparerBase
  {
    /// <summary>
    /// Gets the provider this comparer is associated with.
    /// </summary>
    ISqlComparerProvider Provider { get; }
  }
}
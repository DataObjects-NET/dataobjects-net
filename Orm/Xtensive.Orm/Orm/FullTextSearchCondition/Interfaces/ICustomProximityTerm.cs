// Copyright (C) 2003-2016 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kulakov
// Created:    2016.12.08

namespace Xtensive.Orm.FullTextSearchCondition.Interfaces
{
  /// <summary>
  /// Custom proximity term.
  /// Allow to configure parameters of proximity.
  /// </summary>
  public interface ICustomProximityTerm : IProximityTerm
  {
    /// <summary>
    /// Gets maximum distance between <see cref="IProximityTerm.Terms"/>.
    /// </summary>
    long? MaxDistance { get; }

    /// <summary>
    /// Gets value which indicates whether should keep <see cref="IProximityTerm.Terms"/> order.
    /// </summary>
    bool MatchOrder { get; }
  }
}
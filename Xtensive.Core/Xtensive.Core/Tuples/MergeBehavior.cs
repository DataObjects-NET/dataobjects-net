// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.07.21

namespace Xtensive.Tuples
{
  /// <summary>
  /// Possible tuple merge behaviors.
  /// </summary>
  public enum MergeBehavior
  {
    /// <summary>
    /// The same as <see cref="PreferOrigin"/>
    /// </summary>
    Default = PreferOrigin,

    /// <summary>
    /// Origin values are preferrable.
    /// </summary>
    PreferOrigin = 0,

    /// <summary>
    /// Difference values are preferrable.
    /// </summary>
    PreferDifference = 1,
  }
}
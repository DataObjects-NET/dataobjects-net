// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.07.21

namespace Xtensive.Core.Tuples
{
  /// <summary>
  /// Merge behavior enum.
  /// </summary>
  public enum MergeConflictBehavior
  {
    /// <summary>
    /// The same as <see cref="PreferTarget"/>
    /// </summary>
    Default = PreferTarget,

    /// <summary>
    /// Target values are more preferrable.
    /// </summary>
    PreferTarget = 0,

    /// <summary>
    /// Source values are more preferrable.
    /// </summary>
    PreferSource = 1,
  }
}
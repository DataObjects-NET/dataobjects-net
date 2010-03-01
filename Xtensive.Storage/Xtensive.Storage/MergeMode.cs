// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Ivan Galkin
// Created:    2009.11.26

namespace Xtensive.Storage
{
  /// <summary>
  /// Enumerate possible ways of merging entity states in
  /// <see cref="DisconnectedState"/>.
  /// </summary>
  public enum MergeMode
  {
    /// <summary>
    /// Default value. 
    /// The same as <see cref="Strict"/>.
    /// </summary>
    Default = Strict,
    /// <summary>
    /// Prevents merge on any version conflict.
    /// </summary>
    Strict = 0,
    /// <summary>
    /// Source value will be used on version conflict.
    /// </summary>
    PreferSource = 1,
    /// <summary>
    /// Target value will be used on version conflict.
    /// </summary>
    PreferTarget = 2,
  }
}
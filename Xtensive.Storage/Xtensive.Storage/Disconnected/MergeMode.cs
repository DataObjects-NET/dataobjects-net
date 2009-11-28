// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Ivan Galkin
// Created:    2009.11.26

namespace Xtensive.Storage.Disconnected
{
  /// <summary>
  /// Enumerate all possible merge entity state modes.
  /// </summary>
  public enum MergeMode
  {
    /// <summary>
    /// Default = Restrict.
    /// </summary>
    Default = Restrict,
    /// <summary>
    /// Restricts merge on version conflict.
    /// </summary>
    Restrict = 0,
    /// <summary>
    /// Source value will be used on version confilct.
    /// </summary>
    PreferSource = 1,
    /// <summary>
    /// Target value will be used on version confilct.
    /// </summary>
    PreferTarget = 2,
  }
}
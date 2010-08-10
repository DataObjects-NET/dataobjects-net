// Copyright (C) 2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2010.08.04

using System;

namespace Xtensive.Storage
{
  /// <summary>
  /// Specifies version check mode for particular perstent property. Applied via <see cref="VersionAttribute"/>.
  /// </summary>
  [Serializable]
  public enum VersionMode
  {
    /// <summary>
    /// The field is included into entity version and its value requires manual update.
    /// </summary>
    Manual = 0,
    /// <summary>
    /// The field does not participate in version checks and don't included into entity version.
    /// </summary>
    Skip,
    /// <summary>
    /// The field is included into entity version and its value managed automatically.
    /// </summary>
    Auto,
    /// <summary>
    /// Default value is <see cref="Manual"/>.
    /// </summary>
    Default = Manual
  }
}
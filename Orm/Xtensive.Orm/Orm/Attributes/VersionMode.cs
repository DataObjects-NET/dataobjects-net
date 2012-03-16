// Copyright (C) 2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2010.08.04

using System;

namespace Xtensive.Orm
{
  /// <summary>
  /// Specifies version check mode for particular persistent property. Applied via <see cref="VersionAttribute"/>.
  /// </summary>
  [Serializable]
  public enum VersionMode
  {
    /// <summary>
    /// Default value.
    /// The same as <see cref="Manual"/>.
    /// </summary>
    Default = Manual,
    /// <summary>
    /// The field is included into entity version; its value must be updated manually.
    /// </summary>
    Manual = 0,
    /// <summary>
    /// The field must not be included into entity version.
    /// </summary>
    Skip,
    /// <summary>
    /// The field is included into entity version; its value is updated automatically.
    /// </summary>
    Auto,
  }
}
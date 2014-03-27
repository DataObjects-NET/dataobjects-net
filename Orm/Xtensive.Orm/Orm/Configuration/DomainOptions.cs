// Copyright (C) 2014 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2014.03.12

using System;

namespace Xtensive.Orm.Configuration
{
  /// <summary>
  /// Possible options for <see cref="Domain"/>.
  /// </summary>
  [Flags]
  public enum DomainOptions
  {
    /// <summary>
    /// Empty option set.
    /// </summary>
    None = 0,

    /// <summary>
    /// Enables dynamic mapping in <see cref="Domain"/>.
    /// </summary>
    DynamicMapping = 1 << 0,

    /// <summary>
    /// Default option set (<see cref="None"/>).
    /// </summary>
    Default = None,
  }
}
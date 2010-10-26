// Copyright (C) 2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2010.02.02

using System;

namespace Xtensive.Orm.Configuration
{
  /// <summary>
  /// A policy for dealing with database readers.
  /// </summary>
  [Serializable]
  public enum ReaderPreloadingPolicy
  {
    /// <summary>
    /// Default value is <see cref="Auto"/>.
    /// </summary>
    Default = Auto,

    /// <summary>
    /// Preload reader if and only if the underlying storage does not support MARS.
    /// </summary>
    Auto = 0,

    /// <summary>
    /// Always preload reader.
    /// </summary>
    Always = 1,

    /// <summary>
    /// Never preload reader.
    /// </summary>
    Never = 2,
  }
}
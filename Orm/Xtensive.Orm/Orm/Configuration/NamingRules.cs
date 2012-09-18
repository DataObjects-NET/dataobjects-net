// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2007.09.04

using System;

namespace Xtensive.Orm.Configuration
{
  /// <summary>
  /// Miscellaneous naming policy rules.
  /// </summary>
  [Flags]
  [Serializable]
  public enum NamingRules
  {
    /// <summary>
    /// No modification should be applied.
    /// </summary>
    None = 0,

    /// <summary>
    /// All hyphens should be replaced with underscore symbol.
    /// This option is mutually exclusive with <see cref="RemoveHyphens"/>.
    /// </summary>
    UnderscoreHyphens = 0x1,

    /// <summary>
    /// All dots should be replaced with underscore symbol.
    /// This option is mutually exclusive with <see cref="RemoveDots"/>.
    /// </summary>
    UnderscoreDots = 0x2,

    /// <summary>
    /// All hyphens should be removed.
    /// This option is mutually exclusive with <see cref="UnderscoreHyphens"/>.
    /// </summary>
    RemoveHyphens = 0x4,

    /// <summary>
    /// All dots should be removed.
    /// This option is mutually exclusive with <see cref="UnderscoreDots"/>.
    /// </summary>
    RemoveDots = 0x8,

    /// <summary>
    /// Default value is <see cref="None"/>.
    /// </summary>
    Default = None,
  }
}
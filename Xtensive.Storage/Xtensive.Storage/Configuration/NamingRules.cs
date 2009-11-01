// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2007.09.04

using System;

namespace Xtensive.Storage.Configuration
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
    Default = 0,

    /// <summary>
    /// All hyphens should be replaced with underscore symbol.
    /// </summary>
    UnderscoreHyphens = 0x1,

    /// <summary>
    /// All dots should be replaced with underscore symbol.
    /// </summary>
    UnderscoreDots = 0x2,
  }
}
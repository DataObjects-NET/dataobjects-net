// Copyright (C) 2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2010.01.12

namespace Xtensive.Sql.Info
{
  /// <summary>
  /// Defines full-text options.
  /// </summary>
  public enum FullTextSearchFeatures
  {
    /// <summary>
    /// Indicates that RDBMS doesn't support full-text at all.
    /// </summary>
    None = 0x0,

    /// <summary>
    /// Indicates that RDBMS uses table with 'KEY' and 'RANK' columns.
    /// </summary>
    SingleKeyRankTable = 0x1,

    /// <summary>
    /// Indicates that RDBMS supports column functions to calculate full-text rank.
    /// </summary>
    Full = 0x2,
  }
}
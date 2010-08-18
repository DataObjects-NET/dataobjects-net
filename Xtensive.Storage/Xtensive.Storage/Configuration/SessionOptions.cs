// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2008.10.06

using System;

namespace Xtensive.Storage.Configuration
{
  /// <summary>
  /// Enumerates possible options of the <see cref="Session"/>.
  /// </summary>
  [Flags]
  public enum SessionOptions
  {
    /// <summary>
    /// None of <see cref="SessionOptions"/>.
    /// Value is <see langword="0x0"/>.
    /// </summary>
    None = 0x0,

    /// <summary>
    /// Session will persist changes transparently before transaction commit and queries.
    /// </summary>
    AutoPersist = 0x2,

    /// <summary>
    /// Entity state will not cross transaction boundaries.
    /// </summary>
    Transactional = 0x4,

    /// <summary>
    /// Transactions will actually be opened just before execution of DB command. 
    /// This option is ignored for non-SQL providers.
    /// Value is <see langword="0x8" />.
    /// </summary>
    AutoShortenTransactions = 0x8
  }
}
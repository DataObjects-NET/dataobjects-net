// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2008.10.06

using System;
using Xtensive.Storage;

namespace Xtensive.Storage.Configuration
{
  /// <summary>
  /// Enumerates possible options of the <see cref="Session"/>.
  /// </summary>
  [Flags]
  public enum SessionOptions
  {
    /// <summary>
    /// Default options is <see cref="None"/>.
    /// </summary>
    Default = None,

    /// <summary>
    /// None of <see cref="SessionOptions"/>.
    /// Value is <see langword="0x0"/>.
    /// </summary>
    None = 0x0,

    /// <summary>
    /// Session allows automatic transaction.
    /// If transactions is required (i.e. for transactional method),
    /// but there is no current transaction, transaction is automatically opened.
    /// If this option is not specified and
    /// transactional method is called without active transaction an exception is thrown.
    /// Value is <see langword="0x1"/>.
    /// </summary>
    AutoTransactions = 0x1,

    /// <summary>
    /// Session uses ambient transactions.
    /// This option implicitly enables <see cref="AutoTransactions"/>.
    /// This mode must be normally used for UI sessions.
    /// Value is <see langword="0x3" />.
    /// </summary>
    AmbientTransactions = 0x3,

    /// <summary>
    /// Transactions will actually be opened just before execution of DB command. 
    /// This option is ignored fo our in-memory provider.
    /// Value is <see langword="0x4" />.
    /// </summary>
    AutoShortenTransactions = 0x4
  }
}
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
    /// Default options is <see cref="None"/>.
    /// </summary>
    Default = None,

    /// <summary>
    /// None of <see cref="SessionOptions"/>.
    /// Value is <see langword="0x0"/>.
    /// </summary>
    None = 0x0,

    /// <summary>
    /// Session uses ambient transactions.
    /// This mode must be normally used for UI sessions.
    /// Value is <see langword="0x3" />.
    /// </summary>
    AmbientTransactions = 0x3,

    /// <summary>
    /// Transactions will actually be opened just before execution of DB command. 
    /// This option is ignored for non-SQL providers.
    /// Value is <see langword="0x4" />.
    /// </summary>
    AutoShortenTransactions = 0x4,

    /// <summary>
    /// Enables activation of this <see cref="Session"/> from another session having this option.
    /// </summary>
    /// <remarks>
    /// <para>
    /// By default, activation of one session from another leads to exception, 
    /// since normally this indicates the same thread controls two sessions (and transactions),
    /// which is dangerous (may lead to application-level deadlock).
    /// </para>
    /// <para>
    /// Alternatively, this might indicate unintentional usage of data fetched by 
    /// one session inside another.
    /// </para>
    /// <para>
    /// So to activate one session from another, you must use either <see cref="Session.Deactivate"/>
    /// method or this option.
    /// </para>
    /// </remarks>
    AllowSwitching = 0x8,
  }
}
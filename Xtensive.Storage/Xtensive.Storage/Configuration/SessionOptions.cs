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
    /// Transactions will actually be opened just before execution of DB command. 
    /// This option is ignored for non-SQL providers.
    /// Value is <see langword="0x8" />.
    /// </summary>
    AutoShortenTransactions = 0x8,

    /// <summary>
    /// Enables activation of this <see cref="Session"/> from another session having this option.
    /// </summary>
    /// <remarks>
    /// <para>
    /// By default, activation of a session inside another one with running transaction 
    /// (i.e. when another session is active, and transaction is already running there)
    /// leads to <see cref="InvalidOperationException"/>, 
    /// since normally this indicates the same thread controls two sessions and transactions,
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
    /// <para>
    /// See <see href="http://support.x-tensive.com/question/2870/nested-sessions-and-transactions">description of 
    /// this feature on Support@x-tensive.com</see> for further details and examples.
    /// </para>
    /// </remarks>
    AllowSwitching = 0x10,

    /// <summary>
    /// Enables automatic activation of session on all public members of <see cref="ISessionBound"/> implementors 
    /// (e.g. <see cref="Entity"/>, <see cref="Structure"/> and their inheritors).
    /// </summary>
    AutoActivation = 0x20,

    /// <summary>
    /// <see cref="DisconnectedState"/> will be created and attached in the session constructor.
    /// </summary>
    Disconnected = 0x40,

    /// <summary>
    /// Consider <see cref="TransactionalBehavior.Auto"/> option as <see cref="TransactionalBehavior.Suppress"/> when processing <see cref="TransactionalAttribute"/>.
    /// </summary>
    AutoTransactionSuppressMode = 0x80,

    /// <summary>
    /// Consider <see cref="TransactionalBehavior.Auto"/> option as <see cref="TransactionalBehavior.Open"/> when processing <see cref="TransactionalAttribute"/>.
    /// </summary>
    AutoTransactionOpenMode = 0x100,

    /// <summary>
    /// Predefined option set for client-side sessions (WPF, Windows Forms, console applications, etc.).
    /// Combines <see cref="AutoShortenTransactions"/> | <see cref="AutoTransactionOpenMode"/> | <see cref="Disconnected"/> flags.
    /// </summary>
    ClientProfile = AutoShortenTransactions | AutoTransactionOpenMode | Disconnected,

    /// <summary>
    /// Predefined option set for server-side sessions (ASP.NET, ASP.NET MVC, services, etc.).
    /// Combines  <see cref="AutoActivation"/> | <see cref="AutoShortenTransactions"/> flags.
    /// </summary>
    ServerProfile = AutoShortenTransactions | AutoActivation,

    /// <summary>
    /// Predefined option set for compatibility with previous versions of DataObjects.Net (4.3.* and earlier).
    /// Combines <see cref="AutoTransactionOpenMode"/> | <see cref="AutoActivation"/> | <see cref="AutoShortenTransactions"/> flags.
    /// </summary>
    LegacyProfile = AutoShortenTransactions | AutoTransactionOpenMode | AutoActivation
  }
}
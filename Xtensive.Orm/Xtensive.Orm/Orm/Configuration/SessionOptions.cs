// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2008.10.06

using System;

namespace Xtensive.Orm.Configuration
{
  /// <summary>
  /// Enumerates possible options of the <see cref="Session"/>.
  /// </summary>
  [Flags]
  public enum SessionOptions
  {
    /// <summary>
    /// Default option set.
    /// The same as <see cref="ServerProfile"/>.
    /// </summary>
    Default = ServerProfile,

    /// <summary>
    /// None of <see cref="SessionOptions"/> is on.
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
    /// as <see cref="TransactionalBehavior.Suppress"/> when processing <see cref="TransactionalAttribute"/>.
    /// </summary>
    AutoTransactionSuppressMode = 0x80,

    /// <summary>
    /// Consider <see cref="TransactionalBehavior.Auto"/> option 
    /// </summary>
    AutoTransactionOpenMode = 0x100,

    /// <summary>
    /// Enables reading of fields of removed objects.
    /// By default this leads no an exception - only <see cref="Entity.Key"/>, <see cref="Entity.TypeId"/> and
    /// few other system properties of removed objects can be accessed.
    /// This option allows to read all the properties of removed objects, which values are available.
    /// </summary>
    ReadRemovedObjects = 0x200,

    // Profiles

    /// <summary>
    /// Predefined option set for server-side sessions (ASP.NET, ASP.NET MVC, services, etc.).
    /// Includes only
    /// <see cref="AutoShortenTransactions"/> flag.
    /// </summary>
    ServerProfile = AutoShortenTransactions,

    /// <summary>
    /// Combines 
    /// <see cref="AutoShortenTransactions"/> | 
    /// <see cref="AutoTransactionOpenMode"/> | 
    /// <see cref="AllowSwitching"/> | 
    /// <see cref="Disconnected"/> flags.
    /// </summary>
    ClientProfile = AutoShortenTransactions | AutoTransactionOpenMode | AllowSwitching | Disconnected,

    /// <summary>
    /// Predefined option set for compatibility with previous versions of DataObjects.Net (4.3.* and earlier).
    /// Combines 
    /// <see cref="AutoTransactionOpenMode"/> | 
    /// <see cref="AutoActivation"/> | 
    /// <see cref="AutoShortenTransactions"/> flags.
    /// </summary>
    LegacyProfile = AutoShortenTransactions | AutoTransactionOpenMode | AutoActivation,
  }
}
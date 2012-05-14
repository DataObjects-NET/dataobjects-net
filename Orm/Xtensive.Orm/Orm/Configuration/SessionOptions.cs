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
    /// None of <see cref="SessionOptions"/> is on.
    /// Value is <see langword="0x0"/>.
    /// </summary>
    None = 0x0,

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
    AllowSwitching = 1 << 4,

    /// <summary>
    /// Enables automatic activation of session on all public members of <see cref="ISessionBound"/> implementors 
    /// (e.g. <see cref="Entity"/>, <see cref="Structure"/> and their inheritors).
    /// </summary>
    AutoActivation = 1 << 5,

    /// <summary>
    /// <see cref="DisconnectedState"/> will be created and attached in the session constructor.
    /// </summary>
    Disconnected = 1 << 6,

    /// <summary>
    /// Enables <see cref="TransactionalBehavior.Suppress"/> for automatic transactions.
    /// </summary>
    AutoTransactionSuppressMode = 1 << 7,

    /// <summary>
    /// Enables <see cref="TransactionalBehavior.Auto"/> for automatic transactions.
    /// </summary>
    AutoTransactionOpenMode = 1 << 8,

    /// <summary>
    /// Enables reading of fields of removed objects.
    /// By default this leads no an exception - only <see cref="Entity.Key"/>, <see cref="Entity.TypeId"/> and
    /// few other system properties of removed objects can be accessed.
    /// This option allows to read all the properties of removed objects, which values are available.
    /// </summary>
    ReadRemovedObjects = 1 << 9,

    /// <summary>
    /// Enables suppression of any exception occured during transaction rollback.
    /// This option is useful if exception hiding occurs due to exceptions in <see cref="TransactionScope.Dispose"/>.
    /// Any exception thrown in <see cref="SessionEventAccessor.TransactionRollbacking"/> event
    /// will not be affected by this option.
    /// </summary>
    SuppressRollbackExceptions = 1 << 11,

    // Profiles

    /// <summary>
    /// Predefined option set for server-side sessions (ASP.NET, ASP.NET MVC, services, etc.).
    /// Includes only <see cref="None"/> flag.
    /// </summary>
    ServerProfile = None | (1 << 10),

    /// <summary>
    /// Predefined option set for client-side sessions (WPF, Windows Forms, console applications, etc.).
    /// Combines 
    /// <see cref="AutoTransactionOpenMode"/> | 
    /// <see cref="Disconnected"/> flags.
    /// </summary>
    ClientProfile = AutoTransactionOpenMode | Disconnected,

    /// <summary>
    /// Predefined option set for compatibility with previous versions of DataObjects.Net (4.3.* and earlier).
    /// Combines 
    /// <see cref="AutoTransactionOpenMode"/> | 
    /// <see cref="AutoActivation"/> flags.
    /// </summary>
    LegacyProfile = AutoTransactionOpenMode | AutoActivation,

    /// <summary>
    /// Default option set.
    /// The same as <see cref="ServerProfile"/>.
    /// </summary>
    Default = ServerProfile,
  }
}
// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2008.10.06

using System;
using JetBrains.Annotations;

namespace Xtensive.Orm.Configuration
{
  /// <summary>
  /// Enumerates possible options of the <see cref="Session"/>.
  /// </summary>
  [Flags, UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
  public enum SessionOptions
  {
    /// <summary>
    /// None of <see cref="SessionOptions"/> is on.
    /// Value is <see langword="0x0"/>.
    /// </summary>
    None = 0,

    /// <summary>
    /// Enables reading of <see cref="Entity"/> objects without active transaction.
    /// This option changes <see cref="Session"/> behavior in two ways.
    /// It becames possible to execute queries without any active transaction.
    /// Entities loaded or modified in a transaction that is already committed don't refetch their data.
    /// </summary>
    NonTransactionalReads = 1 << 0,

    // Not used:
    // 1 << 1
    // 1 << 2
    // 1 << 3

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
    /// Enables reading of fields of removed objects.
    /// By default this leads no an exception - only <see cref="Entity.Key"/>, <see cref="Entity.TypeId"/> and
    /// few other system properties of removed objects can be accessed.
    /// This option allows to read all the properties of removed objects, which values are available.
    /// </summary>
    ReadRemovedObjects = 1 << 9,

    // Server profile:
    // 1 << 10

    /// <summary>
    /// Enables suppression of any exception occurred during transaction rollback.
    /// This option is useful if exception hiding occurs due to exceptions in <see cref="TransactionScope.Dispose"/>.
    /// Any exception thrown in <see cref="SessionEventAccessor.TransactionRollbacking"/> event
    /// will not be affected by this option.
    /// </summary>
    SuppressRollbackExceptions = 1 << 11,

    /// <summary>
    /// Enables validation of entity versions during save to the database.
    /// This option disables any SQL statement batching.
    /// </summary>
    ValidateEntityVersions = 1 << 12,

    /// <summary>
    /// Enables validation framework in this session.
    /// </summary>
    ValidateEntities = 1 << 13,

    /// <summary>
    /// Enables generation of <see cref="Key"/>s for <see cref="Entity">Entities</see>
    /// just before saving to storage.
    /// </summary>
    LazyKeyGeneration = 1 << 14,

    /// <summary>
    /// Enables automatic persist of changes in case of committing of transaction, query and some others.
    /// </summary>
    AutoSaveChanges = 1 << 15,

    /// <summary>
    /// Enables reading and saving of <see cref="Entity"/> objects without active transaction.
    /// Contains
    /// <see cref="NonTransactionalReads"/>
    /// </summary>
    NonTransactionalEntityStates = (1 << 16) | NonTransactionalReads,

    // Profiles

    /// <summary>
    /// Predefined option set for server-side sessions (ASP.NET, ASP.NET MVC, services, etc.).
    /// Includes only <see cref="ValidateEntities"/> flag.
    /// </summary>
    ServerProfile = ValidateEntities | AutoSaveChanges | (1 << 10),

    /// <summary>
    /// Predefined option set for client-side sessions (WPF, Windows Forms, console applications, etc.).
    /// Combines 
    /// <see cref="NonTransactionalEntityStates"/> | 
    /// <see cref="LazyKeyGeneration"/> |
    /// <see cref="ValidateEntities"/> flags.
    /// </summary>
    ClientProfile = NonTransactionalEntityStates | LazyKeyGeneration | ValidateEntities,

    /// <summary>
    /// Predefined option set for compatibility with previous versions of DataObjects.Net (4.3.* and earlier).
    /// Combines  
    /// <see cref="AutoActivation"/> |
    /// <see cref="ValidateEntities"/> flags.
    /// </summary>
    LegacyProfile =  ValidateEntities | AutoActivation,

    /// <summary>
    /// Default option set.
    /// The same as <see cref="ServerProfile"/>.
    /// </summary>
    Default = ServerProfile,
  }
}
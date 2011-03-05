// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.12.11

namespace Xtensive.Orm.Internals
{
  /// <summary>
  /// A reason of calling <see cref="Session.Persist(Xtensive.Orm.Internals.PersistReason)"/>.
  /// </summary>
  internal enum PersistReason
  {
    /// <summary>
    /// Manual persist is requested.
    /// </summary>
    Manual,
    /// <summary>
    /// Query is to be executed.
    /// </summary>
    Query,
    /// <summary>
    /// Nested transaction is about to start.
    /// </summary>
    NestedTransaction,
    /// <summary>
    /// Commit is to be performed.
    /// </summary>
    Commit,
    /// <summary>
    /// <see cref="Session.EntityChangeRegistry"/> has reached its size limit.
    /// </summary>
    ChangeRegistrySizeLimit,
    /// <summary>
    /// <see cref="VersionValidator"/> is about to be disposed.
    /// </summary>
    ValidateVersions,
    /// <summary>
    /// <see cref="Session"/> is about to remap its keys.
    /// </summary>
    RemapEntityKeys,
    /// <summary>
    /// It's necessary to flush entity removal.
    /// </summary>
    PersistEntityRemoval,
    /// <summary>
    /// <see cref="DisconnectedState"/> is about to be attached to <see cref="Session"/>.
    /// </summary>
    DisconnectedStateAttach,
    /// <summary>
    /// <see cref="DisconnectedState"/> is about to be merged with another one.
    /// </summary>
    DisconnectedStateMerge,
    /// <summary>
    /// <see cref="DisconnectedState"/> is about to run local refeernce-related query.
    /// </summary>
    DisconnectedStateReferenceCacheLookup,
    /// <summary>
    /// <see cref="DisconnectedState"/> is about to be enumerated.
    /// </summary>
    DisconnectedStateEnumeration,
    /// <summary>
    /// Another persist reason.
    /// </summary>
    Other,
  }
}
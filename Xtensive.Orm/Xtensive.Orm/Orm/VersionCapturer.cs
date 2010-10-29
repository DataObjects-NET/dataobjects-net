// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2010.03.01

using System;
using System.Collections.Generic;
using Xtensive.Aspects;
using Xtensive.Core;
using Xtensive.Internals.DocTemplates;
using Xtensive.Tuples.Transform;
using System.Linq;

namespace Xtensive.Orm
{
  /// <summary>
  /// A service listening to entity change-related events in <see cref="Session"/>
  /// and writing the information on their original version to <see cref="Versions"/> set
  /// (<see cref="VersionSet"/>).
  /// </summary>
  [Infrastructure]
  public sealed class VersionCapturer : SessionBound, 
    IDisposable
  {
    private readonly VersionSet materializedVersions = new VersionSet();
    private readonly VersionSet modifiedVersions = new VersionSet();
    private readonly List<Key> removedKeys = new List<Key>();

    /// <summary>
    /// Gets the version set updated by this service.
    /// </summary>
    public VersionSet Versions { get; private set; }

    #region Session event handlers

    private void EntityMaterialized(object sender, EntityEventArgs e)
    {
      materializedVersions.Add(e.Entity, true);
    }

    private void TransactionRollbacked(object sender, TransactionEventArgs transactionEventArgs)
    {
      removedKeys.Clear();
      modifiedVersions.Clear();
    }

    private void TransactionCommitted(object sender, TransactionEventArgs transactionEventArgs)
    {
      if (transactionEventArgs.Transaction.IsNested)
        return;
      Versions.MergeWith(materializedVersions, Session);
      Versions.MergeWith(modifiedVersions, Session);
      foreach (var key in removedKeys)
        Versions.Remove(key);
      materializedVersions.Clear();
      modifiedVersions.Clear();
      removedKeys.Clear();
    }

    private void TransactionOpened(object sender, TransactionEventArgs transactionEventArgs)
    {
      if (transactionEventArgs.Transaction.IsNested)
        return;
      Versions.MergeWith(materializedVersions, Session);
      materializedVersions.Clear();
    }

    private void Persisting(object sender, EventArgs eventArgs)
    {
      var registry = Session.EntityChangeRegistry;
      var modifiedStates = registry.GetItems(PersistenceState.Modified)
        .Concat(registry.GetItems(PersistenceState.New));
      foreach (var state in modifiedStates) {
        var versionTuple = state.Type.VersionExtractor.Apply(TupleTransformType.Tuple, state.Tuple);
        modifiedVersions.Add(state.Key, new VersionInfo(versionTuple), true);
      }
      removedKeys.AddRange(registry.GetItems(PersistenceState.Removed).Select(s => s.Key));
    }

    #endregion

    #region Private methods

    private void AttachEventHandlers()
    {
      Session.SystemEvents.TransactionOpened += TransactionOpened;
      Session.SystemEvents.TransactionCommitted +=TransactionCommitted;
      Session.SystemEvents.TransactionRollbacked += TransactionRollbacked;
      Session.SystemEvents.EntityMaterialized += EntityMaterialized;
      Session.SystemEvents.Persisting += Persisting;
    }

    private void DetachEventHandlers()
    {
      Session.SystemEvents.TransactionOpened -= TransactionOpened;
      Session.SystemEvents.TransactionCommitted -= TransactionCommitted;
      Session.SystemEvents.TransactionRollbacked -= TransactionRollbacked;
      Session.SystemEvents.EntityMaterialized -= EntityMaterialized;
      Session.SystemEvents.Persisting -= Persisting;
    }

    #endregion

    // Factory methods

    /// <summary>
    /// Attaches the version capturer to the current session.
    /// </summary>
    /// <param name="versions">The <see cref="VersionSet"/> to append captured versions to.</param>
    /// <returns>
    /// A newly created <see cref="VersionCapturer"/> attached
    /// to the current session.
    /// </returns>
    public static VersionCapturer Attach(VersionSet versions)
    {
      return Attach(Session.Demand(), versions);
    }

    /// <summary>
    /// Attaches the version capturer to the current session.
    /// </summary>
    /// <param name="session">The session to attach the capturer to.</param>
    /// <param name="versions">The <see cref="VersionSet"/> to append captured versions to.</param>
    /// <returns>
    /// A newly created <see cref="VersionCapturer"/> attached
    /// to the specified <paramref name="session"/>.
    /// </returns>
    public static VersionCapturer Attach(Session session, VersionSet versions)
    {
      return new VersionCapturer(session, versions);
    }


    // Constructors

    private VersionCapturer(Session session, VersionSet versions)
      : base(session)
    {
      ArgumentValidator.EnsureArgumentNotNull(versions, "versions");
      Versions = versions;
      AttachEventHandlers();
    }

    // Dispose
    
    /// <see cref="DisposableDocTemplate.Dispose()" copy="true"/>
    public void Dispose()
    {
      DetachEventHandlers();
    }
  }
}
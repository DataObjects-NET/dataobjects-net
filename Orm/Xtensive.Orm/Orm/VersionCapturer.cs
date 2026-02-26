// Copyright (C) 2010-2021 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Alex Yakunin
// Created:    2010.03.01

using System;
using System.Collections.Generic;
using Xtensive.Core;

using Xtensive.Tuples.Transform;
using System.Linq;

namespace Xtensive.Orm
{
  /// <summary>
  /// A service listening to entity change-related events in <see cref="Session"/>
  /// and writing the information on their original version to <see cref="Versions"/> set
  /// (<see cref="VersionSet"/>).
  /// </summary>
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
      //two foreach operators are faster than one concat because of bare hashset enumerator
      foreach(var state in registry.GetItems(PersistenceState.Modified)) {
        AddModifiedVersion(state);
      }
      foreach (var state in registry.GetItems(PersistenceState.New)) {
        AddModifiedVersion(state);
      }
       
      removedKeys.AddRange(registry.GetItems(PersistenceState.Removed).Select(s => s.Key));

      void AddModifiedVersion(EntityState state)
      {
        var versionTuple = state.Type.VersionExtractor.Apply(TupleTransformType.Tuple, state.Tuple);
        _ = modifiedVersions.Add(state.Key, new VersionInfo(versionTuple), true);
      }
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
      ArgumentNullException.ThrowIfNull(versions);
      Versions = versions;
      AttachEventHandlers();
    }

    // Dispose

    /// <summary>
    /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
    /// </summary>
    public void Dispose()
    {
      DetachEventHandlers();
    }
  }
}
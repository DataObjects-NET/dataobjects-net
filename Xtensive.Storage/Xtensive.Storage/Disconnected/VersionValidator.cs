// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Ivan Galkin
// Created:    2009.11.10

using System;
using System.Collections.Generic;
using Xtensive.Core;
using Xtensive.Core.Internals.DocTemplates;
using System.Linq;
using Xtensive.Core.Tuples;
using Xtensive.Core.Tuples.Transform;
using Xtensive.Storage.Internals;
using Xtensive.Storage.Model;
using Xtensive.Storage.Rse;

namespace Xtensive.Storage.Disconnected
{
  /// <summary>
  /// Validate versions.
  /// </summary>
  public class VersionValidator : IDisposable
  {
    private HashSet<Key> checkedKeys = new HashSet<Key>();
    private Dictionary<Key, VersionInfo> actualVersionCache;
    private readonly Func<Key, VersionInfo> versionGetter;
    private Dictionary<Key, QueryTask> readVersionTasks;
    private Dictionary<Key, VersionInfo> versionsToCheck;

    /// <summary>
    /// Gets the session.
    /// </summary>
    protected Session Session { get; private set; }
    
    /// <summary>
    /// Gets the stored version.
    /// </summary>
    /// <param name="key">The instance key.</param>
    /// <returns>Stored version.</returns>
    protected virtual VersionInfo GetStoredVersion(Key key)
    {
      return versionGetter.Invoke(key);
    }

    /// <summary>
    /// Called when entity changing.
    /// </summary>
    /// <param name="entity">The changed entity.</param>
    protected virtual void OnEntityChanging(Entity entity)
    {
      if (entity.PersistenceState==PersistenceState.New)
        return;
      if (entity.Type.VersionExtractor==null)
        return;
      if (entity.State.IsStale)
        return;
      if (actualVersionCache.ContainsKey(entity.Key))
        return;
      if (checkedKeys.Contains(entity.Key))
        return;
      actualVersionCache.Add(entity.Key, entity.GetVersion());
    }

    /// <summary>
    /// Called when transaction opening.
    /// </summary>
    protected virtual void OnTransactionOpen()
    {
    }

    /// <summary>
    /// Registers specified entity to check version.
    /// </summary>
    /// <param name="entity">The entity.</param>
    protected virtual void RegisterToCheck(Entity entity)
    {
      if (entity.Type.VersionExtractor==null
        || versionsToCheck.ContainsKey(entity.Key))
        return;
      VersionInfo version;
      if (actualVersionCache.TryGetValue(entity.Key, out version))
        versionsToCheck.Add(entity.Key, version);
      else {
        var queryTask = CreateReadVersionTask(entity.Key);
        Session.RegisterDelayedQuery(queryTask);
        readVersionTasks.Add(entity.Key, queryTask);
      }
    }

    /// <summary>
    /// Checks the version.
    /// </summary>
    /// <param name="key">The key.</param>
    /// <param name="actualVersion">The actual version.</param>
    protected virtual void CheckVersion(Key key, VersionInfo actualVersion)
    {
      var storedVersion = GetStoredVersion(key);
      if (storedVersion.IsVoid)
        return;
      if (storedVersion!=actualVersion || actualVersion.IsVoid)
        throw new InvalidOperationException(string.Format(
          "Version of entity with key '{0}' is not actual.", key));
    }

    /// <summary>
    /// Determines whether entity with specified key is already checked.
    /// </summary>
    /// <param name="key">The key.</param>
    /// <returns>
    /// <see langword="true"/> if entity with specified key is already checked; otherwise, <see langword="false"/>.
    /// </returns>
    protected bool IsChecked(Key key)
    {
      return checkedKeys.Contains(key);
    }
    
    # region Event handlers

    private void OnEntityRemoving(object sender, EntityEventArgs e)
    {
      OnEntityChanging(e.Entity);
    }

    private void OnEntitySetItemRemoving(object sender, EntitySetItemEventArgs e)
    {
      OnEntityChanging(e.EntitySet.Owner);
    }

    private void OnEntitySetItemAdding(object sender, EntitySetItemEventArgs e)
    {
      OnEntityChanging(e.EntitySet.Owner);
    }

    private void OnEntityFieldValueSetting(object sender, FieldValueEventArgs e)
    {
      OnEntityChanging(e.Entity);
    }

    private void OnTransactionOpen(object sender, TransactionEventArgs e)
    {
      checkedKeys = new HashSet<Key>();
      actualVersionCache = new Dictionary<Key, VersionInfo>();
      OnTransactionOpen();
    }

    private void OnPersisting(object sender, EventArgs e)
    {
      versionsToCheck = new Dictionary<Key, VersionInfo>();
      readVersionTasks = new Dictionary<Key, QueryTask>();
      RegisterCheckVersionTasks();
      if (readVersionTasks.Count > 0)
        Session.ExecuteAllDelayedQueries(true);
    }

    private void OnPersisted(object sender, EventArgs e)
    {
      if (readVersionTasks.Count > 0)
        foreach (var task in readVersionTasks) {
          var key = task.Key;
          var version = ReadVersion(task.Key.Type, task.Value.Result.FirstOrDefault());
          versionsToCheck.Add(key, version);
        }
      foreach (var pair in versionsToCheck)
        CheckVersion(pair.Key, pair.Value);
    }

    # endregion

    private void Attach()
    {
      Session.Persisting += OnPersisting;
      Session.Persisted += OnPersisted;
      Session.TransactionOpen += OnTransactionOpen;
      Session.EntityFieldValueSetting += OnEntityFieldValueSetting;
      Session.EntitySetItemAdding += OnEntitySetItemAdding;
      Session.EntitySetItemRemoving += OnEntitySetItemRemoving;
      Session.EntityRemoving += OnEntityRemoving;
    }

    private void Detach()
    {
      Session.Persisting -= OnPersisting;
      Session.Persisted -= OnPersisted;
      Session.TransactionOpen -= OnTransactionOpen;
      Session.EntityFieldValueSetting -= OnEntityFieldValueSetting;
      Session.EntitySetItemAdding -= OnEntitySetItemAdding;
      Session.EntitySetItemRemoving -= OnEntitySetItemRemoving;
      Session.EntityRemoving -= OnEntityRemoving;
    }

    private void RegisterCheckVersionTasks()
    {
      var registry = Session.EntityChangeRegistry;
      foreach (var item in registry.GetItems(PersistenceState.New))
        checkedKeys.Add(item.Key);
      foreach (var item in registry.GetItems(PersistenceState.Modified)) {
        RegisterToCheck(item.Entity);
        checkedKeys.Add(item.Key);
      }
      foreach (var item in registry.GetItems(PersistenceState.Removed)) {
        RegisterToCheck(item.Entity);
        checkedKeys.Add(item.Key);
      }

      /*
      var changedItem =
        registry.GetItems(PersistenceState.Modified)
          .Concat(registry.GetItems(PersistenceState.Removed))
          .Where(item => item.Type.VersionExtractor!=null
            && !checkedKeys.Contains(item.Key));
      foreach (var item in changedItem) {
        if (versionsToCheck.ContainsKey(item.Key)
          || readVersionTasks.ContainsKey(item.Key))
          continue;
        VersionInfo version;
        if (actualVersions.TryGetValue(item.Key, out version))
          versionsToCheck.Add(item.Key, version);
        else {
          var queryTask = CreateReadVersionTask(item.Key);
          Session.RegisterDelayedQuery(queryTask);
          readVersionTasks.Add(item.Key, queryTask);
        }
      }
      */
    }

    private static VersionInfo ReadVersion(TypeInfo type, Tuple state)
    {
      if (state==null)
        return new VersionInfo();
      var versionTuple = type.VersionExtractor.Apply(TupleTransformType.Tuple, state);
      return new VersionInfo(versionTuple);
    }

    private QueryTask CreateReadVersionTask(Key key)
    {
      var type = key.Type;
      var provider = type.Indexes.PrimaryIndex.ToRecordSet().Seek(key.Value).Provider;
      var execProvider = Session.CompilationContext.Compile(provider);
      return new QueryTask(execProvider, null);
    }

    /// <inheritdoc/>
    public void Dispose()
    {
      if (Session!=null) {
        Detach();
        Session = null;
      }
    }


    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="session">The session.</param>
    /// <param name="versionGetter">The stored version getter.</param>
    public VersionValidator(Session session, Func<Key, VersionInfo> versionGetter)
    {
      ArgumentValidator.EnsureArgumentNotNull(session, "session");
      ArgumentValidator.EnsureArgumentNotNull(versionGetter, "versionGetter");
      if (session.IsPersisting)
        throw new InvalidOperationException();

      Session = session;
      this.versionGetter = versionGetter;
      Attach();
    }
  }
}

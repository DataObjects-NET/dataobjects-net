// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Ivan Galkin
// Created:    2009.11.10

using System;
using System.Collections.Generic;
using System.Linq;
using Xtensive.Aspects;
using Xtensive.Core;
using Xtensive.Diagnostics;
using Xtensive.Internals.DocTemplates;
using Xtensive.Tuples;
using Tuple = Xtensive.Tuples.Tuple;
using Xtensive.Tuples.Transform;
using Xtensive.Orm.Internals;
using Xtensive.Orm.Model;
using Xtensive.Orm.Resources;
using Xtensive.Storage.Rse;

namespace Xtensive.Orm
{
  /// <summary>
  /// An attachable service validating versions inside the specified <see cref="Session"/>.
  /// </summary>
  [Infrastructure]
  public sealed class VersionValidator : SessionBound, 
    IDisposable
  {
    private HashSet<Key> processed = new HashSet<Key>();
    private Dictionary<Key, VersionInfo> knownVersions;
    private Dictionary<Key, VersionInfo> queuedVersions;
    private Dictionary<Key, QueryTask> fetchVersionTasks;
    private readonly Func<Key, VersionInfo> expectedVersionProvider;
    private bool isAttached;

    /// <summary>
    /// Validates the <paramref name="version"/>
    /// for the specified <paramref name="key"/>.
    /// </summary>
    /// <param name="key">The key to validate version for.</param>
    /// <param name="version">The version to validate.</param>
    /// <returns>
    /// <see langword="True"/>, if validation passes successfully;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    public bool ValidateVersion(Key key, VersionInfo version)
    {
      var expectedVersion = expectedVersionProvider.Invoke(key);
      return expectedVersion.IsVoid || expectedVersion == version;
    }

    /// <summary>
    /// Validates the <paramref name="version"/>
    /// for the specified <paramref name="key"/>.
    /// </summary>
    /// <param name="key">The key to validate version for.</param>
    /// <param name="version">The version to validate.</param>
    /// <param name="throwOnFailure">Indicates whether <see cref="InvalidOperationException"/>
    /// must be thrown on validation failure.</param>
    /// <returns>
    /// <see langword="True"/>, if validation passes successfully;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    /// <exception cref="VersionConflictException">Version conflict is detected.</exception>
    public bool ValidateVersion(Key key, VersionInfo version, bool throwOnFailure)
    {
      var result = ValidateVersion(key, version);
      if (throwOnFailure && !result) {
        if (Log.IsLogged(LogEventTypes.Info))
          Log.Info(Strings.LogSessionXVersionValidationFailedKeyYVersionZExpected3,
            Session, key, version, expectedVersionProvider.Invoke(key));
        throw new VersionConflictException(string.Format(
          Strings.ExVersionOfEntityWithKeyXDiffersFromTheExpectedOne, key));
      }
      return result;
    }

    #region Validator logic

    private void Initialize()
    {
      knownVersions = new Dictionary<Key, VersionInfo>();
      processed = new HashSet<Key>();
    }

    private void FetchVersions()
    {
      queuedVersions = new Dictionary<Key, VersionInfo>();
      fetchVersionTasks = new Dictionary<Key, QueryTask>();

      var registry = Session.EntityChangeRegistry;
      foreach (var item in registry.GetItems(PersistenceState.New))
        processed.Add(item.Key);
      foreach (var item in registry.GetItems(PersistenceState.Modified)) {
        EnqueueVersionValidation(item);
        processed.Add(item.Key);
      }
      foreach (var item in registry.GetItems(PersistenceState.Removed)) {
        EnqueueVersionValidation(item);
        processed.Add(item.Key);
      }
      if (fetchVersionTasks.Count > 0)
        Session.Handler.ExecuteQueryTasks(fetchVersionTasks.Values, true);
    }

    private void FetchLeftVersions()
    {
      queuedVersions = new Dictionary<Key, VersionInfo>();
      fetchVersionTasks = new Dictionary<Key, QueryTask>();

      foreach (var pair in knownVersions) {
        EnqueueVersionValidation(pair.Key);
        processed.Add(pair.Key);
      }
      if (fetchVersionTasks.Count > 0)
        Session.Handler.ExecuteQueryTasks(fetchVersionTasks.Values, true);
    }

    private void EnqueueVersionValidation(EntityState state)
    {
      if (state.Type.VersionExtractor==null
          || queuedVersions.ContainsKey(state.Key)
          || processed.Contains(state.Key))
        return;
      VersionInfo version;
      if (knownVersions.TryGetValue(state.Key, out version))
        queuedVersions.Add(state.Key, version);
      else {
        var task = CreateFetchVersionTask(state.Key);
        Session.RegisterDelayedQuery(task);
        fetchVersionTasks.Add(state.Key, task);
      }
    }

    private void EnqueueVersionValidation(Key key)
    {
      if (processed.Contains(key))
        return;
      var task = CreateFetchVersionTask(key);
      Session.RegisterDelayedQuery(task);
      fetchVersionTasks.Add(key, task);
    }

    private QueryTask CreateFetchVersionTask(Key key)
    {
      var type = key.Type;
      var provider = type.Indexes.PrimaryIndex.ToRecordQuery().Seek(key.Value).Provider;
      var execProvider = Session.CompilationService.Compile(provider);
      return new QueryTask(execProvider, null);
    }

    private void ValidateFetchedVersions()
    {
      Session.ExecuteDelayedQueries(true);
      if (fetchVersionTasks.Count > 0)
        foreach (var task in fetchVersionTasks) {
          var key = task.Key;
          var version = ExtractVersion(task.Key.Type, task.Value.Result.FirstOrDefault());
          queuedVersions.Add(key, version);
        }
      foreach (var pair in queuedVersions)
        ValidateVersion(pair.Key, pair.Value, true);
    }

    private static VersionInfo ExtractVersion(TypeInfo type, Tuple state)
    {
      if (state==null)
        return VersionInfo.Void;
      var versionTuple = type.VersionExtractor.Apply(TupleTransformType.Tuple, state);
      return new VersionInfo(versionTuple);
    }

    private void DropFetchVersionsData()
    {
      queuedVersions = null;
      fetchVersionTasks = null;
    }

    #endregion

    #region Event handlers

    private void OnTransactionOpened(object sender, TransactionEventArgs e)
    {
      if (e.Transaction.IsNested)
        return;
      Initialize();
    }

    private void OnTransactionCommitting(object sender, TransactionEventArgs e)
    {
      if (e.Transaction.IsNested)
        return;
      Session.Persist(PersistReason.ValidateVersions);
      try {
        FetchLeftVersions();
        ValidateFetchedVersions();
      }
      finally {
        DropFetchVersionsData();
      }
    }

    private void OnTransactionClosed(object sender, TransactionEventArgs e)
    {
      if (e.Transaction.IsNested)
        return;
      processed = null;
      knownVersions = null;
    }

    private void OnEntityVersionInfoChanging(object sender, EntityVersionInfoChangedEventArgs e)
    {
      OnEntityChanging(e.Entity);
    }

    private void OnEntityRemoving(object sender, EntityEventArgs e)
    {
      OnEntityChanging(e.Entity);
    }

    private void OnEntityChanging(Entity entity)
    {
      // "return" here means "we can't rely on Entity.VersionInfo,
      // and so it must be fetched on Session.Persist, or
      // there is nothing to validate"
      if (entity.PersistenceState==PersistenceState.New)
        return;
      if (entity.TypeInfo.VersionExtractor==null)
        return;
      if (entity.State.IsStale && !entity.TypeInfo.HasVersionRoots)
        return;
      // Here we know the actual version is stored in VersionInfo
      if (knownVersions.ContainsKey(entity.Key))
        return;
      if (processed.Contains(entity.Key))
        return;
      knownVersions.Add(entity.Key, entity.VersionInfo);
    }

    private void OnPersisting(object sender, EventArgs e)
    {
      FetchVersions();
    }

    private void OnPersisted(object sender, EventArgs e)
    {
      try {
        ValidateFetchedVersions();
      }
      finally {
        DropFetchVersionsData();
      }
    }

    #endregion

    #region AttachEventHandlers \ DetachEventHandlers methods

    private void AttachEventHandlers()
    {
      if (isAttached)
        throw new InvalidOperationException(Strings.ExTheServiceIsAlreadyAttachedToSession);
      isAttached = true;
      try {
        Session.SystemEvents.TransactionOpened += OnTransactionOpened;
        Session.SystemEvents.TransactionCommitting += OnTransactionCommitting;
        Session.SystemEvents.TransactionCommitted += OnTransactionClosed;
        Session.SystemEvents.TransactionRollbacked += OnTransactionClosed;
        Session.SystemEvents.EntityVersionInfoChanging += OnEntityVersionInfoChanging;
        Session.SystemEvents.EntityRemoving += OnEntityRemoving;
        Session.SystemEvents.Persisting += OnPersisting;
        Session.SystemEvents.Persisted += OnPersisted;
      }
      catch {
        DetachEventHandlers();
        throw;
      }
    }

    private void DetachEventHandlers()
    {
      if (isAttached) {
        isAttached = false;
        Session.SystemEvents.TransactionOpened -= OnTransactionOpened;
        Session.SystemEvents.TransactionCommitting -= OnTransactionCommitting;
        Session.SystemEvents.TransactionCommitted -= OnTransactionClosed;
        Session.SystemEvents.TransactionRollbacked -= OnTransactionClosed;
        Session.SystemEvents.EntityVersionInfoChanging -= OnEntityVersionInfoChanging;
        Session.SystemEvents.EntityRemoving -= OnEntityRemoving;
        Session.SystemEvents.Persisting -= OnPersisting;
        Session.SystemEvents.Persisted -= OnPersisted;
      }
    }

    #endregion

    #region Attach methods (factory methods)

    /// <summary>
    /// Attaches the validator to the current session.
    /// </summary>
    /// <param name="expectedVersions">The set containing expected versions.</param>
    /// <returns>
    /// A newly created <see cref="VersionValidator"/> attached
    /// to the current session.
    /// </returns>
    public static VersionValidator Attach(VersionSet expectedVersions)
    {
      return Attach(Session.Demand(), expectedVersions);
    }

    /// <summary>
    /// Attaches the validator to the specified session.
    /// </summary>
    /// <param name="session">The session to attach validator to.</param>
    /// <param name="expectedVersions">The set containing expected versions.</param>
    /// <returns>
    /// A newly created <see cref="VersionValidator"/> attached
    /// to the specified <paramref name="session"/>.
    /// </returns>
    public static VersionValidator Attach(Session session, VersionSet expectedVersions)
    {
      return new VersionValidator(session, expectedVersions.Get);
    }

    /// <summary>
    /// Attaches the validator to the current session.
    /// </summary>
    /// <param name="expectedVersionProvider">The expected version provider.</param>
    /// <returns>A newly created <see cref="VersionValidator"/> attached
    /// to the current session.</returns>
    public static VersionValidator Attach(Func<Key, VersionInfo> expectedVersionProvider)
    {
      return Attach(Session.Demand(), expectedVersionProvider);
    }

    /// <summary>
    /// Attaches the validator to the specified session.
    /// </summary>
    /// <param name="session">The session to attach validator to.</param>
    /// <param name="expectedVersionProvider">The expected version provider.</param>
    /// <returns>A newly created <see cref="VersionValidator"/> attached
    /// to the specified <paramref name="session"/>.</returns>
    public static VersionValidator Attach(Session session, Func<Key, VersionInfo> expectedVersionProvider)
    {
      return new VersionValidator(session, expectedVersionProvider);
    }

    #endregion


    // Constructors

    /// <exception cref="InvalidOperationException">Session is persisting the changes.</exception>
    private VersionValidator(Session session, Func<Key, VersionInfo> expectedVersionProvider)
      : base(session)
    {
      ArgumentValidator.EnsureArgumentNotNull(expectedVersionProvider, "expectedVersionProvider");
      if (session.IsPersisting)
        throw new InvalidOperationException(
          Strings.ExServiceCanNotBeAttachedToSessionWhileItIsPersistingTheChanges);

      this.expectedVersionProvider = expectedVersionProvider;
      AttachEventHandlers();
      if (session.Transaction!=null)
        Initialize();
    }

    // Dispose
    
    /// <see cref="DisposableDocTemplate.Dispose()" copy="true"/>
    public void Dispose()
    {
      try {
        if (Session.Transaction!=null) {
          Session.Persist(PersistReason.ValidateVersions);
          FetchLeftVersions();
          ValidateFetchedVersions();
        }
      }
      finally {
        DropFetchVersionsData();
        DetachEventHandlers();
      }
    }
  }
}

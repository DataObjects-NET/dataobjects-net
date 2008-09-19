// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2007.08.10

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Xtensive.Core;
using Xtensive.Core.Collections;
using Xtensive.Core.Diagnostics;
using Xtensive.Core.Disposable;
using Xtensive.Core.Internals.DocTemplates;
using Xtensive.Integrity.Atomicity;
using Xtensive.Storage.Configuration;
using Xtensive.Storage.Internals;
using Xtensive.Storage.Model;
using Xtensive.Storage.Providers;
using Xtensive.Storage.Rse;
using Xtensive.Storage.Resources;
using Xtensive.Storage.Rse.Compilation;

namespace Xtensive.Storage
{
  /// <summary>
  /// Session.
  /// </summary>
  public class Session : DomainBound,
    IContext<SessionScope>,
    IResource
  {
    private bool isPersisting;
    private volatile bool isDisposed;
    private readonly Set<object> consumers = new Set<object>();
    private readonly object _lock = new object();
    private readonly CompilationScope compilationScope;    

    internal readonly List<EntityData> newEntities = new List<EntityData>();
    internal readonly List<EntityData> modifiedEntities = new List<EntityData>();
    internal readonly List<EntityData> removedEntities = new List<EntityData>();    

    /// <summary>
    /// Gets the configuration of the <see cref="Session"/>.
    /// </summary>
    public SessionConfiguration Configuration { get; private set; }

    /// <summary>
    /// Gets the name.
    /// </summary>
    public string Name { get; private set; }

    /// <summary>
    /// Gets the atomicity context.
    /// </summary>
    public AtomicityContext AtomicityContext { get; private set; }

    /// <summary>
    /// Gets the current validation context.
    /// </summary>
    /// <exception cref="InvalidOperationException">Can not get validation context: There is no active transaction.</exception>
    public ValidationContext ValidationContext
    {
      get {
        if (Transaction==null)
          throw new InvalidOperationException(Strings.ExCanNotGetValidationContextThereIsNoActiveTransaction);
        return Transaction.ValidationContext;
      }
    }

    /// <summary>
    /// Gets the active transaction.
    /// </summary>    
    public Transaction Transaction { get; private set; }

    #region Private \ internal properties

    internal HandlerAccessor Handlers { get; private set; }

    internal SessionHandler Handler { get; private set; }

    internal EntityCache Cache { get; private set; }

    #endregion

    /// <summary>
    /// Opens a new or already running transaction.
    /// </summary>
    /// <returns>
    /// A new <see cref="TransactionScope"/> object, if new
    /// <see cref="Transaction"/> is created;
    /// otherwise, <see langword="null"/>.
    /// </returns>
    public TransactionScope OpenTransaction()
    {
      if (Transaction==null) {
        Transaction = new Transaction(this);
        return (TransactionScope) Transaction.Begin();
      }
      return null;
    }

    /// <summary>
    /// Opens the "inconsistent region" - the code region, in which changed entities
    /// should just queue the validation rather then perform it immediately.
    /// </summary>
    /// <returns></returns>
    public IDisposable OpenInconsistentRegion()
    {
      return ValidationContext.OpenInconsistentRegion();
    }

    /// <summary>
    /// Persists all modified instances immediately.
    /// </summary>
    /// <remarks>
    /// This method should be called to ensure that all delayed
    /// updates are flushed to the storage. 
    /// Note, that this method is called automatically when it's necessary,
    /// e.g. before beginning\committing\rolling back a transaction,
    /// establishing a save point or rolling back to it, performing a
    /// query and so further. So generally you should not worry
    /// about calling this method.
    /// </remarks>
    /// <exception cref="ObjectDisposedException">Session is already disposed.</exception>
    public void Persist()
    {
      if (isPersisting)
        return;
      isPersisting = true;
      try {
        EnsureNotDisposed();

        if (Log.IsLogged(LogEventTypes.Debug))
          Log.Debug("Session '{0}'. Persisting.", this);

        Handler.Persist();

        ClearDirtyData();
      }
      finally {
        isPersisting = false;
      }
    }

    private void ClearDirtyData()
    {
      foreach (EntityData data in newEntities)
        data.PersistenceState = PersistenceState.Synchronized;

      foreach (EntityData data in modifiedEntities)
        data.PersistenceState = PersistenceState.Synchronized;

      foreach (EntityData data in removedEntities)
        data.PersistenceState = PersistenceState.Synchronized;      

      newEntities.Clear();
      modifiedEntities.Clear();
      removedEntities.Clear();
    }

    public IEnumerable<T> All<T>() 
      where T : class, IEntity
    {      
      EnsureNotDisposed();
      Persist();

      TypeInfo type = Domain.Model.Types[typeof (T)];
      RecordSet result = type.Indexes.PrimaryIndex.ToRecordSet();
      foreach (T entity in result.ToEntities<T>())
        yield return entity;
    }

    #region OnXxx methods

    internal void OnTransctionBegin()
    {
      Handler.BeginTransaction();
    }

    internal void OnTransactionCommit()
    {
      try {
        Persist();
        Handler.CommitTransaction();
        Cache.ClearRemoved();
        OnTranscationEnd();
      }
      catch {        
        OnTransactionRollback();
        throw;
      }
    }

    internal void OnTransactionRollback()
    {
      try {
        Handler.RollbackTransaction();
      }
      finally {
        ClearDirtyData();
        Cache.RestoreRemoved();
        OnTranscationEnd();
      }
    }

    private void OnTranscationEnd()
    {
      Transaction = null;
    }

    #endregion

    #region IResource methods

    /// <inheritdoc/>
    void IResource.AddConsumer(object consumer)
    {
      consumers.Add(consumer);
    }

    /// <inheritdoc/>
    void IResource.RemoveConsumer(object consumer)
    {
      consumers.Remove(consumer);
      if (!(this as IResource).HasConsumers)
        Dispose();
    }

    /// <inheritdoc/>
    bool IResource.HasConsumers
    {
      get { return consumers.Count > 0; }
    }

    #endregion

    #region IContext<...> methods

    /// <summary>
    /// Gets the current active <see cref="Session"/> instance.
    /// </summary>
    public static Session Current {
      [DebuggerStepThrough]
      get { return SessionScope.CurrentSession; }
    }

    /// <inheritdoc/>
    public SessionScope Activate()
    {
      if (IsActive)
        return null;      
      return new SessionScope(this, null);      
    }

    /// <inheritdoc/>
    IDisposable IContext.Activate()
    {
      return Activate();
    }

    /// <inheritdoc/>
    public bool IsActive {
      get { return Current==this; }
    }

    #endregion

    #region EnsureXxx methods

    /// <summary>
    /// Ensures the session is not disposed.
    /// </summary>
    /// <exception cref="ObjectDisposedException">Session is already disposed.</exception>
    protected void EnsureNotDisposed()
    {
      if (isDisposed)
        throw new ObjectDisposedException(Strings.SessionIsAlreadyDisposed);
    }

    #endregion

    /// <inheritdoc/>
    public override string ToString()
    {
      return Name;
    }


    // Constructors

    internal Session(Domain domain, SessionConfiguration configuration)
      : base(domain)
    {
      // Both Domain and Configuration are valid references here;
      // Configuration is already locked
      Configuration = configuration;
      Handlers = domain.Handlers;
      Handler = Handlers.HandlerFactory.CreateHandler<SessionHandler>();
      Cache = new EntityCache(this, Configuration.CacheSize);
      Name = configuration.Name;
      Handler.Session = this;
      Handler.Initialize();
      compilationScope = Handlers.DomainHandler.CompilationContext.Activate();
      AtomicityContext = new AtomicityContext(this, AtomicityContextOptions.Undoable);
    }

    #region Dispose pattern

    /// <summary>
    /// <see cref="ClassDocTemplate.Dispose" copy="true"/>
    /// </summary>
    public void Dispose()
    {
      Dispose(true);
      GC.SuppressFinalize(this);
    }

    /// <summary>
    /// <see cref="ClassDocTemplate.Dtor" copy="true"/>
    /// </summary>
    ~Session()
    {
      Dispose(false);
    }

    /// <summary>
    /// <see cref="ClassDocTemplate.Dispose" copy="true"/>
    /// </summary>
    protected virtual void Dispose(bool disposing)
    {
      if (isDisposed)
        return;
      lock (_lock) {
        if (isDisposed)
          return;
        try {
          if (Log.IsLogged(LogEventTypes.Debug))
            Log.Debug("Session '{0}'. Disposing", this);          
          Handler.DisposeSafely();
          compilationScope.DisposeSafely();
        }
        finally {
          isDisposed = true;
        }
      }
    }

    #endregion
  }
}
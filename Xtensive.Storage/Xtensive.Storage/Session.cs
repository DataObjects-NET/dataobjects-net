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
using Xtensive.Integrity.Transactions;
using Xtensive.Storage.Configuration;
using Xtensive.Storage.Internals;
using Xtensive.Storage.Model;
using Xtensive.Storage.Providers;
using Xtensive.Storage.Rse;
using Xtensive.Storage.Resources;
using Xtensive.Storage.Rse.Compilation;

namespace Xtensive.Storage
{
  public class Session : DomainBound,
    IContext<SessionScope>,
    IResource
  {
    private volatile bool isDisposed;
    private readonly Set<object> consumers = new Set<object>();
    private readonly object _lock = new object();
    private readonly CompilationScope compilationScope;


    /// <summary>
    /// Gets the atomicity context.
    /// </summary>
    public AtomicityContext AtomicityContext { get; private set; }

    #region Transaction related methods

    /// <summary>
    /// Gets the active transaction.
    /// </summary>    
    public Transaction Transaction { get; private set; }

    /// <summary>
    /// Opens new transaction, if there is no active one.
    /// </summary>
    /// <returns>Scope of the active transaction.</returns>
    public TransactionScope BeginTransaction()
    {
      if (Transaction==null) {
        Transaction = new Transaction(this);
        Handler.BeginTransaction();
        return Transaction.Activate();
      }

      return null;
    }

    internal void OnTransactionCommit()
    {
      Persist();
      Handler.CommitTransaction();
      OnTranscationFinished();
    }

    internal void OnTransactionRollback()
    {
      foreach (EntityData data in DirtyData.GetItems(PersistenceState.New))
        data.Entity.PersistenceState = PersistenceState.Inconsistent;

      foreach (EntityData data in DirtyData.GetItems(PersistenceState.Modified))
        data.Entity.PersistenceState = PersistenceState.Persisted;

      foreach (EntityData data in DirtyData.GetItems(PersistenceState.Removed))
        data.Entity.PersistenceState = PersistenceState.Persisted;

      DirtyData.Clear();

      Handler.RollbackTransaction();
      OnTranscationFinished();
    }

    private void OnTranscationFinished()
    {
      Transaction = null;
      DataCache.Reset();
    }

    #endregion

    #region Private \ internal properties

    internal HandlerAccessor Handlers { get; private set; }

    internal SessionHandler Handler { get; private set; }

    internal EntityDataCache DataCache { get; private set; }

    internal FlagRegistry<PersistenceState, EntityData> DirtyData { get; private set; }

    #endregion

    /// <summary>
    /// Gets the configuration of the <see cref="Session"/>.
    /// </summary>
    public SessionConfiguration Configuration { get; private set; }

    /// <summary>
    /// Gets the name.
    /// </summary>
    public string Name { get; private set; }

    /// <summary>
    /// Persists all modified instances immediately.
    /// </summary>
    /// <remarks>
    /// This method should be called to ensure that all delayed
    /// updates are flushed to the storage. Note, that this method 
    /// is called automatically when it's necessary - e.g.
    /// before beginning\committing\rolling back a transaction,
    /// establishing a save point or rolling back to it, performing a
    /// query and so further. So generally you should not worry
    /// about calling this method.
    /// </remarks>
    /// <exception cref="ObjectDisposedException">Session is already disposed.</exception>
    public void Persist()
    {
      EnsureNotDisposed();
      if (DirtyData.Count==0)
        return;
      
      if (Log.IsLogged(LogEventTypes.Debug))
        Log.Debug("Session '{0}'. Persisting dirty data: {1}", this, DirtyData);

      Handler.Persist(DirtyData);

      HashSet<EntityData> @new = DirtyData.GetItems(PersistenceState.New);
      HashSet<EntityData> modified = DirtyData.GetItems(PersistenceState.Modified);
      HashSet<EntityData> removed = DirtyData.GetItems(PersistenceState.Removed);

      foreach (EntityData data in @new.Union(modified).Except(removed))
        data.PersistenceState = PersistenceState.Persisted;

      foreach (EntityData data in removed)
        DataCache.Remove(data.Key);

      DirtyData.Clear();
    }

    public IEnumerable<T> All<T>() where T : class,   IEntity
    {      
      EnsureNotDisposed();
      Persist();

      TypeInfo type = Domain.Model.Types[typeof (T)];
      RecordSet result = type.Indexes.PrimaryIndex.ToRecordSet();
      foreach (T entity in result.ToEntities<T>())
        yield return entity;
    }

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
      else
        return new SessionScope(this);
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
      DataCache = new EntityDataCache(this, Configuration.CacheSize);
      DirtyData = new FlagRegistry<PersistenceState, EntityData>(e => e.PersistenceState);
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
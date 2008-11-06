// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2007.08.10

using System;
using System.Collections.Generic;
using System.Diagnostics;
using Xtensive.Core;
using Xtensive.Core.Aspects;
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

    internal EntityStateRegistry EntityStateRegistry { get; private set; }

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

    internal LowLevelServiceMap LowLevelServices { get; private set; }

    /// <summary>
    /// Gets the current validation context.
    /// </summary>
    /// <exception cref="InvalidOperationException">Can not get validation context: There is no active transaction.</exception>
    public ValidationContext ValidationContext {
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

    /// <summary>
    /// Gets the ambient transaction scope.
    /// </summary>    
    public TransactionScope AmbientTransactionScope { get; private set; }

    /// <summary>
    /// Indicates whether debug event logging is enabled.
    /// Caches <see cref="Log.IsLogged"/> method result for <see cref="LogEventTypes.Debug"/> event.
    /// </summary>
    internal bool IsDebugEventLoggingEnabled { get; private set; }

    #region Private \ internal properties

    internal HandlerAccessor Handlers { get; private set; }

    internal SessionHandler Handler { get; private set; }

    [Infrastructure]
    internal SessionCache Cache { get; private set; }

    #endregion

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

    /// <summary>
    /// Persists all modified instances immediately.
    /// </summary>
    /// <remarks>
    /// This method should be called to ensure that all delayed
    /// updates are flushed to the storage. 
    /// Note, that this method is called automatically when it's necessary,
    /// e.g. before beginning\committing\rolling back a transaction, performing a
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

        if (IsDebugEventLoggingEnabled)
          Log.Debug("Session '{0}'. Persisting...", this);

        Handler.Persist();

        if (IsDebugEventLoggingEnabled)
          Log.Debug("Session '{0}'. Persisted.", this);

        EntityStateRegistry.Clear();
      }
      finally {
        isPersisting = false;
      }
    }

    #region OpenXxx methods

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
      var transaction = Transaction;
      if (transaction==null) {
        transaction = new Transaction(this);
        Transaction = transaction;
        var ts = (TransactionScope) transaction.Begin();
        if (ts!=null && Configuration.UsesAmbientTransactions) {
          AmbientTransactionScope = ts;
          return null;
        }
        return ts;
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

    #endregion

    #region Commit & RollbackAmbientTransaction methods

    /// <summary>
    /// Commits the ambient transaction - 
    /// i.e. completes <see cref="AmbientTransactionScope"/> and disposes it.
    /// </summary>
    public void CommitAmbientTransaction()
    {
      var ts = AmbientTransactionScope;
      try {
        ts.Complete();
      }
      finally {
        AmbientTransactionScope = null;
        ts.DisposeSafely();
      }
    }

    /// <summary>
    /// Rolls back the ambient transaction - 
    /// i.e. disposes <see cref="AmbientTransactionScope"/>.
    /// </summary>
    public void RollbackAmbientTransaction()
    {
      var ts = AmbientTransactionScope;
      AmbientTransactionScope = null;
      ts.DisposeSafely();
    }

    #endregion

    #region OnXxx methods

    internal void OnBeginTransaction()
    {
      Handler.BeginTransaction();
    }

    internal void OnCommitTransaction()
    {
      try {
        Persist();
        Handler.CommitTransaction();
        OnCompleteTransaction();
      }
      catch {        
        OnRollbackTransaction();
        throw;
      }
    }

    internal void OnRollbackTransaction()
    {
      try {
        Handler.RollbackTransaction();
      }
      finally {
        EntityStateRegistry.Clear();
        OnCompleteTransaction();
      }
    }

    private void OnCompleteTransaction()
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
      IsDebugEventLoggingEnabled = Log.IsLogged(LogEventTypes.Debug); // Just to cache this value
      // Both Domain and Configuration are valid references here;
      // Configuration is already locked
      Configuration = configuration;
      Handlers = domain.Handlers;
      Handler = Handlers.HandlerFactory.CreateHandler<SessionHandler>();
      Cache = new SessionCache(this, configuration.CacheSize);
      Name = configuration.Name;
      Handler.Session = this;
      Handler.Initialize();
      AtomicityContext = new AtomicityContext(this, AtomicityContextOptions.Undoable);
      compilationScope = Handlers.DomainHandler.CompilationContext.Activate();
      LowLevelServices = new LowLevelServiceMap(this);
      EntityStateRegistry = new EntityStateRegistry(this);
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
          if (IsDebugEventLoggingEnabled)
            Log.Debug("Session '{0}'. Disposing.", this);          
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
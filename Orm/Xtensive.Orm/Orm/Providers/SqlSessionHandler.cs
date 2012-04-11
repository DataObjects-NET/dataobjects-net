// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2008.05.20

using System;
using System.Collections.Generic;
using System.Linq;
using Xtensive.IoC;
using Xtensive.Orm.Internals;
using Xtensive.Orm.Internals.Prefetch;
using Xtensive.Parameters;
using Xtensive.Sql;
using Tuple = Xtensive.Tuples.Tuple;

namespace Xtensive.Orm.Providers
{
  /// <summary>
  /// <see cref="Session"/>-level handler for SQL storages.
  /// </summary>
  public partial class SqlSessionHandler : SessionHandler,
    IProviderExecutor,
    IDirectSqlService
  {
    private readonly StorageDriver driver;
    private readonly DomainHandler domainHandler;
    private readonly SqlConnection connection;
    private readonly CommandProcessor commandProcessor;

    private bool isDisposed;

    /// <inheritdoc/>
    public override bool TransactionIsStarted { get { return connection.ActiveTransaction!=null; } }

    /// <summary>
    /// Gets <see cref="SqlConnection"/> associated with current instance.
    /// </summary>
    public SqlConnection Connection
    {
      get
      {
        EnsureConnectionIsOpen();
        return connection;
      }
    }

    /// <summary>
    /// Gets <see cref="CommandFactory"/> associated with current instance.
    /// </summary>
    public CommandFactory CommandFactory
    {
      get
      {
        EnsureConnectionIsOpen();
        return commandProcessor.Factory;
      }
    }

    /// <inheritdoc/>
    public override void SetCommandTimeout(int? commandTimeout)
    {
      connection.CommandTimeout = commandTimeout;
    }

    #region Transaction control methods

    /// <inheritdoc/>
    public override void BeginTransaction(Transaction transaction)
    {
      EnsureConnectionIsOpen();
      driver.BeginTransaction(
        Session, connection, IsolationLevelConverter.Convert(transaction.IsolationLevel));
    }

    /// <inheritdoc/>
    public override void CreateSavepoint(Transaction transaction)
    {
      EnsureConnectionIsOpen();
      driver.MakeSavepoint(Session, connection, transaction.SavepointName);
    }

    /// <inheritdoc/>
    public override void RollbackToSavepoint(Transaction transaction)
    {
      EnsureConnectionIsOpen();
      driver.RollbackToSavepoint(Session, connection, transaction.SavepointName);
    }

    /// <inheritdoc/>
    public override void ReleaseSavepoint(Transaction transaction)
    {
      EnsureConnectionIsOpen();
      driver.ReleaseSavepoint(Session, connection, transaction.SavepointName);
    }

    /// <inheritdoc/>
    public override void CommitTransaction(Transaction transaction)
    {
      if (connection.ActiveTransaction!=null)
        driver.CommitTransaction(Session, connection);
    }

    /// <inheritdoc/>
    public override void RollbackTransaction(Transaction transaction)
    {
      if (connection.ActiveTransaction!=null)
        driver.RollbackTransaction(Session, connection);
    }

    public override void CompletingTransaction(Transaction transaction)
    {
      prefetchManager.CancelTasks();
      commandProcessor.ClearTasks();
    }

    #endregion
    
    #region Private / internal members

    private void RegisterQueryTask(QueryTask task, QueryRequest request)
    {
      task.Result = new List<Tuple>();
      commandProcessor.RegisterTask(new SqlLoadTask(request, task.Result, task.ParameterContext));
    }

    private void EnsureConnectionIsOpen()
    {
      driver.EnsureConnectionIsOpen(Session, connection);
    }

    #endregion

    /// <inheritdoc/>
    public override void ExecuteQueryTasks(IEnumerable<QueryTask> queryTasks, bool allowPartialExecution)
    {
      EnsureConnectionIsOpen();

      var nonBatchedTasks = new List<QueryTask>();
      foreach (var task in queryTasks) {
        var sqlProvider = task.DataSource as SqlProvider;
        if (sqlProvider!=null && sqlProvider.Request.CheckOptions(QueryRequestOptions.AllowOptimization))
          RegisterQueryTask(task, sqlProvider.Request);
        else
          nonBatchedTasks.Add(task);
      }

      if (nonBatchedTasks.Count==0) {
        commandProcessor.ExecuteTasks(allowPartialExecution);
        return;
      }

      commandProcessor.ExecuteTasks();
      foreach (var task in nonBatchedTasks) {
        using (new EnumerationContext(Session).Activate())
        using (task.ParameterContext.ActivateSafely())
          task.Result = task.DataSource.ToList();
      }
    }

    /// <inheritdoc/>
    public override void AddSystemServices(ICollection<ServiceRegistration> r)
    {
      r.Add(new ServiceRegistration(typeof (ISqlExecutor), new SqlExecutor(driver, connection, Session)));
      r.Add(new ServiceRegistration(typeof (IDirectSqlService), this));
      r.Add(new ServiceRegistration(typeof (IProviderExecutor), this));
    }

    /// <inheritdoc/>
    public override void Persist(EntityChangeRegistry registry, bool allowPartialExecution)
    {
      domainHandler.Persister.Persist(registry, commandProcessor);
      commandProcessor.ExecuteTasks(allowPartialExecution);
    }

    /// <inheritdoc/>
    public override void Dispose()
    {
      if (isDisposed)
        return;
      isDisposed = true;
      driver.CloseConnection(Session, connection);
    }

    public SqlSessionHandler(Session session, SqlConnection connection)
      : base(session)
    {
      this.connection = connection;

      domainHandler = Handlers.DomainHandler;
      driver = Handlers.StorageDriver;

      commandProcessor = domainHandler.CommandProcessorFactory.CreateCommandProcessor(Session, connection);
      prefetchManager = new PrefetchManager(Session);
    }
  }
}

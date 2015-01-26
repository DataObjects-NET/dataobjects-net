// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2008.05.20

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xtensive.Core;
using Xtensive.IoC;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Internals;
using Xtensive.Orm.Internals.Prefetch;
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
    private readonly bool transactionIsExternal;
    private readonly bool connectionIsExternal;
    private readonly CommandProcessor commandProcessor;
    private readonly List<string> initializationSqlScripts = new List<string>();

    private Transaction pendingTransaction;
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
        Prepare();
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
        Prepare();
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
      pendingTransaction = transaction;

      if (Session.Configuration.Type!=SessionType.User)
        Prepare();
    }

    /// <inheritdoc/>
    public override void CommitTransaction(Transaction transaction)
    {
      pendingTransaction = null;
      if (connection.ActiveTransaction!=null && !transactionIsExternal)
        driver.CommitTransaction(Session, connection);
      if (!connectionIsExternal)
        driver.CloseConnection(Session, connection);
    }

    /// <inheritdoc/>
    public override void RollbackTransaction(Transaction transaction)
    {
      pendingTransaction = null;
      if (connection.ActiveTransaction!=null && !transactionIsExternal)
        driver.RollbackTransaction(Session, connection);
      if (!connectionIsExternal)
        driver.CloseConnection(Session, connection);
    }

    /// <inheritdoc/>
    public override void CreateSavepoint(Transaction transaction)
    {
      Prepare();
      driver.MakeSavepoint(Session, connection, transaction.SavepointName);
    }

    /// <inheritdoc/>
    public override void RollbackToSavepoint(Transaction transaction)
    {
      Prepare();
      driver.RollbackToSavepoint(Session, connection, transaction.SavepointName);
    }

    /// <inheritdoc/>
    public override void ReleaseSavepoint(Transaction transaction)
    {
      Prepare();
      driver.ReleaseSavepoint(Session, connection, transaction.SavepointName);
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

    private void Prepare()
    {
      Session.EnsureNotDisposed();
      driver.EnsureConnectionIsOpen(Session, connection);
      foreach (var script in initializationSqlScripts)
        using (var command = connection.CreateCommand(script))
          driver.ExecuteNonQuery(Session, command);
      initializationSqlScripts.Clear();
      if (pendingTransaction==null)
        return;
      var transaction = pendingTransaction;
      pendingTransaction = null;
      if (connection.ActiveTransaction==null) // Handle external transactions
        driver.BeginTransaction(Session, connection, IsolationLevelConverter.Convert(transaction.IsolationLevel));
    }

    #endregion

    /// <inheritdoc/>
    public override void ExecuteQueryTasks(IEnumerable<QueryTask> queryTasks, ExecutionBehavior behavior)
    {
      Prepare();
      var queryTasksList = queryTasks.ToList();
      var nonBatchedTasks = new List<QueryTask>();
#if NET45
      Session.AsyncQueriesManager.SetDelayedTaskToStarted(queryTasksList);
#endif
      foreach (var task in queryTasksList) {
        var sqlProvider = task.DataSource as SqlProvider;
        if (sqlProvider!=null && sqlProvider.Request.CheckOptions(QueryRequestOptions.AllowOptimization))
          RegisterQueryTask(task, sqlProvider.Request);
        else
          nonBatchedTasks.Add(task);
      }
      try {
        if (nonBatchedTasks.Count==0) {
          commandProcessor.ExecuteTasks(behavior);
#if NET45
          Session.AsyncQueriesManager.SetDelayedTasksToCompleted(queryTasksList);
#endif
          return;
        }

        commandProcessor.ExecuteTasks();
        foreach (var task in nonBatchedTasks) {
          using (new EnumerationContext(Session).Activate())
          using (task.ParameterContext.ActivateSafely())
            task.Result = task.DataSource.ToList();
        }
#if NET45
        Session.AsyncQueriesManager.SetDelayedTasksToCompleted(queryTasksList);
#endif
      }
      catch (Exception exception) {
#if NET45
        Session.AsyncQueriesManager.SetDelayedTasksToFault(queryTasksList, exception);
#endif
        throw;
      }
    }

#if NET45
    public override async Task ExecuteQueryTasksAsync(IEnumerable<QueryTask> queryTasks, ExecutionBehavior behavior, CancellationToken token)
    {
      Prepare();
      var queryTasksList = queryTasks.ToList();
      var nonBatchedTasks = new List<QueryTask>();

      Session.AsyncQueriesManager.SetDelayedTaskToStarted(queryTasksList);
      foreach (var task in queryTasksList) {
        var sqlProvider = task.DataSource as SqlProvider;
        if (sqlProvider != null && sqlProvider.Request.CheckOptions(QueryRequestOptions.AllowOptimization))
          RegisterQueryTask(task, sqlProvider.Request);
        else
          nonBatchedTasks.Add(task);
      }

      try {
        if (nonBatchedTasks.Count==0) {
          await commandProcessor.ExecuteTasksAsync(ExecutionBehavior.PartialExecutionIsNotAllowed, token)
            .ContinueWith((task, tuple) => {
                            var state = (Tuple<Session, IList<QueryTask>>) tuple;
                            state.Item1.AsyncQueriesManager.SetDelayedTasksToCompleted(state.Item2);
                          }, new Tuple<Session, IList<QueryTask>>(Session, queryTasksList), token);
          return;
        }

        foreach (var task in nonBatchedTasks) {
          using (new EnumerationContext(Session).Activate())
          using (task.ParameterContext.ActivateSafely())
            task.Result = task.DataSource.ToList();
        }
        Session.AsyncQueriesManager.SetDelayedTasksToCompleted(queryTasksList);
      }
      catch (Exception exception) {
        Session.AsyncQueriesManager.SetDelayedTasksToFault(queryTasksList, exception);
        throw;
      }
    }
#endif

    /// <inheritdoc/>
    [Obsolete("Use SqlSessionHandler.ExecuteQueryTasks(IEnumerable<QueryTask>, ExecutionBehavior) instead.")]
    public override void ExecuteQueryTasks(IEnumerable<QueryTask> queryTasks, bool allowPartialExecution)
    {
      ExecuteQueryTasks(queryTasks, ConvertToTaskExecutionBehavior(allowPartialExecution));
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
      Prepare();
      domainHandler.Persister.Persist(registry, commandProcessor);
      commandProcessor.ExecuteTasks(ConvertToTaskExecutionBehavior(allowPartialExecution));
    }

    /// <inheritdoc/>
    public override void Dispose()
    {
      if (isDisposed)
        return;
      isDisposed = true;
      if (!connectionIsExternal) {
        driver.CloseConnection(Session, connection);
        driver.DisposeConnection(Session, connection);
      }
    }

    internal override void SetStorageNode(StorageNode node)
    {
      if (!connectionIsExternal)
        driver.ApplyNodeConfiguration(connection, node.Configuration);
    }

    internal SqlSessionHandler(Session session, SqlConnection connection, bool connectionIsExternal, bool transactionIsExternal)
      : base(session)
    {
      this.connection = connection;
      this.connectionIsExternal = connectionIsExternal;
      this.transactionIsExternal = transactionIsExternal;

      domainHandler = Handlers.DomainHandler;
      driver = Handlers.StorageDriver;

      commandProcessor = domainHandler.CommandProcessorFactory.CreateCommandProcessor(Session, connection);
      prefetchManager = new PrefetchManager(Session);
    }
  }
}

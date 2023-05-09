// Copyright (C) 2008-2021 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Alexey Gamzov
// Created:    2008.05.20

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
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

      if (Session.Configuration.Type != SessionType.User) {
        Prepare();
      }
    }

    /// <inheritdoc/>
    public override async Task BeginTransactionAsync(Transaction transaction, CancellationToken ct)
    {
      pendingTransaction = transaction;

      if (Session.Configuration.Type != SessionType.User) {
        await PrepareAsync(ct).ConfigureAwait(false);
      }
    }

    /// <inheritdoc/>
    public override void CommitTransaction(Transaction transaction)
    {
      pendingTransaction = null;
      if (connection.ActiveTransaction != null && !transactionIsExternal) {
        driver.CommitTransaction(Session, connection);
      }

      if (!connectionIsExternal) {
        driver.CloseConnection(Session, connection);
      }
    }

    /// <inheritdoc/>
    public override async ValueTask CommitTransactionAsync(Transaction transaction)
    {
      pendingTransaction = null;
      if (connection.ActiveTransaction != null && !transactionIsExternal) {
        await driver.CommitTransactionAsync(Session, connection).ConfigureAwait(false);
      }

      if (!connectionIsExternal) {
        await driver.CloseConnectionAsync(Session, connection).ConfigureAwait(false);
      }
    }

    /// <inheritdoc/>
    public override void RollbackTransaction(Transaction transaction)
    {
      pendingTransaction = null;
      if (connection.ActiveTransaction != null && !transactionIsExternal) {
        driver.RollbackTransaction(Session, connection);
      }

      if (!connectionIsExternal) {
        driver.CloseConnection(Session, connection);
      }
    }

    /// <inheritdoc/>
    public override async ValueTask RollbackTransactionAsync(Transaction transaction)
    {
      pendingTransaction = null;
      if (connection.ActiveTransaction != null && !transactionIsExternal) {
        await driver.RollbackTransactionAsync(Session, connection).ConfigureAwait(false);
      }

      if (!connectionIsExternal) {
        await driver.CloseConnectionAsync(Session, connection).ConfigureAwait(false);
      }
    }

    /// <inheritdoc/>
    public override void CreateSavepoint(Transaction transaction)
    {
      Prepare();
      driver.MakeSavepoint(Session, connection, transaction.SavepointName);
    }

    /// <inheritdoc/>
    public override async ValueTask CreateSavepointAsync(Transaction transaction, CancellationToken token = default)
    {
      await PrepareAsync(token).ConfigureAwait(false);
      await driver.MakeSavepointAsync(Session, connection, transaction.SavepointName, token).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public override void RollbackToSavepoint(Transaction transaction)
    {
      Prepare();
      driver.RollbackToSavepoint(Session, connection, transaction.SavepointName);
    }

    /// <inheritdoc/>
    public override async ValueTask RollbackToSavepointAsync(Transaction transaction, CancellationToken token = default)
    {
      await PrepareAsync(token).ConfigureAwait(false);
      await driver.RollbackToSavepointAsync(Session, connection, transaction.SavepointName, token).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public override void ReleaseSavepoint(Transaction transaction)
    {
      Prepare();
      driver.ReleaseSavepoint(Session, connection, transaction.SavepointName);
    }

    /// <inheritdoc/>
    public override async ValueTask ReleaseSavepointAsync(Transaction transaction, CancellationToken token = default)
    {
      await PrepareAsync(token).ConfigureAwait(false);
      await driver.ReleaseSavepointAsync(Session, connection, transaction.SavepointName, token).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public override void CompletingTransaction(Transaction transaction)
    {
      prefetchManager.CancelTasks();
      commandProcessor.ClearTasks();
    }

    #endregion

    #region Private / internal members

    internal Task OpenConnectionAsync(CancellationToken cancellationToken)
    {
      return PrepareAsync(cancellationToken);
    }

    private void RegisterQueryTask(QueryTask task, QueryRequest request)
    {
      task.Result = new List<Tuple>();
      commandProcessor.RegisterTask(new SqlLoadTask(request, task.Result, task.ParameterContext));
    }

    private void Prepare()
    {
      Session.EnsureNotDisposed();
      driver.EnsureConnectionIsOpen(Session, connection);
      foreach (var script in initializationSqlScripts) {
        using (var command = connection.CreateCommand(script)) {
          _ = driver.ExecuteNonQuery(Session, command);
        }
      }

      initializationSqlScripts.Clear();
      if (pendingTransaction==null) {
        return;
      }

      var transaction = pendingTransaction;
      pendingTransaction = null;
      if (connection.ActiveTransaction==null) {
        // Handle external transactions
        driver.BeginTransaction(Session, connection, IsolationLevelConverter.Convert(transaction.IsolationLevel));
      }
    }

    private async Task PrepareAsync(CancellationToken cancellationToken)
    {
      Session.EnsureNotDisposed();
      await driver.EnsureConnectionIsOpenAsync(Session, connection, cancellationToken).ConfigureAwait(false);

      try {
        foreach (var initializationSqlScript in initializationSqlScripts) {
          var command = connection.CreateCommand(initializationSqlScript);
          await using var commandAwaiter = command.ConfigureAwait(false);
          await driver.ExecuteNonQueryAsync(Session, command, cancellationToken).ConfigureAwait(false);
        }
      }
      catch (OperationCanceledException) {
        await connection.CloseAsync().ConfigureAwait(false);
        throw;
      }

      if (pendingTransaction == null) {
        return;
      }

      var transaction = pendingTransaction;
      pendingTransaction = null;
      if (connection.ActiveTransaction == null) {
        // Handle external transactions
        var isolationLevel = IsolationLevelConverter.Convert(transaction.IsolationLevel);
        await driver.BeginTransactionAsync(Session, connection, isolationLevel, cancellationToken).ConfigureAwait(false);
      }
    }

    #endregion

    /// <inheritdoc/>
    public override void ExecuteQueryTasks(IEnumerable<QueryTask> queryTasks, bool allowPartialExecution)
    {
      Prepare();

      var nonBatchedTasks = new List<QueryTask>();
      foreach (var task in queryTasks) {
        if (task.DataSource is SqlProvider sqlProvider && sqlProvider.Request.CheckOptions(QueryRequestOptions.AllowOptimization)) {
          RegisterQueryTask(task, sqlProvider.Request);
        }
        else {
          nonBatchedTasks.Add(task);
        }
      }

      if (nonBatchedTasks.Count==0) {
          using (var context = Session.CommandProcessorContextProvider.ProvideContext(allowPartialExecution)) {
            commandProcessor.ExecuteTasks(context);
          }

          return;
      }

      using (var context = Session.CommandProcessorContextProvider.ProvideContext()) {
        commandProcessor.ExecuteTasks(context);
      }

      foreach (var task in nonBatchedTasks) {
        task.Result = task.DataSource.ToEnumerable(new EnumerationContext(Session, task.ParameterContext)).ToList();
      }
    }

    /// <inheritdoc/>
    public override async Task ExecuteQueryTasksAsync(IEnumerable<QueryTask> queryTasks, bool allowPartialExecution, CancellationToken token)
    {
      await PrepareAsync(token).ConfigureAwait(false);

      var nonBatchedTasks = new List<QueryTask>();
      foreach (var task in queryTasks) {
        if (task.DataSource is SqlProvider sqlProvider && sqlProvider.Request.CheckOptions(QueryRequestOptions.AllowOptimization)) {
          RegisterQueryTask(task, sqlProvider.Request);
        }
        else {
          nonBatchedTasks.Add(task);
        }
      }

      CommandProcessorContext context;
      if (nonBatchedTasks.Count==0) {
        context = Session.CommandProcessorContextProvider.ProvideContext(allowPartialExecution);
        await using var contextAwaiter = context.ConfigureAwait(false);
        await commandProcessor.ExecuteTasksAsync(context, token).ConfigureAwait(false);

        return;
      }

      context = Session.CommandProcessorContextProvider.ProvideContext();
      await using (context.ConfigureAwait(false)) {
        await commandProcessor.ExecuteTasksAsync(context, token).ConfigureAwait(false);
      }

      foreach (var task in nonBatchedTasks) {
        task.Result = task.DataSource.ToEnumerable(new EnumerationContext(Session, task.ParameterContext)).ToList();
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
      Prepare();
      domainHandler.Persister.Persist(registry, commandProcessor);

      using var context = Session.CommandProcessorContextProvider.ProvideContext(allowPartialExecution);
      commandProcessor.ExecuteTasks(context);
    }

    /// <inheritdoc/>
    public override async Task PersistAsync(EntityChangeRegistry registry, bool allowPartialExecution,
      CancellationToken token)
    {
      await PrepareAsync(token).ConfigureAwait(false);
      domainHandler.Persister.Persist(registry, commandProcessor);

      var context = Session.CommandProcessorContextProvider.ProvideContext(allowPartialExecution);
      await using var contextAwaiter = context.ConfigureAwait(false);
      await commandProcessor.ExecuteTasksAsync(context, token).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public override void Dispose()
    {
      if (isDisposed) {
        return;
      }

      isDisposed = true;
      if (!connectionIsExternal) {
        driver.CloseConnection(Session, connection);
        driver.DisposeConnection(Session, connection);
      }
    }

    /// <inheritdoc/>
    public override async ValueTask DisposeAsync()
    {
      if (isDisposed) {
        return;
      }

      isDisposed = true;
      if (!connectionIsExternal) {
        await driver.CloseConnectionAsync(Session, connection).ConfigureAwait(false);
        await driver.DisposeConnectionAsync(Session, connection).ConfigureAwait(false);
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

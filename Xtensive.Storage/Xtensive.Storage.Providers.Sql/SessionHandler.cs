// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2008.05.20

using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using Xtensive.Core.Tuples;
using Xtensive.Sql;
using Xtensive.Storage.Internals;
using Xtensive.Storage.Providers.Sql.Resources;

namespace Xtensive.Storage.Providers.Sql
{
  /// <summary>
  /// <see cref="Session"/>-level handler for SQL storages.
  /// </summary>
  public class SessionHandler : Providers.SessionHandler,
    IQueryExecutor
  {
    private Driver driver;
    private DomainHandler domainHandler;
    private SqlConnection connection;
    private CommandProcessor commandProcessor;

    /// <summary>
    /// Gets the connection.
    /// </summary>
    public SqlConnection Connection {
      get {
        lock (ConnectionSyncRoot) {
          EnsureConnectionIsOpen();
        }
        return connection;
      }
    }

    /// <summary>
    /// Gets the active transaction.
    /// </summary>    
    public DbTransaction Transaction { get; private set; }

    public override void ExecuteQueryTasks(IList<QueryTask> queryTasks, bool allowPartialExecution)
    {
      lock (ConnectionSyncRoot) {
        EnsureConnectionIsOpen();
        EnsureAutoShortenTransactionIsStarted();
        var nonBatchedTasks = new List<QueryTask>();
        foreach (var task in queryTasks) {
          var sqlProvider = task.DataSource as SqlProvider;
          if (sqlProvider!=null && sqlProvider.Request.CheckOptions(RequestOptions.AllowBatching))
            RegisterQueryTask(task, sqlProvider.Request);
          else
            nonBatchedTasks.Add(task);
        }
        if (nonBatchedTasks.Count > 0) {
          commandProcessor.ExecuteRequests();
          base.ExecuteQueryTasks(nonBatchedTasks, allowPartialExecution);
        }
        else
          commandProcessor.ExecuteRequests(allowPartialExecution);
      }
    }

    private void RegisterQueryTask(QueryTask task, QueryRequest request)
    {
      task.Result = new List<Tuple>();
      commandProcessor.RegisterTask(new SqlQueryTask(request, task.ParameterContext, task.Result));
    }

    #region Transaction control methods

    /// <inheritdoc/>
    public override void BeginTransaction()
    {
      lock (ConnectionSyncRoot) {
        EnsureConnectionIsOpen();
        if (Transaction!=null || IsAutoshortenTransactionActivated)
          throw new InvalidOperationException(Strings.ExTransactionIsAlreadyOpen);
        if (IsAutoshortenTransactionsEnabled()) {
          IsAutoshortenTransactionActivated = true;
          return;
        }
        BeginNativeTransaction();
      }
    }

    /// <inheritdoc/>
    public override void CommitTransaction()
    {
      base.CommitTransaction();
      lock (ConnectionSyncRoot) {
        if (Transaction==null && (!IsAutoshortenTransactionActivated && IsAutoshortenTransactionsEnabled()))
          throw new InvalidOperationException(Strings.ExTransactionIsNotOpen);
        if (Transaction!=null)
          driver.CommitTransaction(Session, Transaction);
        IsAutoshortenTransactionActivated = false;
        EndNativeTransaction();
      }
    }

    /// <inheritdoc/>
    public override void RollbackTransaction()
    {
      base.RollbackTransaction();
      lock (ConnectionSyncRoot) {
        if (Transaction==null && (!IsAutoshortenTransactionActivated && IsAutoshortenTransactionsEnabled()))
          throw new InvalidOperationException(Strings.ExTransactionIsNotOpen);
        if (Transaction!=null)
          driver.RollbackTransaction(Session, Transaction);
        IsAutoshortenTransactionActivated = false;
        EndNativeTransaction();
      }
    }

    #endregion
    
    #region IQueryExecutor members

    /// <inheritdoc/>
    public IEnumerator<Tuple> ExecuteTupleReader(QueryRequest request)
    {
      lock (ConnectionSyncRoot) {
        EnsureConnectionIsOpen();
        EnsureAutoShortenTransactionIsStarted();
        var enumerator = commandProcessor.ExecuteRequestsWithReader(request);
        using (enumerator) {
          while (enumerator.MoveNext())
            yield return enumerator.Current;
        }
      }
    }

    /// <inheritdoc/>
    public int ExecuteNonQuery(ISqlCompileUnit statement)
    {
      lock (ConnectionSyncRoot) {
        EnsureConnectionIsOpen();
        EnsureAutoShortenTransactionIsStarted();
        using (var command = CreateCommand(statement))
          return driver.ExecuteNonQuery(Session, command);
      }
    }

    /// <inheritdoc/>
    public object ExecuteScalar(ISqlCompileUnit statement)
    {
      lock (ConnectionSyncRoot) {
        EnsureConnectionIsOpen();
        EnsureAutoShortenTransactionIsStarted();
        using (var command = CreateCommand(statement))
          return driver.ExecuteScalar(Session, command);
      }
    }

    /// <inheritdoc/>
    public int ExecuteNonQuery(string commandText)
    {
      lock (ConnectionSyncRoot) {
        EnsureConnectionIsOpen();
        EnsureAutoShortenTransactionIsStarted();
        using (var command = CreateCommand(commandText))
          return driver.ExecuteNonQuery(Session, command);
      }
    }

    /// <inheritdoc/>
    public object ExecuteScalar(string commandText)
    {
      lock (ConnectionSyncRoot) {
        EnsureConnectionIsOpen();
        EnsureAutoShortenTransactionIsStarted();
        using (var command = CreateCommand(commandText))
          return driver.ExecuteScalar(Session, command);
      }
    }
    
    public void Store(TemporaryTableDescriptor descriptor, IEnumerable<Tuple> tuples)
    {
      lock (ConnectionSyncRoot) {
        EnsureConnectionIsOpen();
        EnsureAutoShortenTransactionIsStarted();
        foreach (var tuple in tuples)
          commandProcessor.RegisterTask(new SqlPersistTask(descriptor.StoreRequest, tuple));
        commandProcessor.ExecuteRequests();
      }
    }

    public void Clear(TemporaryTableDescriptor descriptor)
    {
      lock (ConnectionSyncRoot) {
        EnsureConnectionIsOpen();
        EnsureAutoShortenTransactionIsStarted();
        commandProcessor.RegisterTask(new SqlPersistTask(descriptor.ClearRequest, null));
        commandProcessor.ExecuteRequests();
      }
    }

    #endregion

    #region Insert, Update, Delete

    /// <inheritdoc/>
    public override void Persist(IEnumerable<PersistAction> persistActions, bool allowPartialExecution)
    {
      foreach (var action in persistActions)
        commandProcessor.RegisterTask(CreatePersistTask(action));
      commandProcessor.ExecuteRequests(allowPartialExecution);
    }

    private SqlPersistTask CreatePersistTask(PersistAction action)
    {
      switch (action.ActionKind) {
      case PersistActionKind.Insert:
        return CreateInsertTask(action);
      case PersistActionKind.Update:
        return CreateUpdateTask(action);
      case PersistActionKind.Remove:
        return CreateRemoveTask(action);
      default:
        throw new ArgumentOutOfRangeException("action.ActionKind");
      }
    }
    
    private SqlPersistTask CreateInsertTask(PersistAction action)
    {
      var task = new PersistRequestBuilderTask(PersistRequestKind.Insert, action.EntityState.Type);
      var request = domainHandler.GetPersistRequest(task);
      var tuple = action.EntityState.Tuple.ToRegular();
      return new SqlPersistTask(request, tuple);
    }

    private SqlPersistTask CreateUpdateTask(PersistAction action)
    {
      var entityState = action.EntityState;
      var dTuple = entityState.DifferentialTuple;
      var source = dTuple.Difference;
      var fieldStateMap = source.GetFieldStateMap(TupleFieldState.Available);
      var task = new PersistRequestBuilderTask(PersistRequestKind.Update, entityState.Type, fieldStateMap);
      var request = domainHandler.GetPersistRequest(task);
      var tuple = entityState.Tuple.ToRegular();
      return new SqlPersistTask(request, tuple);
    }

    private SqlPersistTask CreateRemoveTask(PersistAction action)
    {
      var task = new PersistRequestBuilderTask(PersistRequestKind.Remove, action.EntityState.Type);
      var request = domainHandler.GetPersistRequest(task);
      var tuple = action.EntityState.Key.Value;
      return new SqlPersistTask(request, tuple);
    }

    #endregion
    
    #region Private / internal members
    
    private CommandProcessor CreateCommandProcessor()
    {
      int batchSize = Session.Configuration.BatchSize;
      var result = Handlers.DomainHandler.ProviderInfo.Supports(ProviderFeatures.Batches) && batchSize > 1
        ? new BatchingCommandProcessor(this, batchSize)
        : (CommandProcessor) new SimpleCommandProcessor(this);
      return result;
    }

    private void EnsureConnectionIsOpen()
    {
      if (connection!=null && connection.State==ConnectionState.Open)
        return;
      if (connection==null)
        connection = driver.CreateConnection(Session, Handlers.Domain.Configuration.ConnectionInfo);
      driver.OpenConnection(Session, connection);
      commandProcessor = CreateCommandProcessor();
    }
    
    private void EnsureAutoShortenTransactionIsStarted()
    {
      // TODO: remove autoshortened transactions logic from SQL session handler
      if (Transaction!=null)
        return;
      if (!IsAutoshortenTransactionsEnabled() || !IsAutoshortenTransactionActivated)
        throw new InvalidOperationException(Strings.ExTransactionIsNotOpen);
      BeginNativeTransaction();
    }

    private void BeginNativeTransaction()
    {
      Transaction = driver.BeginTransaction(Session, connection, 
        IsolationLevelConverter.Convert(Session.Transaction.IsolationLevel));
      commandProcessor.Transaction = Transaction;
    }

    private void EndNativeTransaction()
    {
      commandProcessor.ClearTasks();
      Transaction = null;
    }

    private DbCommand CreateCommand(string commandText)
    {
      var command = connection.CreateCommand(commandText);
      command.Transaction = Transaction;
      command.CommandText = commandText;
      return command;
    }

    private DbCommand CreateCommand(ISqlCompileUnit statement)
    {
      var command = connection.CreateCommand(statement);
      command.Transaction = Transaction;
      return command;
    }
 
    #endregion

    /// <inheritdoc/>
    public override void Initialize()
    {
      base.Initialize();
      domainHandler = (DomainHandler) Handlers.DomainHandler;
      driver = domainHandler.Driver;
    }

    /// <inheritdoc/>
    public override void Dispose()
    {
      if (connection!=null)
        driver.CloseConnection(Session, connection);
    }
  }
}

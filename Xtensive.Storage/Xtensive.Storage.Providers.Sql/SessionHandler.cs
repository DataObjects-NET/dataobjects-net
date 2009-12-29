// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2008.05.20

using System;
using System.Collections.Generic;
using System.Transactions;
using Xtensive.Core.Tuples;
using Xtensive.Sql;
using Xtensive.Storage.Internals;

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

    /// <inheritdoc/>
    public override bool TransactionIsStarted {
      get { return connection!=null && connection.ActiveTransaction!=null; }
    }

    /// <summary>
    /// Gets the connection.
    /// </summary>
    public SqlConnection Connection {
      get {
        lock (connectionSyncRoot) {
          EnsureConnectionIsOpen();
        }
        return connection;
      }
    }

    /// <inheritdoc/>
    public override void ExecuteQueryTasks(IEnumerable<QueryTask> queryTasks, bool allowPartialExecution)
    {
      lock (connectionSyncRoot) {
        EnsureConnectionIsOpen();
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
    public override void BeginTransaction(IsolationLevel isolationLevel)
    {
      lock (connectionSyncRoot) {
        EnsureConnectionIsOpen();
        driver.BeginTransaction(
          Session, connection, IsolationLevelConverter.Convert(isolationLevel));
      }
    }

    /// <inheritdoc/>
    public override void MakeSavepoint(string name)
    {
      lock (connectionSyncRoot) {
        EnsureConnectionIsOpen();
        driver.MakeSavepoint(Session, connection, name);
      }
    }

    /// <inheritdoc/>
    public override void RollbackToSavepoint(string name)
    {
      base.RollbackToSavepoint(name);
      lock (connectionSyncRoot) {
        EnsureConnectionIsOpen();
        driver.RollbackToSavepoint(Session, connection, name);
      }
    }

    /// <inheritdoc/>
    public override void ReleaseSavepoint(string name)
    {
      base.ReleaseSavepoint(name);
      lock (connectionSyncRoot) {
        EnsureConnectionIsOpen();
        driver.ReleaseSavepoint(Session, connection, name);
      }
    }

    /// <inheritdoc/>
    public override void CommitTransaction()
    {
      base.CommitTransaction();
      lock (connectionSyncRoot) {
        if (Connection.ActiveTransaction!=null)
          driver.CommitTransaction(Session, connection);
        EndNativeTransaction();
      }
    }

    /// <inheritdoc/>
    public override void RollbackTransaction()
    {
      base.RollbackTransaction();
      lock (connectionSyncRoot) {
        if (Connection.ActiveTransaction!=null)
          driver.RollbackTransaction(Session, Connection);
        EndNativeTransaction();
      }
    }

    #endregion
    
    #region IQueryExecutor members

    /// <inheritdoc/>
    public IEnumerator<Tuple> ExecuteTupleReader(QueryRequest request)
    {
      lock (connectionSyncRoot) {
        EnsureConnectionIsOpen();
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
      lock (connectionSyncRoot) {
        EnsureConnectionIsOpen();
        using (var command = connection.CreateCommand(statement))
          return driver.ExecuteNonQuery(Session, command);
      }
    }

    /// <inheritdoc/>
    public object ExecuteScalar(ISqlCompileUnit statement)
    {
      lock (connectionSyncRoot) {
        EnsureConnectionIsOpen();
        using (var command = connection.CreateCommand(statement))
          return driver.ExecuteScalar(Session, command);
      }
    }

    /// <inheritdoc/>
    public int ExecuteNonQuery(string commandText)
    {
      lock (connectionSyncRoot) {
        EnsureConnectionIsOpen();
        using (var command = connection.CreateCommand(commandText))
          return driver.ExecuteNonQuery(Session, command);
      }
    }

    /// <inheritdoc/>
    public object ExecuteScalar(string commandText)
    {
      lock (connectionSyncRoot) {
        EnsureConnectionIsOpen();
        using (var command = connection.CreateCommand(commandText))
          return driver.ExecuteScalar(Session, command);
      }
    }
    
    public void Store(TemporaryTableDescriptor descriptor, IEnumerable<Tuple> tuples)
    {
      lock (connectionSyncRoot) {
        EnsureConnectionIsOpen();
        foreach (var tuple in tuples)
          commandProcessor.RegisterTask(new SqlPersistTask(descriptor.StoreRequest, tuple));
        commandProcessor.ExecuteRequests();
      }
    }

    public void Clear(TemporaryTableDescriptor descriptor)
    {
      lock (connectionSyncRoot) {
        EnsureConnectionIsOpen();
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

    private void EnsureConnectionIsOpen()
    {
      if (connection!=null)
        return;
      connection = driver.CreateConnection(Session);
      driver.OpenConnection(Session, connection);
      commandProcessor = domainHandler.CommandProcessorFactory.CreateCommandProcessor(Session, Connection);
    }

    private void EndNativeTransaction()
    {
      commandProcessor.ClearTasks();
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

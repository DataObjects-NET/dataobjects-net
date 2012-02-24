// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2008.05.20

using System;
using System.Collections.Generic;
using Xtensive.Collections;
using Xtensive.IoC;
using Xtensive.Orm.Internals;
using Xtensive.Sql;
using Xtensive.Tuples;
using Tuple = Xtensive.Tuples.Tuple;

namespace Xtensive.Orm.Providers.Sql
{
  /// <summary>
  /// <see cref="Session"/>-level handler for SQL storages.
  /// </summary>
  public partial class SessionHandler : Providers.SessionHandler,
    IQueryExecutor,
    IDirectSqlService
  {
    private static readonly IEnumerable<ServiceRegistration> baseServiceRegistrations =
      EnumerableUtils<ServiceRegistration>.Empty;

    private SqlStorageDriver driver;
    private DomainHandler domainHandler;
    private SqlConnection connection;
    private CommandProcessor commandProcessor;

    /// <inheritdoc/>
    public override bool TransactionIsStarted {
      get { return connection!=null && connection.ActiveTransaction!=null; }
    }

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
    /// Gets <see cref="CommandPartFactory"/> associated with current instance.
    /// </summary>
    public CommandPartFactory CommandPartFactory
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
      if (connection!=null)
        connection.CommandTimeout = commandTimeout;
    }

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
      if (nonBatchedTasks.Count > 0) {
        commandProcessor.ExecuteTasks();
        base.ExecuteQueryTasks(nonBatchedTasks, allowPartialExecution);
      }
      else
        commandProcessor.ExecuteTasks(allowPartialExecution);
    }

    private void RegisterQueryTask(QueryTask task, QueryRequest request)
    {
      task.Result = new List<Tuple>();
      commandProcessor.Tasks.Enqueue(new SqlQueryTask(request, task.ParameterContext, task.Result));
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
      if (Connection.ActiveTransaction!=null)
        driver.CommitTransaction(Session, connection);
      EndNativeTransaction();
    }

    /// <inheritdoc/>
    public override void RollbackTransaction(Transaction transaction)
    {
      if (Connection.ActiveTransaction!=null)
        driver.RollbackTransaction(Session, Connection);
      EndNativeTransaction();
    }

    #endregion
    
    #region Insert, Update, Delete

    /// <inheritdoc/>
    public override void Persist(IEnumerable<PersistAction> persistActions, bool allowPartialExecution)
    {
      foreach (var action in persistActions)
        commandProcessor.Tasks.Enqueue(CreatePersistTask(action));
      commandProcessor.ExecuteTasks(allowPartialExecution);
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
      commandProcessor.Tasks.Clear();
    }

    #endregion

    /// <inheritdoc/>
    protected override void AddBaseServiceRegistrations(List<ServiceRegistration> registrations)
    {
      registrations.AddRange(baseServiceRegistrations);
    }

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

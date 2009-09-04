// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2008.05.20

using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using Xtensive.Core;
using Xtensive.Core.Tuples;
using Xtensive.Sql;
using Xtensive.Storage.Internals;
using Xtensive.Storage.Providers.Sql.Resources;
using Xtensive.Storage.Rse.Providers;

namespace Xtensive.Storage.Providers.Sql
{
  /// <summary>
  /// <see cref="Session"/>-level handler for SQL storages.
  /// </summary>
  public class SessionHandler : Providers.SessionHandler
  {
    private Driver driver;
    private DomainHandler domainHandler;
    private SqlConnection connection;
    private CommandProcessor commandProcessor;

    /// <summary>
    /// Gets the connection.
    /// </summary>
    public SqlConnection Connection { // TODO: remove this property
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
        BeginDbTransaction();
      }
    }

    /// <inheritdoc/>
    public override void CommitTransaction()
    {
      lock (ConnectionSyncRoot) {
        if (Transaction==null && (!IsAutoshortenTransactionActivated && IsAutoshortenTransactionsEnabled()))
          throw new InvalidOperationException(Strings.ExTransactionIsNotOpen);
        if (Transaction!=null)
          driver.CommitTransaction(Transaction);
        IsAutoshortenTransactionActivated = false;
        Transaction = null;
      }
    }

    /// <inheritdoc/>
    public override void RollbackTransaction()
    {
      lock (ConnectionSyncRoot) {
        if (Transaction==null && (!IsAutoshortenTransactionActivated && IsAutoshortenTransactionsEnabled()))
          throw new InvalidOperationException(Strings.ExTransactionIsNotOpen);
        if (Transaction!=null)
          driver.RollbackTransaction(Transaction);
        IsAutoshortenTransactionActivated = false;
        Transaction = null;
      }
    }

    #endregion

    /// <inheritdoc/>
    public override IEnumerator<Tuple> Execute(ExecutableProvider provider)
    {
      lock (ConnectionSyncRoot) {
        EnsureConnectionIsOpen();
        EnsureAutoShortenTransactionIsStarted();
        ActivateCommandProcessor();
        var enumerator = commandProcessor.ExecuteRequestsWithReader(((SqlProvider) provider).Request);
        using (enumerator) {
          while (enumerator.MoveNext())
            yield return enumerator.Current;
        }
      }
    }

    public override void Execute(IList<QueryTask> queryTasks, bool dirty)
    {
      lock (ConnectionSyncRoot) {
        EnsureConnectionIsOpen();
        EnsureAutoShortenTransactionIsStarted();
        ActivateCommandProcessor();
        foreach (var task in queryTasks) {
          var request = ((SqlProvider) task.DataSource).Request;
          task.Result = new List<Tuple>();
          commandProcessor.RegisterTask(new SqlQueryTask(request, task.ParameterContext, task.Result));
        }
        commandProcessor.ExecuteRequests(dirty);
      }
    }

    #region ExecuteStatement methods

    internal int ExecuteNonQueryStatement(ISqlCompileUnit statement)
    {
      lock (ConnectionSyncRoot) {
        EnsureConnectionIsOpen();
        EnsureAutoShortenTransactionIsStarted();
        using (var command = CreateCommand(statement))
          return driver.ExecuteNonQuery(command);
      }
    }

    internal object ExecuteScalarStatement(ISqlCompileUnit statement)
    {
      lock (ConnectionSyncRoot) {
        EnsureConnectionIsOpen();
        EnsureAutoShortenTransactionIsStarted();
        using (var command = CreateCommand(statement))
          return driver.ExecuteScalar(command);
      }
    }

    internal int ExecuteNonQueryStatement(string commandText)
    {
      lock (ConnectionSyncRoot) {
        EnsureConnectionIsOpen();
        EnsureAutoShortenTransactionIsStarted();
        using (var command = CreateCommand(commandText))
          return driver.ExecuteNonQuery(command);
      }
    }

    internal object ExecuteScalarStatement(string commandText)
    {
      lock (ConnectionSyncRoot) {
        EnsureConnectionIsOpen();
        EnsureAutoShortenTransactionIsStarted();
        using (var command = CreateCommand(commandText))
          return driver.ExecuteScalar(command);
      }
    }

    #endregion

    #region ExecuteRequest methods

    /// <summary>
    /// Executes the specified <see cref="SqlPersistRequest"/>.
    /// </summary>
    /// <param name="request">The request to execute.</param>
    /// <param name="tuple">A state tuple.</param>
    /// <returns>Number of modified rows.</returns>
    public void ExecutePersistRequest(SqlPersistRequest request, Tuple tuple)
    {
      lock (ConnectionSyncRoot) {
        EnsureConnectionIsOpen();
        EnsureAutoShortenTransactionIsStarted();
        ActivateCommandProcessor();
        commandProcessor.ExecutePersist(new SqlPersistTask(request, tuple));
      }
    }

    /// <summary>
    /// Executes the specified <see cref="SqlScalarRequest"/>.
    /// </summary>
    /// <param name="request">The request to execute.</param>
    /// <returns>The first column of the first row of executed result set.</returns>
    public object ExecuteScalarRequest(SqlScalarRequest request)
    {
      lock (ConnectionSyncRoot) {
        EnsureConnectionIsOpen();
        EnsureAutoShortenTransactionIsStarted();
        ActivateCommandProcessor();
        return commandProcessor.ExecuteScalar(new SqlScalarTask(request));
      }
    }

    #endregion

    #region Insert, Update, Delete

    /// <inheritdoc/>
    public override void Persist(IEnumerable<PersistAction> persistActions, bool dirty)
    {
      ActivateCommandProcessor();
      foreach (var action in persistActions)
        commandProcessor.RegisterTask(CreatePersistTask(action));
      commandProcessor.ExecuteRequests(dirty);
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
      var task = new SqlRequestBuilderTask(SqlPersistRequestKind.Insert, action.EntityState.Type);
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
      var task = new SqlRequestBuilderTask(SqlPersistRequestKind.Update, entityState.Type, fieldStateMap);
      var request = domainHandler.GetPersistRequest(task);
      var tuple = entityState.Tuple.ToRegular();
      return new SqlPersistTask(request, tuple);
    }

    private SqlPersistTask CreateRemoveTask(PersistAction action)
    {
      var task = new SqlRequestBuilderTask(SqlPersistRequestKind.Remove, action.EntityState.Type);
      var request = domainHandler.GetPersistRequest(task);
      var tuple = action.EntityState.Key.Value;
      return new SqlPersistTask(request, tuple);
    }

    #endregion
    
    #region Private / internal members
    
    private void EnsureConnectionIsOpen()
    {
      if (connection!=null && connection.State==ConnectionState.Open)
        return;
      if (connection==null)
        connection = driver.CreateConnection(Handlers.Domain.Configuration.ConnectionInfo);
      driver.OpenConnection(connection);
    }
    
    private void EnsureAutoShortenTransactionIsStarted()
    {
      if (Transaction!=null)
        return;
      if (!IsAutoshortenTransactionsEnabled() || !IsAutoshortenTransactionActivated)
        throw new InvalidOperationException(Strings.ExTransactionIsNotOpen);
      BeginDbTransaction();
    }

    private void BeginDbTransaction()
    {
      Transaction = driver.BeginTransaction(
        connection, IsolationLevelConverter.Convert(Session.Transaction.IsolationLevel));
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

    private void ActivateCommandProcessor()
    {
      commandProcessor.Connection = Connection;
      commandProcessor.Transaction = Transaction;
    }
 
    #endregion

    /// <inheritdoc/>
    public override void Initialize()
    {
      base.Initialize();
      domainHandler = (DomainHandler) Handlers.DomainHandler;
      driver = domainHandler.Driver;
      
      int batchSize = this.Session.Configuration.BatchSize;
      commandProcessor =
        Handlers.DomainHandler.ProviderInfo.Supports(ProviderFeatures.Batches) && batchSize > 1
          ? new BatchingCommandProcessor(domainHandler, batchSize)
          : (CommandProcessor) new SimpleCommandProcessor(domainHandler);
    }

    /// <inheritdoc/>
    public override void Dispose()
    {
      if (connection!=null)
        driver.CloseConnection(connection);
    }
  }
}

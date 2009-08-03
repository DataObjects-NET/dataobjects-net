// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2008.05.20

using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;
using Xtensive.Core;
using Xtensive.Core.Collections;
using Xtensive.Core.Disposing;
using Xtensive.Core.Tuples;
using Xtensive.Sql;
using Xtensive.Sql.ValueTypeMapping;
using Xtensive.Storage.Configuration;
using Xtensive.Storage.Internals;
using Xtensive.Storage.Model;
using Xtensive.Storage.Providers.Sql.Resources;

namespace Xtensive.Storage.Providers.Sql
{
  /// <summary>
  /// <see cref="Session"/>-level handler for SQL storages.
  /// </summary>
  public class SessionHandler : Providers.SessionHandler
  {
    private const int PersistBatchSize = 25;
    
    private SqlConnection connection;
    
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

    /// <summary>
    /// Gets the domain handler.
    /// </summary>
    protected internal DomainHandler DomainHandler { get; private set; }

    #region Transaction control methods

    /// <inheritdoc/>
    public override void BeginTransaction()
    {
      lock (ConnectionSyncRoot) {
        EnsureConnectionIsOpen();
        if (Transaction!=null || IsAutoshortenTransactionActivated)
          throw new InvalidOperationException(Strings.TransactionIsAlreadyOpen);
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
          throw new InvalidOperationException(Strings.TransactionIsNotOpen);
        if (Transaction != null)
          Transaction.Commit();
        IsAutoshortenTransactionActivated = false;
        Transaction = null;
      }
    }

    /// <inheritdoc/>
    public override void RollbackTransaction()
    {
      lock (ConnectionSyncRoot) {
        if (Transaction==null && (!IsAutoshortenTransactionActivated && IsAutoshortenTransactionsEnabled()))
          throw new InvalidOperationException(Strings.TransactionIsNotOpen);
        if (Transaction != null)
          Transaction.Rollback();
        IsAutoshortenTransactionActivated = false;
        Transaction = null;
      }
    }

    #endregion
    
    /// <summary>
    /// Executes the specified <see cref="SqlFetchRequest"/>.
    /// </summary>
    /// <param name="request">The request.</param>
    /// <returns></returns>
    public IEnumerator<Tuple> Execute(SqlFetchRequest request)
    {
      lock (ConnectionSyncRoot) {
        var descriptor = request.TupleDescriptor;
        var accessor = DomainHandler.GetDataReaderAccessor(descriptor);
        using (var reader = ExecuteFetchRequest(request))
          while (reader.Read()) {
            var tuple = Tuple.Create(descriptor);
            accessor.Read(reader, tuple);
            yield return tuple;
          }
      }
    }

    #region ExecuteStatement methods

    /// <summary>
    /// Executes the specified statement.
    /// Works similar to <see cref="DbCommand.ExecuteDbDataReader"/>
    /// </summary>
    /// <param name="statement">The statement to execute.</param>
    /// <returns><see cref="DbDataReader"/> with results of statement execution.</returns>
    public DbDataReader ExecuteReaderStatement(ISqlCompileUnit statement)
    {
      lock (ConnectionSyncRoot) {
        EnsureConnectionIsOpen();
        EnsureAutoShortenTransactionIsStarted();
        using (var command = connection.CreateCommand(statement)) {
          command.Transaction = Transaction;
          return command.ExecuteReader();
        }
      }
    }

    /// <summary>
    /// Executes the specified statement.
    /// Works similar to <see cref="DbCommand.ExecuteNonQuery"/>.
    /// </summary>
    /// <param name="statement">The statement.</param>
    /// <returns>Number of affected rows.</returns>
    public int ExecuteNonQueryStatement(ISqlCompileUnit statement)
    {
      lock (ConnectionSyncRoot) {
        EnsureConnectionIsOpen();
        EnsureAutoShortenTransactionIsStarted();
        using (var command = connection.CreateCommand(statement)) {
          command.Transaction = Transaction;
          return command.ExecuteNonQuery();
        }
      }
    }

    /// <summary>
    /// Executes the specified statement.
    /// Works similar to <see cref="DbCommand.ExecuteScalar"/>
    /// </summary>
    /// <param name="statement">The statement.</param>
    /// <returns>The first column of the first row of executed result set.</returns>
    public object ExecuteScalarStatement(ISqlCompileUnit statement)
    {
      lock (ConnectionSyncRoot) {
        EnsureConnectionIsOpen();
        EnsureAutoShortenTransactionIsStarted();
        using (var command = connection.CreateCommand(statement)) {
          command.Transaction = Transaction;
          return command.ExecuteScalar();
        }
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
    public int ExecutePersistRequest(SqlPersistRequest request, Tuple tuple)
    {
      lock (ConnectionSyncRoot) {
        EnsureConnectionIsOpen();
        EnsureAutoShortenTransactionIsStarted();
        using (var command = CreatePersistCommand(request, tuple)) {
          command.Transaction = Transaction;
          return command.ExecuteNonQuery();
        }
      }
    }

    /// <summary>
    /// Executes the batch update request in a single batch.
    /// </summary>
    /// <param name="requests">The requests.</param>
    /// <returns>Number of modified rows.</returns>
    public int ExecuteBatchPersistRequest(IEnumerable<Pair<SqlPersistRequest, Tuple>> requests)
    {
      lock (ConnectionSyncRoot) {
        EnsureConnectionIsOpen();
        EnsureAutoShortenTransactionIsStarted();
        using (var command = CreateBatchPersistCommand(requests)) {
          command.Transaction = Transaction;
          return command.ExecuteNonQuery();
        }
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
        using (var command = CreateScalarCommand(request)) {
          command.Transaction = Transaction;
          return command.ExecuteScalar();
        }
      }
    }

    /// <summary>
    /// Executes the specified <see cref="SqlFetchRequest"/>.
    /// </summary>
    /// <param name="request">The request to execute.</param>
    /// <returns><see cref="DbDataReader"/> with results of statement execution.</returns>
    public DbDataReader ExecuteFetchRequest(SqlFetchRequest request)
    {
      lock (ConnectionSyncRoot) {
        EnsureConnectionIsOpen();
        EnsureAutoShortenTransactionIsStarted();
        using (var command = CreateFetchCommand(request)) {
          command.Transaction = Transaction;
          return command.ExecuteReader();
        }
      }
    }

    #endregion

    #region Insert, Update, Delete

    /// <inheritdoc/>
    public override void Persist(IEnumerable<PersistAction> persistActions)
    {
      var batched = persistActions
        .Select<PersistAction, Pair<SqlPersistRequest, Tuple>>(CreatePersistRequest)
        .Batch(0, PersistBatchSize, PersistBatchSize);
      foreach (var batch in batched)
        ExecuteBatchPersistRequest(batch);
    }

    private Pair<SqlPersistRequest, Tuple> CreatePersistRequest(PersistAction action)
    {
      switch (action.ActionKind) {
      case PersistActionKind.Insert:
        return CreateInsertRequest(action);
      case PersistActionKind.Update:
        return CreateUpdateRequest(action);
      case PersistActionKind.Remove:
        return CreateRemoveRequest(action);
      default:
        throw new ArgumentOutOfRangeException("action.ActionKind");
      }
    }
    
    private Pair<SqlPersistRequest, Tuple> CreateInsertRequest(PersistAction action)
    {
      var task = new SqlRequestBuilderTask(SqlPersistRequestKind.Insert, action.EntityState.Type);
      var request = DomainHandler.GetPersistRequest(task);
      var tuple = action.EntityState.Tuple.ToRegular();
      return new Pair<SqlPersistRequest, Tuple>(request, tuple);
    }

    private Pair<SqlPersistRequest, Tuple> CreateUpdateRequest(PersistAction action)
    {
      var entityState = action.EntityState;
      var dTuple = entityState.DifferentialTuple;
      var source = dTuple.Difference;
      if (source==null) // Because new entity can be updated by toposort
        source = dTuple.Origin; // TODO: Fix this workaround!
      var fieldStateMap = source.GetFieldStateMap(TupleFieldState.Available);
      var task = new SqlRequestBuilderTask(SqlPersistRequestKind.Update, entityState.Type, fieldStateMap);
      var request = DomainHandler.GetPersistRequest(task);
      var tuple = entityState.Tuple.ToRegular();
      return new Pair<SqlPersistRequest, Tuple>(request, tuple);
    }

    private Pair<SqlPersistRequest, Tuple> CreateRemoveRequest(PersistAction action)
    {
      var task = new SqlRequestBuilderTask(SqlPersistRequestKind.Remove, action.EntityState.Type);
      var request = DomainHandler.GetPersistRequest(task);
      var tuple = action.EntityState.Key.Value;
      return new Pair<SqlPersistRequest, Tuple>(request, tuple);
    }

    #endregion

    #region CreateCommand methods

    /// <summary>
    /// Creates <see cref="DbCommand"/> from specified <see cref="SqlScalarRequest"/>.
    /// </summary>
    /// <param name="request">The request.</param>
    /// <returns>A created command.</returns>
    protected DbCommand CreateScalarCommand(SqlScalarRequest request)
    {
      var command = connection.CreateCommand();
      command.CommandText = request.Compile(DomainHandler).GetCommandText();
      return command;
    }

    /// <summary>
    /// Creates <see cref="DbCommand"/> from specified <see cref="SqlFetchRequest"/>.
    /// </summary>
    /// <param name="request">The request.</param>
    /// <returns>A created command.</returns>
    protected DbCommand CreateFetchCommand(SqlFetchRequest request)
    {
      var command = connection.CreateCommand();
      var compilationResult = request.Compile(DomainHandler);
      var variantKeys = new List<object>();
      foreach (var binding in request.ParameterBindings) {
        object parameterValue = binding.ValueAccessor.Invoke();
        if (parameterValue==null && binding.SmartNull)
          variantKeys.Add(binding.ParameterReference.Parameter);
        else {
          string parameterName = compilationResult.GetParameterName(binding.ParameterReference.Parameter);
          AddParameter(command, parameterName, parameterValue, binding.TypeMapping);
        }
      }
      command.CommandText = compilationResult.GetCommandText(variantKeys);
      return command;
    }

    /// <summary>
    /// Creates <see cref="DbCommand"/> from specified <see cref="SqlPersistRequest"/>.
    /// </summary>
    /// <param name="request">The request.</param>
    /// <param name="value">Tuple that contain values for update parameters.</param>
    /// <returns>A created command.</returns>
    protected DbCommand CreatePersistCommand(SqlPersistRequest request, Tuple value)
    {
      var command = connection.CreateCommand();
      command.CommandText = FillCommandParameters(command, request, value, "p");
      return command;
    }

    /// <summary>
    /// Creates <see cref="DbCommand"/> from specified sequence of <see cref="SqlPersistRequest"/> and correnponding tuples.
    /// </summary>
    /// <param name="requests">The requests.</param>
    /// <returns>A created command.</returns>
    protected DbCommand CreateBatchPersistCommand(IEnumerable<Pair<SqlPersistRequest, Tuple>> requests)
    {
      var command = connection.CreateCommand();
      var commandText = new StringBuilder();
      var requestNumber = 0;
      foreach (var request in requests) {
        var currentCommandText = FillCommandParameters(command, request.First, request.Second,
          string.Format("p{0}_", requestNumber));
        commandText.AppendLine(currentCommandText);
        requestNumber++;
      }
      command.CommandText = commandText.ToString();
      return command;
    }

    /// <summary>
    /// Fills the command parameters to the specified command.
    /// </summary>
    /// <param name="command">The command.</param>
    /// <param name="request">The request.</param>
    /// <param name="value">The value.</param>
    /// <param name="parameterNamePrefix">The parameter name prefix.</param>
    /// <returns>Command text to execute.</returns>
    protected string FillCommandParameters(DbCommand command,
      SqlPersistRequest request, Tuple value, string parameterNamePrefix)
    {
      int parameterIndex = 0;
      var parameterNames = new Dictionary<object, string>();
      var compilationResult = request.Compile(DomainHandler);
      foreach (var binding in request.ParameterBindings) {
        object parameterValue = value.IsNull(binding.FieldIndex) ? null : value.GetValueOrDefault(binding.FieldIndex);
        string parameterName = parameterNamePrefix + parameterIndex++;
        parameterNames.Add(binding.ParameterReference.Parameter, parameterName);
        AddParameter(command, parameterName, parameterValue, binding.TypeMapping);
      }
      return compilationResult.GetCommandText(parameterNames);
    }

    /// <summary>
    /// Creates the parameter with the specified <paramref name="name"/> and <paramref name="value"/>
    /// taking into account <paramref name="mapping"/>.
    /// Created parameter is registered in <paramref name="command"/>.
    /// </summary>
    /// <param name="command">The command.</param>
    /// <param name="name">The name.</param>
    /// <param name="value">The value.</param>
    /// <param name="mapping">The mapping.</param>
    protected void AddParameter(DbCommand command, string name, object value, TypeMapping mapping)
    {
      var parameter = command.CreateParameter();
      parameter.ParameterName = name;
      mapping.SetParameterValue(parameter, value);
      command.Parameters.Add(parameter);
    }

    #endregion

    /// <summary>
    /// Ensures the connection is open.
    /// </summary>
    protected void EnsureConnectionIsOpen()
    {
      if (connection!=null && connection.State==ConnectionState.Open)
        return;
      connection = DomainHandler.Driver.CreateConnection(Handlers.Domain.Configuration.ConnectionInfo);
      if (connection==null)
        throw new InvalidOperationException(Strings.ExUnableToCreateConnection);
      connection.Open();
    }

    #region Private / internal members

    private void BeginDbTransaction()
    {
      Transaction = connection.BeginTransaction(
        IsolationLevelConverter.Convert(Session.Transaction.IsolationLevel));
    }

    private void EnsureAutoShortenTransactionIsStarted()
    {
      if (Transaction==null) {
        if (!IsAutoshortenTransactionsEnabled() || !IsAutoshortenTransactionActivated)
          throw new InvalidOperationException(Strings.TransactionIsNotOpen);
        BeginDbTransaction();
      }
    }

    #endregion

    /// <inheritdoc/>
    public override void Initialize()
    {
      DomainHandler = Handlers.DomainHandler as DomainHandler;
    }

    /// <inheritdoc/>
    public override void Dispose()
    {
      connection.DisposeSafely();
    }
  }
}

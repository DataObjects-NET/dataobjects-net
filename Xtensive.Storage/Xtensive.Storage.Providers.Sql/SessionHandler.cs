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
using Xtensive.Core;
using Xtensive.Core.Collections;
using Xtensive.Core.Disposing;
using Xtensive.Core.Tuples;
using Xtensive.Sql;
using Xtensive.Sql.ValueTypeMapping;
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
    private SqlConnection connection;

    /// <summary>
    /// Gets the connection.
    /// </summary>
    public SqlConnection Connection {
      get {
        EnsureConnectionIsOpen();
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
      EnsureConnectionIsOpen();
      if (Transaction!=null)
        throw new InvalidOperationException(Strings.TransactionIsAlreadyOpen);
      Transaction = connection.BeginTransaction(
        IsolationLevelConverter.Convert(Session.Transaction.IsolationLevel));
    }

    /// <inheritdoc/>
    public override void CommitTransaction()
    {
      if (Transaction==null)
        throw new InvalidOperationException(Strings.TransactionIsNotOpen);
      Transaction.Commit();
      Transaction = null;
    }

    /// <inheritdoc/>
    public override void RollbackTransaction()
    {
      if (Transaction==null)
        throw new InvalidOperationException(Strings.TransactionIsNotOpen);
      Transaction.Rollback();
      Transaction = null;
    }

    #endregion

    /// <summary>
    /// Executes the specified <see cref="SqlFetchRequest"/>.
    /// </summary>
    /// <param name="request">The request.</param>
    /// <returns></returns>
    public IEnumerator<Tuple> Execute(SqlFetchRequest request)
    {
      var descriptor = request.TupleDescriptor;
      var accessor = DomainHandler.GetDataReaderAccessor(descriptor);
      using (var reader = ExecuteFetchRequest(request))
        while (reader.Read()) {
          var tuple = Tuple.Create(descriptor);
          accessor.Read(reader, tuple);
          yield return tuple;
        }
    }

    #region Execute statement methods

    /// <summary>
    /// Executes the specified statement.
    /// Works similar to <see cref="DbCommand.ExecuteDbDataReader"/>
    /// </summary>
    /// <param name="statement">The statement to execute.</param>
    /// <returns><see cref="DbDataReader"/> with results of statement execution.</returns>
    public DbDataReader ExecuteReaderStatement(ISqlCompileUnit statement)
    {
      EnsureConnectionIsOpen();
      using (var command = Connection.CreateCommand(statement)) {
        command.Transaction = Transaction;
        return command.ExecuteReader();
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
      EnsureConnectionIsOpen();
      using (var command = Connection.CreateCommand(statement)) {
        command.Transaction = Transaction;
        return command.ExecuteNonQuery();
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
      EnsureConnectionIsOpen();
      using (var command = Connection.CreateCommand(statement)) {
        command.Transaction = Transaction;
        return command.ExecuteScalar();
      }
    }

    #endregion

    #region Execute request methods

    /// <summary>
    /// Executes the specified <see cref="SqlUpdateRequest"/>.
    /// </summary>
    /// <param name="request">The request to execute.</param>
    /// <param name="tuple">A state tuple.</param>
    /// <returns>Number of modified rows.</returns>
    public int ExecuteUpdateRequest(SqlUpdateRequest request, Tuple tuple)
    {
      EnsureConnectionIsOpen();
      using (var command = CreateUpdateCommand(request, tuple)) {
        command.Transaction = Transaction;
        return command.ExecuteNonQuery();
      }
    }

    /// <summary>
    /// Executes the specified <see cref="SqlScalarRequest"/>.
    /// </summary>
    /// <param name="request">The request to execute.</param>
    /// <returns>The first column of the first row of executed result set.</returns>
    public object ExecuteScalarRequest(SqlScalarRequest request)
    {
      EnsureConnectionIsOpen();
      using (var command = CreateScalarCommand(request)) {
        command.Transaction = Transaction;
        return command.ExecuteScalar();
      }
    }

    /// <summary>
    /// Executes the specified <see cref="SqlFetchRequest"/>.
    /// </summary>
    /// <param name="request">The request to execute.</param>
    /// <returns><see cref="DbDataReader"/> with results of statement execution.</returns>
    public DbDataReader ExecuteFetchRequest(SqlFetchRequest request)
    {
      EnsureConnectionIsOpen();
      using (var command = CreateFetchCommand(request)) {
        command.Transaction = Transaction;
        return command.ExecuteReader();
      }
    }

    #endregion

    #region Insert, Update, Delete

    /// <inheritdoc/>
    public override void Persist(IEnumerable<EntityStateAction> entityStateActions)
    {
      var batched = entityStateActions.Select(entityStateAction =>
        new Triplet<SqlUpdateRequest, EntityStateAction, Tuple>(
          CreateRequest(entityStateAction),
          entityStateAction,
          CloneValue(entityStateAction)))
        .Batch(0, 1, 1);
      foreach (var batch in batched)
        PersistBatch(batch);
    }

    private SqlUpdateRequest CreateRequest(EntityStateAction entityStateAction)
    {
      switch (entityStateAction.PersistAction) {
        case PersistAction.Insert:
          return CreateInsert(entityStateAction.EntityState);
        case PersistAction.Update:
          return CreateUpdate(entityStateAction.EntityState);
        case PersistAction.Remove:
          return CreateRemove(entityStateAction.EntityState);
        default:
          throw new ArgumentOutOfRangeException("entityStateAction.PersistAction");
      }
    }

    private static Tuple CloneValue(EntityStateAction entityStateAction)
    {
      switch (entityStateAction.PersistAction) {
        case PersistAction.Insert:
        case PersistAction.Update:
          return entityStateAction.EntityState.Tuple.ToRegular();
        case PersistAction.Remove:
          return entityStateAction.EntityState.Key.Value;
        default:
          throw new ArgumentOutOfRangeException("entityStateAction.PersistAction");
      }
    }
    
    private SqlUpdateRequest CreateInsert(EntityState state)
    {
      var task = new SqlRequestBuilderTask(SqlUpdateRequestKind.Insert, state.Type);
      return DomainHandler.RequestCache
        .GetValue(task, _task => DomainHandler.RequestBuilder.Build(_task));
    }

    private SqlUpdateRequest CreateUpdate(EntityState state)
    {
      var fieldStateMap = state.GetDifferentialTuple().Difference
        .GetFieldStateMap(TupleFieldState.Available);
      var task = new SqlRequestBuilderTask(SqlUpdateRequestKind.Update, state.Type, fieldStateMap);
      return DomainHandler.RequestCache.GetValue(task, _task => DomainHandler.RequestBuilder.Build(_task));
    }

    private SqlUpdateRequest CreateRemove(EntityState state)
    {
      var task = new SqlRequestBuilderTask(SqlUpdateRequestKind.Remove, state.Type);
      return DomainHandler.RequestCache.GetValue(task, _task => DomainHandler.RequestBuilder.Build(_task));
    }

    private void PersistBatch(
      IEnumerable<Triplet<SqlUpdateRequest, EntityStateAction, Tuple>> requestTriplets)
    {
      foreach (var requestTriplet in requestTriplets) {
        switch (requestTriplet.Second.PersistAction) {
          case PersistAction.Insert:
            ExecuteInsert(requestTriplet.First, requestTriplet.Second.EntityState.Type,
              requestTriplet.Third);
            break;
          case PersistAction.Update:
            ExecuteUpdate(requestTriplet.First, requestTriplet.Second.EntityState.Type,
              requestTriplet.Third);
            break;
          case PersistAction.Remove:
            ExecuteRemove(requestTriplet.First, requestTriplet.Second.EntityState.Type,
              requestTriplet.Third);
            break;
          default:
            throw new ArgumentOutOfRangeException("requestTriplet.Third");
        }
      }
    }

    private void ExecuteInsert(SqlUpdateRequest request, TypeInfo typeInfo, Tuple value)
    {
      int rowsAffected = ExecuteUpdateRequest(request, value);
      if (rowsAffected!=request.ExpectedResult)
        throw new InvalidOperationException(
          string.Format(Strings.ExErrorOnInsert, typeInfo.Name, rowsAffected,
            request.ExpectedResult));
    }

    private void ExecuteUpdate(SqlUpdateRequest request, TypeInfo typeInfo, Tuple value)
    {
      int rowsAffected = ExecuteUpdateRequest(request, value);
      if (rowsAffected!=request.ExpectedResult)
        throw new InvalidOperationException(
          string.Format(Strings.ExErrorOnUpdate, typeInfo.Name, rowsAffected,
            request.ExpectedResult));
    }

    private void ExecuteRemove(SqlUpdateRequest request, TypeInfo typeInfo, Tuple value)
    {
      int rowsAffected = ExecuteUpdateRequest(request, value);
      if (rowsAffected!=request.ExpectedResult)
        throw new InvalidOperationException(
          string.Format(
            rowsAffected==0 ? Strings.ExInstanceNotFound : Strings.ExInstanceMultipleResults,
            typeInfo.Name));
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
      var command = Connection.CreateCommand();
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
      var command = Connection.CreateCommand();
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
    /// Creates <see cref="DbCommand"/> from specified <see cref="SqlUpdateRequest"/>.
    /// </summary>
    /// <param name="request">The request.</param>
    /// <param name="value">Tuple that contain values for update parameters.</param>
    /// <returns>A created command.</returns>
    protected DbCommand CreateUpdateCommand(SqlUpdateRequest request, Tuple value)
    {
      var command = Connection.CreateCommand();
      var compilationResult = request.Compile(DomainHandler);

      foreach (var binding in request.ParameterBindings) {
        object parameterValue = value.IsNull(binding.FieldIndex) ? null : value.GetValue(binding.FieldIndex);
        string parameterName = compilationResult.GetParameterName(binding.ParameterReference.Parameter);
        AddParameter(command, parameterName, parameterValue, binding.TypeMapping);
      }

      command.CommandText = compilationResult.GetCommandText();
      return command;
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
    public void EnsureConnectionIsOpen()
    {
      if (connection!=null && connection.State==ConnectionState.Open)
        return;
      connection = DomainHandler.Driver.CreateConnection(Handlers.Domain.Configuration.ConnectionInfo.ToString());
      if (connection==null)
        throw new InvalidOperationException(Strings.ExUnableToCreateConnection);
      connection.Open();
    }

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

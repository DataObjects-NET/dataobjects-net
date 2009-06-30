// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2008.05.20

using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using Xtensive.Core.Disposing;
using Xtensive.Core.Tuples;
using Xtensive.Sql.Common;
using Xtensive.Sql.Dom;
using Xtensive.Storage.Providers.Sql.Mappings;
using Xtensive.Storage.Providers.Sql.Resources;

namespace Xtensive.Storage.Providers.Sql
{
  /// <summary>
  /// <see cref="Session"/>-level handler for SQL storages.
  /// </summary>
  public abstract class SessionHandler : Providers.SessionHandler
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
    /// Works similar to <see cref="SqlCommand.ExecuteDbDataReader"/>
    /// </summary>
    /// <param name="statement">The statement to execute.</param>
    /// <returns><see cref="DbDataReader"/> with results of statement execution.</returns>
    public virtual DbDataReader ExecuteReader(ISqlCompileUnit statement)
    {
      EnsureConnectionIsOpen();
      using (var command = CreateCommand(statement)) {
        command.Prepare();
        command.Transaction = Transaction;
        return command.ExecuteReader();
      }
    }

    /// <summary>
    /// Executes the specified statement.
    /// Works similar to <see cref="SqlCommand.ExecuteNonQuery"/>.
    /// </summary>
    /// <param name="statement">The statement.</param>
    /// <returns>Number of affected rows.</returns>
    public virtual int ExecuteNonQuery(ISqlCompileUnit statement)
    {
      EnsureConnectionIsOpen();
      using (var command = CreateCommand(statement)) {
        command.Prepare();
        command.Transaction = Transaction;
        return command.ExecuteNonQuery();
      }
    }

    /// <summary>
    /// Executes the specified statement.
    /// Works similar to <see cref="SqlCommand.ExecuteScalar"/>
    /// </summary>
    /// <param name="statement">The statement.</param>
    /// <returns>The first column of the first row of executed result set.</returns>
    public virtual object ExecuteScalar(ISqlCompileUnit statement)
    {
      EnsureConnectionIsOpen();
      using (var command = CreateCommand(statement)) {
        command.Prepare();
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
    public virtual int ExecuteUpdateRequest(SqlUpdateRequest request, Tuple tuple)
    {
      EnsureConnectionIsOpen();
      using (var command = CreateUpdateCommand(request, tuple)) {
        command.Prepare();
        command.Transaction = Transaction;
        return command.ExecuteNonQuery();
      }
    }

    /// <summary>
    /// Executes the specified <see cref="SqlScalarRequest"/>.
    /// </summary>
    /// <param name="request">The request to execute.</param>
    /// <returns>The first column of the first row of executed result set.</returns>
    public virtual object ExecuteScalarRequest(SqlScalarRequest request)
    {
      EnsureConnectionIsOpen();
      using (var command = CreateScalarCommand(request)) {
        command.Prepare();
        command.Transaction = Transaction;
        return command.ExecuteScalar();
      }
    }

    /// <summary>
    /// Executes the specified <see cref="SqlFetchRequest"/>.
    /// </summary>
    /// <param name="request">The request to execute.</param>
    /// <returns><see cref="DbDataReader"/> with results of statement execution.</returns>
    public virtual DbDataReader ExecuteFetchRequest(SqlFetchRequest request)
    {
      EnsureConnectionIsOpen();
      using (var command = CreateFetchCommand(request)) {
        command.Prepare();
        command.Transaction = Transaction;
        return command.ExecuteReader();
      }
    }

    #endregion

    #region Insert, Update, Delete

    /// <inheritdoc/>
    protected override void Insert(EntityState state)
    {
      var task = new SqlRequestBuilderTask(SqlUpdateRequestKind.Insert, state.Type);
      var request = DomainHandler.RequestCache.GetValue(task, _task => DomainHandler.RequestBuilder.Build(_task));
      int rowsAffected = ExecuteUpdateRequest(request, state.Tuple);
      if (rowsAffected!=request.ExpectedResult)
        throw new InvalidOperationException(
          string.Format(Strings.ExErrorOnInsert, state.Type.Name, rowsAffected, request.ExpectedResult));
    }

    /// <inheritdoc/>
    protected override void Update(EntityState state)
    {
      var fieldStateMap = state.Tuple.Difference.GetFieldStateMap(TupleFieldState.Available);
      var task = new SqlRequestBuilderTask(SqlUpdateRequestKind.Update, state.Type, fieldStateMap);
      var request = DomainHandler.RequestCache.GetValue(task, _task => DomainHandler.RequestBuilder.Build(_task));
      int rowsAffected = ExecuteUpdateRequest(request, state.Tuple);
      if (rowsAffected!=request.ExpectedResult)
        throw new InvalidOperationException(
          string.Format(Strings.ExErrorOnUpdate, state.Type.Name, rowsAffected, request.ExpectedResult));
    }

    /// <inheritdoc/>
    protected override void Remove(EntityState state)
    {
      var task = new SqlRequestBuilderTask(SqlUpdateRequestKind.Remove, state.Type);
      var request = DomainHandler.RequestCache.GetValue(task, _task => DomainHandler.RequestBuilder.Build(_task));
      int rowsAffected = ExecuteUpdateRequest(request, state.Key.Value);
      if (rowsAffected!=request.ExpectedResult)
        throw new InvalidOperationException(
          string.Format(
            rowsAffected==0 ? Strings.ExInstanceNotFound : Strings.ExInstanceMultipleResults,
            state.Key.EntityType.Name));
    }

    #endregion

    #region CreateCommand methods

    /// <summary>
    /// Creates the <see cref="SqlCommand"/> bound to connection associated with this <see cref="SessionHandler"/>.
    /// </summary>
    /// <param name="statement">The statement.</param>
    /// <returns>A created command.</returns>
    protected virtual SqlCommand CreateCommand(ISqlCompileUnit statement)
    {
      return new SqlCommand(Connection) {Statement = statement};
    }

    /// <summary>
    /// Creates <see cref="SqlCommand"/> from specified <see cref="SqlScalarRequest"/>.
    /// </summary>
    /// <param name="request">The request.</param>
    /// <returns>A created command.</returns>
    protected virtual SqlCommand CreateScalarCommand(SqlScalarRequest request)
    {
      var command = new SqlCommand(connection);
      command.CommandText = request.Compile(DomainHandler).GetCommandText();
      return command;
    }

    /// <summary>
    /// Creates <see cref="SqlCommand"/> from specified <see cref="SqlFetchRequest"/>.
    /// </summary>
    /// <param name="request">The request.</param>
    /// <returns>A created command.</returns>
    protected virtual SqlCommand CreateFetchCommand(SqlFetchRequest request)
    {
      var command = new SqlCommand(Connection);
      var compilationResult = request.Compile(DomainHandler);
      var variantKeys = new List<object>();
      foreach (var binding in request.ParameterBindings) {
        object parameterValue = binding.ValueAccessor.Invoke();
        if ((parameterValue == null || parameterValue == DBNull.Value) && binding.SmartNull)
          variantKeys.Add(binding.ParameterReference.Parameter);
        else {
          string parameterName = compilationResult.GetParameterName(binding.ParameterReference.Parameter);
          command.Parameters.Add(CreateParameter(parameterName, parameterValue, binding.TypeMapping));
        }
      }
      command.CommandText = compilationResult.GetCommandText(variantKeys);
      return command;
    }

    /// <summary>
    /// Creates <see cref="SqlCommand"/> from specified <see cref="SqlUpdateRequest"/>.
    /// </summary>
    /// <param name="request">The request.</param>
    /// <param name="value">Tuple that contain values for update parameters.</param>
    /// <returns>A created command.</returns>
    protected virtual SqlCommand CreateUpdateCommand(SqlUpdateRequest request, Tuple value)
    {
      var command = new SqlCommand(Connection);
      var compilationResult = request.Compile(DomainHandler);

      foreach (var binding in request.ParameterBindings) {
        object parameterValue = binding.ValueAccessor.Invoke(value);
        string parameterName = compilationResult.GetParameterName(binding.ParameterReference.Parameter);
        command.Parameters.Add(CreateParameter(parameterName, parameterValue, binding.TypeMapping));
      }

      command.CommandText = compilationResult.GetCommandText();
      return command;
    }

    /// <summary>
    /// Creates the parameter with the specified <paramref name="name"/> and <paramref name="value"/>
    /// taking into account <paramref name="mapping"/>.
    /// </summary>
    /// <param name="name">The name.</param>
    /// <param name="value">The value.</param>
    /// <param name="mapping">The mapping.</param>
    /// <returns>A created parameter.</returns>
    protected virtual SqlParameter CreateParameter(string name, object value, DataTypeMapping mapping)
    {
      value = value!=null && value!=DBNull.Value && mapping.ToSqlValue!=null
        ? mapping.ToSqlValue(value)
        : (value ?? DBNull.Value);

      var typeOfValue = value.GetType();
      int length = 0;
      if (typeOfValue==typeof(string))
        length = ((string) value).Length;
      if (typeOfValue==typeof(byte[]))
        length = ((byte[]) value).Length;
      var valueType = DomainHandler.ValueTypeMapper.BuildSqlValueType(mapping, length);
      return new SqlParameter(name)
        {
          Value = value,
          DbType = mapping.DbType,
          Size = valueType.Size,
          Scale = (byte) valueType.Scale,
          Precision = (byte) valueType.Precision,
        };
    }

    #endregion

    /// <summary>
    /// Ensures the connection is open.
    /// </summary>
    public void EnsureConnectionIsOpen()
    {
      if (connection!=null && connection.State==ConnectionState.Open)
        return;
      connection = DomainHandler.SqlDriver.CreateConnection(new ConnectionInfo(Handlers.Domain.Configuration.ConnectionInfo.ToString())) as SqlConnection;
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

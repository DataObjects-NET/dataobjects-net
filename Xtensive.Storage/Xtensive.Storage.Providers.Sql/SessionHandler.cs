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
using Xtensive.Core.Disposable;
using Xtensive.Core.Tuples;
using Xtensive.Sql.Dom;
using Xtensive.Storage.Linq.Linq2Rse;
using Xtensive.Storage.Providers.Sql.Resources;

namespace Xtensive.Storage.Providers.Sql
{
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

    /// <inheritdoc/>
    public override void BeginTransaction()
    {
      EnsureConnectionIsOpen();
      if (Transaction!=null)
        throw new InvalidOperationException(Strings.TransactionIsAlreadyOpen);
      Transaction = connection.BeginTransaction();
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

    public IEnumerator<Tuple> Execute(SqlFetchRequest request)
    {
      using (var reader = ExecuteReader(request)) {
        Tuple tuple;
        while ((tuple = ReadTuple(reader, request))!=null)
          yield return tuple;
      }
    }

    protected virtual Tuple ReadTuple(DbDataReader reader, SqlFetchRequest request)
    {
      if (!reader.Read())
        return null;
      var tuple = Tuple.Create(request.RecordSetHeader.TupleDescriptor);
      request.DbDataReaderAccessor.Read(reader, tuple);
      return tuple;
    }

    public virtual int ExecuteNonQuery(ISqlCompileUnit statement)
    {
      EnsureConnectionIsOpen();
      using (var command = new SqlCommand(connection)) {
        command.Statement = statement;
        command.Prepare();
        command.Transaction = Transaction;
        return command.ExecuteNonQuery();
      }
    }

    public virtual int ExecuteNonQuery(SqlUpdateRequest request)
    {
      EnsureConnectionIsOpen();
      using (var command = new SqlCommand(connection)) {
        command.CommandText = request.CompiledStatement;
        command.Parameters.AddRange(request.ParameterBindings.Select(b => b.SqlParameter));
        command.Prepare();
        command.Transaction = Transaction;
        return command.ExecuteNonQuery();
      }
    }

    public virtual object ExecuteScalar(SqlScalarRequest request)
    {
      EnsureConnectionIsOpen();
      using (var command = new SqlCommand(connection)) {
        command.CommandText = request.CompiledStatement;
        command.Prepare();
        command.Transaction = Transaction;
        return command.ExecuteScalar();
      }
    }

    public virtual object ExecuteScalar(ISqlCompileUnit statement)
    {
      EnsureConnectionIsOpen();
      using (var command = new SqlCommand(connection)) {
        command.Statement = statement;
        command.Prepare();
        command.Transaction = Transaction;
        return command.ExecuteScalar();
      }
    }

    public virtual DbDataReader ExecuteReader(SqlFetchRequest request)
    {
      EnsureConnectionIsOpen();
      using (var command = new SqlCommand(connection)) {
        command.CommandText = request.CompiledStatement;
        command.Parameters.AddRange(request.ParameterBindings.Select(b => b.SqlParameter));
        command.Prepare();
        command.Transaction = Transaction;
        return command.ExecuteReader();
      }
    }

    /// <inheritdoc/>
    protected override void Insert(EntityState state)
    {
      var task = new SqlRequestBuilderTask(SqlUpdateRequestKind.Insert, state.Type);
      var request = DomainHandler.SqlRequestCache.GetValue(task, _task => DomainHandler.SqlRequestBuilder.Build(_task));
      request.BindParameters(state.Tuple);
      int rowsAffected = ExecuteNonQuery(request);
      if (rowsAffected!=request.ExpectedResult)
        throw new InvalidOperationException(String.Format(Strings.ExErrorOnInsert, state.Type.Name, rowsAffected, request.ExpectedResult));
    }

    /// <inheritdoc/>
    protected override void Update(EntityState state)
    {
      var task = new SqlRequestBuilderTask(SqlUpdateRequestKind.Update, state.Type, state.Tuple.Difference.GetFieldStateMap(TupleFieldState.IsAvailable));
      var request = DomainHandler.SqlRequestCache.GetValue(task, _task => DomainHandler.SqlRequestBuilder.Build(_task));
      request.BindParameters(state.Tuple);
      int rowsAffected = ExecuteNonQuery(request);
      if (rowsAffected!=request.ExpectedResult)
        throw new InvalidOperationException(String.Format(Strings.ExErrorOnUpdate, state.Type.Name, rowsAffected, request.ExpectedResult));
    }

    /// <inheritdoc/>
    protected override void Remove(EntityState state)
    {
      var task = new SqlRequestBuilderTask(SqlUpdateRequestKind.Remove, state.Type);
      var request = DomainHandler.SqlRequestCache.GetValue(task, _task => DomainHandler.SqlRequestBuilder.Build(_task));
      request.BindParameters(state.Key.Value);
      int rowsAffected = ExecuteNonQuery(request);
      if (rowsAffected!=request.ExpectedResult)
        if (rowsAffected==0)
          throw new InvalidOperationException(String.Format(Strings.ExInstanceNotFound, state.Key.Type.Name));
        else
          throw new InvalidOperationException(String.Format(Strings.ExInstanceMultipleResults, state.Key.Type.Name));
    }

    public void EnsureConnectionIsOpen()
    {
      if (connection!=null && connection.State==ConnectionState.Open)
        return;
      connection = DomainHandler.ConnectionProvider.CreateConnection(
        Handlers.Domain.Configuration.ConnectionInfo.ToString()) as SqlConnection;
      if (connection==null)
        throw new InvalidOperationException(Strings.ExUnableToCreateConnection);
      connection.Open();
    }

    /// <inheritdoc/>
    public override void Initialize()
    {
      DomainHandler = Handlers.DomainHandler as DomainHandler;
      LinqProvider = new RseQueryProvider(Session.Domain.Model);
    }

    /// <inheritdoc/>
    public override void Dispose()
    {
      connection.DisposeSafely();
    }
  }
}
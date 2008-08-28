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
using Xtensive.Sql.Dom.Dml;
using Xtensive.Storage.Model;
using Xtensive.Storage.Providers.Sql.Resources;
using SqlFactory = Xtensive.Sql.Dom.Sql;

namespace Xtensive.Storage.Providers.Sql
{
  public abstract class SessionHandler : Providers.SessionHandler
  {
    #region Nested types: ExpressionData and ExpressionHandler

    private struct ExpressionData
    {
      public SqlExpression Expression;
      public readonly Tuple Data;

      public ExpressionData(Tuple data)
      {
        Data = data;
        Expression = null;
      }
    }

    private struct ExpressionHandler : ITupleActionHandler<ExpressionData>
    {
      public bool Execute<TFieldType>(ref ExpressionData actionData, int fieldIndex)
      {
        if (actionData.Data.IsAvailable(fieldIndex) && !actionData.Data.IsNull(fieldIndex))
          actionData.Expression = SqlFactory.Literal(actionData.Data.GetValueOrDefault<TFieldType>(fieldIndex));
        return true;
      }
    }

    #endregion

    private SqlConnection connection;
    private SqlDriver driver;
    private DomainHandler domainHandler;
    private ExpressionHandler expressionHandler;

    /// <summary>
    /// Gets the connection.
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
    /// Gets the active transaction.
    /// </summary>    
    public DbTransaction Transaction { get; private set; }

    /// <summary>
    /// Gets the driver.
    /// </summary>
    public SqlDriver Driver
    {
      get
      {
        EnsureConnectionIsOpen();
        return driver;
      }
    }

    /// <inheritdoc/>
    public override void OpenTransaction()
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

    public IEnumerator<Tuple> Execute(SqlQueryRequest request)
    {
      using (DbDataReader reader = ExecuteReader(request)) {
        Tuple tuple;
        while ((tuple = ReadTuple(reader, request.ElementDescriptor))!=null)
          yield return tuple;
      }
    }

    protected virtual Tuple ReadTuple(DbDataReader reader, TupleDescriptor tupleDescriptor)
    {
      if (!reader.Read())
        return null;
      var tuple = Tuple.Create(tupleDescriptor);
      for (int i = 0; i < reader.FieldCount; i++)
        tuple.SetValue(i, DBNull.Value==reader[i] ? null : reader[i]);
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

    public virtual int ExecuteNonQuery(SqlModificationRequest request)
    {
      EnsureConnectionIsOpen();
      using (var command = new SqlCommand(connection)) {
        command.CommandText = request.CompiledStatement;
        command.Parameters.AddRange(request.GetParameters().ToArray());
        command.Prepare();
        command.Transaction = Transaction;
        return command.ExecuteNonQuery();
      }
    }

    public virtual DbDataReader ExecuteReader(SqlQueryRequest request)
    {
      EnsureConnectionIsOpen();
      using (var command = new SqlCommand(connection)) {
        command.CommandText = request.CompiledStatement;
        command.Parameters.AddRange(request.GetParameters().ToArray());
        command.Prepare();
        command.Transaction = Transaction;
        return command.ExecuteReader();
      }
    }

    /// <inheritdoc/>
    protected override void Insert(EntityData data)
    {
      SqlBatch batch = SqlFactory.Batch();
      SqlModificationRequest request = new SqlModificationRequest(batch);
      int j = 0;
      foreach (IndexInfo primaryIndex in data.Type.AffectedIndexes.Where(i => i.IsPrimary)) {
        SqlTableRef tableRef = SqlFactory.TableRef(domainHandler.GetTable(primaryIndex));
        SqlInsert insert = SqlFactory.Insert(tableRef);
        for (int i = 0; i < primaryIndex.Columns.Count; i++) {
          ColumnInfo column = primaryIndex.Columns[i];
          int offset = data.Type.Fields[column.Field.Name].MappingInfo.Offset;
          SqlParameter p = new SqlParameter("p" + j++);
          request.ParameterBindings[p] = (target => data.Tuple.IsNull(offset) ? DBNull.Value : data.Tuple.GetValue(offset));
          insert.Values[tableRef[i]] = SqlFactory.ParameterRef(p);
        }
        batch.Add(insert);
      }
      request.CompileWith(Driver);
      request.BindTo(data.Tuple);
      int rowsAffected = ExecuteNonQuery(request);
      if (rowsAffected!=batch.Count)
        throw new InvalidOperationException(String.Format(Strings.ExErrorOnInsert, data.Type.Name, rowsAffected, batch.Count));
    }

    /// <inheritdoc/>
    protected override void Update(EntityData data)
    {
      SqlBatch batch = SqlFactory.Batch();
      foreach (IndexInfo primaryIndex in data.Type.AffectedIndexes.Where(i => i.IsPrimary)) {
        var domainHandler1 = (DomainHandler) Handlers.DomainHandler;
        SqlTableRef tableRef = SqlFactory.TableRef(domainHandler1.GetTable(primaryIndex));
        SqlUpdate update = SqlFactory.Update(tableRef);

        for (int i = 0; i < primaryIndex.Columns.Count; i++) {
          ColumnInfo column = primaryIndex.Columns[i];
          int offset = data.Type.Fields[column.Field.Name].MappingInfo.Offset;
          var expressionData = new ExpressionData(data.Tuple);
          data.Tuple.Descriptor.Execute(expressionHandler, ref expressionData, offset);
          if (!SqlExpression.IsNull(expressionData.Expression))
            update.Values[tableRef[i]] = expressionData.Expression;
        }
        SqlExpression where = null;
        for (int i = 0; i < data.Type.Indexes.PrimaryIndex.KeyColumns.Count; i++) {
          var expressionData = new ExpressionData(data.Key.Tuple);
          data.Tuple.Descriptor.Execute(expressionHandler, ref expressionData, i);
          if (!SqlExpression.IsNull(expressionData.Expression)) {
            SqlBinary binary = tableRef[i]==expressionData.Expression;
            where = SqlExpression.IsNull(where) ? binary : where & binary;
          }
        }
        update.Where = where;
        batch.Add(update);
      }
      int rowsAffected = ExecuteNonQuery(batch);
      if (rowsAffected!=batch.Count)
        throw new InvalidOperationException(String.Format(Strings.ExErrorOnUpdate, data.Type.Name, rowsAffected, batch.Count));
    }

    /// <inheritdoc/>
    protected override void Remove(EntityData data)
    {
      SqlBatch batch = SqlFactory.Batch();
      int tableCount = 0;
      foreach (IndexInfo index in data.Type.AffectedIndexes.Where(i => i.IsPrimary)) {
        var domainHandler1 = (DomainHandler) Handlers.DomainHandler;
        SqlTableRef tableRef = SqlFactory.TableRef(domainHandler1.GetTable(index));
        SqlDelete delete = SqlFactory.Delete(tableRef);
        SqlExpression where = null;
        for (int i = 0; i < data.Type.Indexes.PrimaryIndex.KeyColumns.Count; i++) {
          var expressionData = new ExpressionData(data.Key.Tuple);
          data.Tuple.Descriptor.Execute(expressionHandler, ref expressionData, i);
          if (!SqlExpression.IsNull(expressionData.Expression)) {
            SqlBinary binary = tableRef[i]==expressionData.Expression;
            where = SqlExpression.IsNull(where) ? binary : where & binary;
          }
        }
        delete.Where = where;
        batch.Add(delete);
        tableCount++;
      }
      int rowsAffected = ExecuteNonQuery(batch);
      if (rowsAffected!=tableCount)
        if (rowsAffected==0)
          throw new InvalidOperationException(String.Format(Strings.ExInstanceNotFound, data.Key.Type.Name));
        else
          throw new InvalidOperationException(String.Format(Strings.ExInstanceMultipleResults, data.Key.Type.Name));
    }

    public void EnsureConnectionIsOpen()
    {
      if (connection==null || connection.State!=ConnectionState.Open) {
        connection = ((DomainHandler) Handlers.DomainHandler).ConnectionProvider.CreateConnection(Handlers.Domain.Configuration.ConnectionInfo.ToString()) as SqlConnection;
        if (connection==null)
          throw new InvalidOperationException(Strings.ExUnableToCreateConnection);
        connection.Open();
        driver = connection.Driver;
      }
    }

    /// <inheritdoc/>
    public override void Initialize()
    {
      domainHandler = Handlers.DomainHandler as DomainHandler;
    }

    /// <inheritdoc/>
    public override void Dispose()
    {
      connection.DisposeSafely();
    }
  }
}
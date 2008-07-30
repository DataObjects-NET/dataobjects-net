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
using Xtensive.Core.Tuples;
using Xtensive.Sql.Dom;
using Xtensive.Sql.Dom.Database;
using Xtensive.Sql.Dom.Dml;
using Xtensive.Storage.Model;
using Xtensive.Storage.Providers.Sql.Resources;
using Xtensive.Storage.Rse;
using Xtensive.Storage.Rse.Providers.Compilable;
using SqlFactory = Xtensive.Sql.Dom.Sql;

namespace Xtensive.Storage.Providers.Sql
{
  public class SessionHandler : Providers.SessionHandler
  {
    private SqlConnection connection;
    private DbTransaction transaction;
    private ExpressionHandler expressionHandler;

    #region Helper structs & methods

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

    public SqlConnection Connection
    {
      get
      {
        EnsureConnectionIsOpen();
        return connection;
      }
    }

    public DbTransaction Transaction
    {
      get
      {
        EnsureConnectionIsOpen();
        return transaction;
      }
    }

    /// <inheritdoc/>
    protected override void Insert(EntityData data)
    {
      SqlBatch batch = SqlFactory.Batch();
      foreach (IndexInfo primaryIndex in data.Type.AffectedIndexes.Where(i => i.IsPrimary)) {
        SqlTableRef tableRef = GetTableRef(primaryIndex);
        SqlInsert insert = SqlFactory.Insert(tableRef);

        for (int i = 0; i < primaryIndex.Columns.Count; i++) {
          ColumnInfo column = primaryIndex.Columns[i];
          int offset = data.Type.Fields[column.Field.Name].MappingInfo.Offset;
          var expressionData = new ExpressionData(data.Tuple);
          data.Tuple.Descriptor.Execute(expressionHandler, ref expressionData, offset);
          if (!SqlExpression.IsNull(expressionData.Expression))
            insert.Values[tableRef[i]] = expressionData.Expression;
        }
        batch.Add(insert);
      }
      int rowsAffected = ExecuteNonQuery(batch);
      if (rowsAffected!=batch.Count)
        throw new InvalidOperationException(String.Format(Strings.ExInsertInvalid, data.Type.Name, rowsAffected, batch.Count));
    }

    /// <inheritdoc/>
    protected override void Update(EntityData data)
    {
      SqlBatch batch = SqlFactory.Batch();
      foreach (IndexInfo primaryIndex in data.Type.AffectedIndexes.Where(i => i.IsPrimary)) {
        SqlTableRef tableRef = GetTableRef(primaryIndex);
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
        throw new InvalidOperationException(String.Format(Strings.ExUpdateInvalid, data.Type.Name, rowsAffected, batch.Count));
    }

    /// <inheritdoc/>
    protected override void Remove(EntityData data)
    {
      SqlBatch batch = SqlFactory.Batch();
      int tableCount = 0;
      foreach (IndexInfo index in data.Type.AffectedIndexes.Where(i => i.IsPrimary)) {
        SqlTableRef tableRef = GetTableRef(index);
        SqlDelete delete = SqlFactory.Delete(tableRef);
        SqlExpression where = null;
        for (int i = 0; i < data.Type.Indexes.PrimaryIndex.KeyColumns.Count; i++) {
          var expressionData = new ExpressionData(data.Key.Tuple);
          data.Tuple.Descriptor.Execute(expressionHandler, ref expressionData, i);
          if (!SqlExpression.IsNull(expressionData.Expression))
          {
            SqlBinary binary = tableRef[i] == expressionData.Expression;
            where = SqlExpression.IsNull(where) ? binary : where & binary;
          }
        }
        delete.Where = where;
        batch.Add(delete);
        tableCount++;
      }
      int rowsAffected = ExecuteNonQuery(batch);
      if (rowsAffected != tableCount)
        if (rowsAffected==0)
          throw new InvalidOperationException(String.Format(Strings.ExInstanceNotFound, data.Key.Type.Name));
        else
          throw new InvalidOperationException(String.Format(Strings.ExInstanceMultipleResults, data.Key.Type.Name));
    }

    /// <inheritdoc/>
    public override Tuple Fetch(IndexInfo index, Key key, IEnumerable<ColumnInfo> columns)
    {
      EnsureConnectionIsOpen();
      var rs = new IndexProvider(index).Result;
      rs.Range(key.Tuple, key.Tuple);
//      rs.Select(columns.Select(c => c.Name).ToArray());
      var enumerator = rs.GetEnumerator();
      if (enumerator.MoveNext())
        return enumerator.Current;

      /*var provider = new IndexProvider(primaryIndex);
      var compiled = (SqlProvider)provider.Compiled;
      var queryRef = SqlFactory.QueryRef(compiled.Query);
      SqlSelect select = SqlFactory.Select(queryRef);
      IEnumerable<SqlColumn> columnsToAdd = queryRef.Columns.Cast<SqlColumn>().Where(sqlColumn => columns.Any(columnInfo => columnInfo.Name==sqlColumn.Name));
      foreach (SqlColumn column in columnsToAdd)
        select.Columns.Add(column);

      select.Where = DomainHandler.GetWhereStatement(select.Columns[0].SqlTable, GetStatementMapping(index, columnInfo => columnInfo.IsPrimaryKey), key);
      using (DbDataReader reader = ExecuteReader(select)) {
        if (reader.RecordsAffected > 1)
          throw new InvalidOperationException(Strings.ExQueryMultipleResults);
        if (reader.Read()) {
          Tuple tuple = GetTuple(reader, select);
          return tuple;
        }
        return null;
      }*/
      throw new NotImplementedException();
    }


//    private SqlSelect BuildQuery(IndexInfo index, IEnumerable<ColumnInfo> columns)
//    {
//      var fullQuery = DomainHandler.BuildQueryInternal(index);
//      IEnumerable<SqlColumn> columnsToRemove = fullQuery.Columns.Where(sqlColumn => !columns.Any(columnInfo=>columnInfo.Name==sqlColumn.Name));
//      foreach (SqlColumn column in columnsToRemove)
//        fullQuery.Columns.Remove(column);
//      return fullQuery;
//    }

    /// <inheritdoc/>
    public override RecordSet Select(IndexInfo index)
    {
      var provider = new IndexProvider(index);
      return provider.Result;
    }

    /// <inheritdoc/>
    public override void Commit()
    {
      base.Commit();
      if (transaction!=null) {
        transaction.Commit();
        connection.Close();
        transaction = null;
        connection = null;
      }
    }

    #region Internals

    internal int  ExecuteNonQuery(ISqlCompileUnit statement)
    {
      EnsureConnectionIsOpen();
      using (var command = new SqlCommand(connection)) {
        command.Statement = statement;
        command.Prepare();
        command.Transaction = transaction;
        return command.ExecuteNonQuery();
      }
    }

    internal DbDataReader ExecuteReader(ISqlCompileUnit statement)
    {
      EnsureConnectionIsOpen();
      using (var command = new SqlCommand(connection)) {
        command.Statement = statement;
        command.Prepare();
        command.Transaction = transaction;
        return command.ExecuteReader();
      }
    }

    internal DomainHandler DomainHandler
    {
      get { return ((DomainHandler)Handlers.DomainHandler); }
    }

    #endregion

    private Tuple GetTuple(IDataRecord reader, SqlSelect select)
    {
      var typeId = (int) reader[Handlers.NameBuilder.TypeIdFieldName];
      TypeInfo actualType = Handlers.Domain.Model.Types[typeId];
      Tuple result = Tuple.Create(actualType.TupleDescriptor);
      for (int i = 0; i < actualType.Columns.Count; i++) {
        ColumnInfo column = actualType.Columns[i];
        int ordinal = select.Columns.IndexOf(select.Columns[column.Name]);
        if (ordinal >= 0)
          result.SetValue(column.Field.MappingInfo.Offset, reader[ordinal]);
      }
      return result;
    }

    private SqlTableRef GetTableRef(IndexInfo index)
    {
      Table table;
      if (!DomainHandler.RealIndexes.TryGetValue(index, out table))
        throw new InvalidOperationException(String.Format(Strings.ExTypeDoesntHavePrimaryIndex, index.Name));
      return SqlFactory.TableRef(table);
    }

    private void EnsureConnectionIsOpen()
    {
      if (connection==null || transaction==null || connection.State!=ConnectionState.Open) {
        var provider = new SqlConnectionProvider();
        connection = provider.CreateConnection(Handlers.Domain.Configuration.ConnectionInfo.ToString()) as SqlConnection;
        if (connection==null)
          throw new InvalidOperationException(Strings.ExUnableToCreateConnection);
        connection.Open();
        transaction = connection.BeginTransaction();
      }
    }
  }
}

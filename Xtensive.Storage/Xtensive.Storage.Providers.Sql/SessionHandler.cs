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
using Xtensive.Core.Collections;
using Xtensive.Core.Tuples;
using Xtensive.Sql.Dom;
using Xtensive.Sql.Dom.Database;
using Xtensive.Sql.Dom.Dml;
using Xtensive.Storage.Model;
using Xtensive.Storage.Providers.Sql.Resources;
using Xtensive.Storage.Rse;
using System.Linq;
using Xtensive.Indexing;
using Xtensive.Storage.Rse.Providers.Declaration;

namespace Xtensive.Storage.Providers.Sql
{
  public class SessionHandler : Providers.SessionHandler
  {
    private SqlConnection connection;
    private DbTransaction transaction;
    private readonly Dictionary<IndexInfo, long> indexVersions = new Dictionary<IndexInfo, long>();

    /// <inheritdoc/>
    public override void Insert(EntityData data)
    {
      EnsureConnection();
      IncreaseVersion(data.Type.AffectedIndexes);
      SqlBatch batch = Xtensive.Sql.Dom.Sql.Batch();
      foreach (IndexInfo primaryIndex in GetParentPrimaryIndexes(data.Type.Indexes.PrimaryIndex)) {
        SqlTableRef tableRef = GetTableRef(primaryIndex);
        SqlInsert insert = Xtensive.Sql.Dom.Sql.Insert(tableRef);
        foreach (Pair<int, SqlExpression> pair in SetValues(primaryIndex, data.Tuple, data.Type)) {
          insert.Values[tableRef[pair.First]] = pair.Second;
        }
        batch.Add(insert);
      }
      int rowsAffected = ExecuteNonQuery(batch);
      if (rowsAffected!=batch.Count)
        throw new InvalidOperationException(String.Format(Strings.ExInsertInvalid, data.Type.Name, rowsAffected, batch.Count));
    }

    /// <inheritdoc/>
    public override Tuple Fetch(Key key, IEnumerable<ColumnInfo> columns)
    {
      EnsureConnection();
      IndexInfo primaryIndex = key.Type.Indexes.PrimaryIndex;
      SqlSelect select = BuildQuery(primaryIndex, columns);
      select.Where = DomainHandler.GetWhereStatement(select.Columns[0].SqlTable, GetStatementMapping(primaryIndex, columnInfo => columnInfo.IsPrimaryKey), key);
      using (DbDataReader reader = ExecuteReader(select)) {
        if (reader.RecordsAffected > 1)
          throw new InvalidOperationException(Strings.ExQueryMultipleResults);
        if (reader.Read()) {
          Tuple tuple = GetTuple(reader, select);
          return tuple;
        }
        return null;
      }
    }

    private static IEnumerable<StatementMapping> GetStatementMapping(IndexInfo index, Func<ColumnInfo, bool> predicate)
    {
      return index.Columns.Where(predicate).Select((columnInfo, columnIndex) => new StatementMapping(columnIndex, columnInfo.Field.MappingInfo.Offset));
    }

    private SqlSelect BuildQuery(IndexInfo index, IEnumerable<ColumnInfo> columns)
    {
      var fullQuery = DomainHandler.BuildQueryInternal(index);
      IEnumerable<SqlColumn> columnsToRemove = fullQuery.Columns.Where(sqlColumn => !columns.Any(columnInfo=>columnInfo.Name==sqlColumn.Name));
      foreach (SqlColumn column in columnsToRemove)
        fullQuery.Columns.Remove(column);
      return fullQuery;
    }

    /// <inheritdoc/>
    public override void Remove(EntityData data)
    {
      EnsureConnection();
      IncreaseVersion(data.Key.Type.AffectedIndexes);
      SqlBatch batch = Xtensive.Sql.Dom.Sql.Batch();
      int tableCount = 0;
      foreach (IndexInfo index in GetParentPrimaryIndexes(data.Key.Type.Indexes.PrimaryIndex)) {
        SqlTableRef tableRef = GetTableRef(index);
        SqlDelete delete = Xtensive.Sql.Dom.Sql.Delete(tableRef);
        delete.Where = DomainHandler.GetWhereStatement(tableRef, GetStatementMapping(data.Key.Type.Indexes.PrimaryIndex, columnInfo=>columnInfo.IsPrimaryKey), data.Key);
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

    /// <inheritdoc/>
    public override void Update(EntityData data)
    {
      EnsureConnection();
      SqlBatch batch = Xtensive.Sql.Dom.Sql.Batch();
      IncreaseVersion(data.Type.AffectedIndexes);
      foreach (IndexInfo primaryIndex in GetRealPrimaryIndexes(data.Type.Indexes.PrimaryIndex)) {
        SqlTableRef tableRef = GetTableRef(primaryIndex);
        SqlUpdate update = Xtensive.Sql.Dom.Sql.Update(tableRef);
        foreach (Pair<int, SqlExpression> pair in SetValues(primaryIndex, data.Tuple, data.Type)) {
          update.Values[tableRef[pair.First]] = pair.Second;
        }
        update.Where = DomainHandler.GetWhereStatement(tableRef, GetStatementMapping(data.Type.Indexes.PrimaryIndex, columnInfo => columnInfo.IsPrimaryKey), data.Key);
        batch.Add(update);
      }
      int rowsAffected = ExecuteNonQuery(batch);
      if (rowsAffected!=batch.Count)
        throw new InvalidOperationException(String.Format(Strings.ExUpdateInvalid, data.Type.Name, rowsAffected, batch.Count));
    }

    /// <inheritdoc/>
    public override IEnumerable<Tuple> Select(TypeInfo type, IEnumerable<ColumnInfo> columns)
    {
      EnsureConnection();
      var results = new List<Tuple>();
      IndexInfo primaryIndex = type.Indexes.PrimaryIndex;
      SqlSelect select = BuildQuery(primaryIndex, columns);
      using (DbDataReader reader = ExecuteReader(select)) {
        while (reader.Read())
          results.Add(GetTuple(reader, select));
      }
      return results;
    }

    /// <inheritdoc/>
    public override RecordSet QueryIndex(IndexInfo info)
    {
      var provider = new IndexProvider(info);
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

    internal long GetIndexVersion(IndexInfo indexInfo)
    {
      long result;
      indexVersions.TryGetValue(indexInfo, out result);
      return result;
    }

    internal int ExecuteNonQuery(ISqlCompileUnit statement)
    {
      EnsureConnection();
      using (var command = new SqlCommand(connection)) {
        command.Statement = statement;
        command.Prepare();
        command.Transaction = transaction;
        return command.ExecuteNonQuery();
      }
    }

    internal DbDataReader ExecuteReader(ISqlCompileUnit statement)
    {
      EnsureConnection();
      using (var command = new SqlCommand(connection)) {
        command.Statement = statement;
        command.Prepare();
        command.Transaction = transaction;
        return command.ExecuteReader();
      }
    }

    internal DomainHandler DomainHandler
    {
      get { return ((DomainHandler)Session.Domain.Handler); }
    }

    #endregion

    private void IncreaseVersion(IEnumerable<IndexInfo> indexes)
    {
      foreach (IndexInfo index in indexes) {
        long currentVersion;
        indexVersions.TryGetValue(index, out currentVersion);
        indexVersions[index] = currentVersion + 1;
      }
    }

    private Tuple GetTuple(IDataRecord reader, SqlSelect select)
    {
      var typeId = (int) reader[Session.Domain.NameProvider.TypeId];
      TypeInfo actualType = DomainHandler.Domain.Model.Types[typeId];
      Tuple result = Tuple.Create(actualType.TupleDescriptor);
      for (int i = 0; i < actualType.Columns.Count; i++) {
        ColumnInfo column = actualType.Columns[i];
        int ordinal = select.Columns.IndexOf(select.Columns[column.Name]);
        if (ordinal>=0) 
          result.SetValue(column.Field.MappingInfo.Offset, reader[ordinal]);
      }
      return result;
    }

    private SqlTableRef GetTableRef(IndexInfo index)
    {
      var handler = (DomainHandler) Session.Domain.Handler;
      Table table;
      if (!handler.RealIndexes.TryGetValue(index, out table))
        throw new InvalidOperationException(String.Format(Strings.ExTypeDoesntHavePrimaryIndex, index.Name));
      return Xtensive.Sql.Dom.Sql.TableRef(table);
    }

    private void EnsureConnection()
    {
      if (connection==null || transaction==null || connection.State!=ConnectionState.Open) {
        var provider = new SqlConnectionProvider();
        connection = provider.CreateConnection(DomainHandler.Domain.Configuration.ConnectionInfo.ToString()) as SqlConnection;
        if (connection==null)
          throw new InvalidOperationException(Strings.ExUnableToCreateConnection);
        connection.Open();
        transaction = connection.BeginTransaction();
      }
    }

    private IEnumerable<Pair<int, SqlExpression>> SetValues(IndexInfo index, Tuple data, TypeInfo originalType)
    {
      var result = new List<Pair<int, SqlExpression>>();
      for (int i = 0; i < index.Columns.Count; i++) {
        ColumnInfo column = index.Columns[i];
        int offset = originalType.Columns[column.Name].Field.MappingInfo.Offset;
        SqlExpression expression = DomainHandler.GetSqlExpression(data, offset);
        if (expression!=null)
          result.Add(new Pair<int, SqlExpression>(i, expression));
      }
      return result;
    }

    private static IEnumerable<IndexInfo> GetParentPrimaryIndexes(IndexInfo indexInfo)
    {
      IEnumerable<TypeInfo> baseTypes = indexInfo.ReflectedType.GetAncestors().Union(Enumerable.Repeat(indexInfo.ReflectedType, 1));
      return baseTypes.Select(typeInfo => typeInfo.Indexes.FindFirst(IndexAttributes.Real | IndexAttributes.Primary));
    }

    private static IEnumerable<IndexInfo> GetRealPrimaryIndexes(IndexInfo indexInfo)
    {
      if (indexInfo.IsPrimary && !indexInfo.IsVirtual) {
        yield return indexInfo;
      }
      else {
        foreach (IndexInfo baseIndex in indexInfo.BaseIndexes) {
          foreach (IndexInfo realPrimaryIndex in GetRealPrimaryIndexes(baseIndex)) {
            yield return realPrimaryIndex;
          }
        }
      }
    }


    
  }
}
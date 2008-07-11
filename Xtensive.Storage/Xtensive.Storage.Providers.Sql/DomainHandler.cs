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
using Xtensive.Core.Collections;
using Xtensive.Core.Tuples;
using Xtensive.Sql.Common;
using Xtensive.Sql.Dom;
using Xtensive.Sql.Dom.Database;
using Xtensive.Sql.Dom.Database.Providers;
using Xtensive.Sql.Dom.Dml;
using Xtensive.Storage.Model;
using ColumnInfo=Xtensive.Storage.Model.ColumnInfo;
using IndexInfo=Xtensive.Storage.Model.IndexInfo;
using Xtensive.Storage.Providers.Sql.Resources;

namespace Xtensive.Storage.Providers.Sql
{
  public abstract class DomainHandler : Storage.Providers.DomainHandler
  {
    private DbTransaction transaction;
    private Catalog catalog;
    private readonly Dictionary<IndexInfo, Table> realIndexes = new Dictionary<IndexInfo, Table>();
    private SqlConnection connection;
    private Xtensive.Sql.Dom.Database.Model model;

    /// <inheritdoc/>
    public override void Build()
    {
      var provider = new SqlConnectionProvider();
      using (connection = provider.CreateConnection(ExecutionContext.Configuration.ConnectionInfo.ToString()) as SqlConnection) {
        if (connection==null)
          throw new InvalidOperationException(Strings.ExUnableToCreateConnection);
        connection.Open();
        var modelProvider = new SqlModelProvider(connection);
        model = Xtensive.Sql.Dom.Database.Model.Build(modelProvider);
        string catalogName = ExecutionContext.Configuration.ConnectionInfo.Resource;
        catalog = model.DefaultServer.Catalogs[catalogName];
        using (transaction = connection.BeginTransaction()) {
          ClearCatalog();
          BuildCatalog();
          transaction.Commit();
        }
        model = Xtensive.Sql.Dom.Database.Model.Build(modelProvider);
        catalog = model.DefaultServer.Catalogs[catalogName];
      }
    }

    /// <summary>
    /// Gets <see cref="SqlDataType"/> by .NET <see cref="Type"/> and length.
    /// </summary>
    /// <param name="type"></param>
    /// <param name="length"></param>
    /// <returns></returns>
    protected abstract SqlDataType GetSqlDataType(Type type, int? length);

    /// <summary>
    /// Builds <see cref="SqlSelect"/> by index.
    /// </summary>
    /// <param name="indexInfo">Index to build query for.</param>
    protected virtual SqlSelect BuildQuery(IndexInfo indexInfo)
    {
      QueryBuildResult buildResult = BuildQueryParts(indexInfo);
      SqlSelect result = Xtensive.Sql.Dom.Sql.Select(buildResult.Table);
      result.Where = buildResult.Expression;
      return result;
    }

    /// <summary>
    /// Builds "Where" expression for key for specified table.
    /// </summary>
    /// <param name="table">Table to build statement for.</param>
    /// <param name="mapping">Table-to-tuple mapping</param>
    /// <param name="key">Key data</param>
    /// <returns>Expression that can be used in "where" statements</returns>
    public virtual SqlExpression GetWhereStatement(SqlTable table, IEnumerable<StatementMapping> mapping, Key key)
    {
      if (key==null)
        return null;
      SqlExpression expression = null;
      foreach (StatementMapping statementMapping in mapping) {
        SqlBinary binary = (table[statementMapping.TablePosition] == GetSqlExpression(key.Tuple, statementMapping.TuplePosition));
        if (expression == null)
          expression = binary;
        else
          expression &= binary;
      }
      return expression;
    }

    public virtual SqlExpression GetSqlExpression(Tuple tuple, int index)
    {
      if (!tuple.IsAvailable(index) || tuple.IsNull(index) || tuple.GetValueOrDefault(index) == null)
        return null;
      Type type = tuple.Descriptor[index];
      if (type == typeof(Boolean))
        return tuple.GetValue<bool>(index);
      if (type == typeof(SByte))
        return tuple.GetValue<SByte>(index);
      if (type == typeof(Byte))
        return tuple.GetValue<Byte>(index);
      if (type == typeof(Int16))
        return tuple.GetValue<Int16>(index);
      if (type == typeof(UInt16))
        return tuple.GetValue<UInt16>(index);
      if (type == typeof(Int32))
        return tuple.GetValue<Int32>(index);
      if (type == typeof(UInt32))
        return tuple.GetValue<UInt32>(index);
      if (type == typeof(Int64))
        return tuple.GetValue<Int64>(index);
      if (type == typeof(UInt64))
        return tuple.GetValue<UInt64>(index);
      if (type == typeof(Decimal))
        return tuple.GetValue<Decimal>(index);
      if (type == typeof(float))
        return tuple.GetValue<float>(index);
      if (type == typeof(double))
        return tuple.GetValue<double>(index);
      if (type == typeof(DateTime))
        return tuple.GetValue<DateTime>(index);
      if (type == typeof(String))
        return tuple.GetValue<String>(index);
      if (type == typeof(byte[]))
        return tuple.GetValue<byte[]>(index);
      if (type == typeof(Guid))
        return tuple.GetValue<Guid>(index);
      throw new InvalidOperationException(); //Should never be
    }

    #region Internal

    internal Dictionary<IndexInfo, Table> RealIndexes
    {
      get { return realIndexes; }
    }

    internal Catalog Catalog
    {
      get { return catalog; }
    }

    internal SqlSelect BuildQueryInternal(IndexInfo indexInfo)
    {
      return BuildQuery(indexInfo);
    }

    #endregion

    #region Private

    private QueryBuildResult BuildQueryParts(IndexInfo index)
    {
      if (index.IsVirtual) {
        if ((index.Attributes & IndexAttributes.Union) > 0)
          return BuldUnionQuery(index);
        if ((index.Attributes & IndexAttributes.Join) > 0)
          return BuildJoinQuery(index);
        if ((index.Attributes & IndexAttributes.Filtered) > 0)
          return BuildFilteredQuery(index);
        throw new NotSupportedException(String.Format(Strings.ExUnsupportedIndex, index.Name, index.Attributes));
      }
      SqlTableRef tableRef = GetTableRef(index);
      return new QueryBuildResult(tableRef, null, tableRef, GetSqlColumns(index.Columns, tableRef));
    }

    private QueryBuildResult BuildFilteredQuery(IndexInfo index)
    {
      QueryBuildResult buildResult = BuildQueryParts(index.BaseIndexes[0]);
      IEnumerable<TypeInfo> descendants = index.ReflectedType.GetDescendants(true).Union(Enumerable.Repeat(index.ReflectedType, 1));
      IEnumerable<int> descendantTypes = descendants.Convert(typeInfo => typeInfo.TypeId);
      int[] typeIds = descendantTypes.ToArray();
      SqlTableColumn typeIdColumn = buildResult.Table.Columns[ExecutionContext.NameProvider.TypeId];
      SqlArray<int> typIdValues = Xtensive.Sql.Dom.Sql.Array(typeIds);
      SqlBinary inQuery = Xtensive.Sql.Dom.Sql.In(typeIdColumn, typIdValues);
      SqlExpression expression = CombineExpression(buildResult.Expression, inQuery);
      return new QueryBuildResult(buildResult.Table, expression, buildResult.PrimaryTable, buildResult.Columns);
    }

    private QueryBuildResult BuildJoinQuery(IndexInfo index)
    {
      SqlTable table = null;
      SqlExpression expression = null;
      SqlTable primaryTable = null;
      IEnumerable<SqlColumn> columns = null;
      foreach (IndexInfo baseIndex in index.BaseIndexes) {
        QueryBuildResult baseTable = BuildQueryParts(baseIndex);
        if (table == null) {
          table = baseTable.Table;
          expression = baseTable.Expression;
          primaryTable = baseTable.PrimaryTable;
          columns = baseTable.Columns;
        }
        else {
          int keyColumnCount = baseIndex.Columns.Count(colunInfo => colunInfo.IsPrimaryKey || colunInfo.IsSystem);
          SqlExpression joinExpression = null;
          for (int i = 0; i < keyColumnCount; i++) {
            SqlBinary binary = (baseTable.Table.Columns[i] == primaryTable.Columns[i]);
            if (joinExpression == null)
              joinExpression = binary;
            else
              joinExpression &= binary;
          }
          table = table.LeftOuterJoin(baseTable.Table, joinExpression);
          expression = CombineExpression(expression, baseTable.Expression);
          IEnumerable<SqlColumn> nonKeyColumns = baseTable.Columns.Skip(baseIndex.KeyColumns.Count);
          IEnumerable<SqlColumn> dataColumns = nonKeyColumns.Where(sqlColumn => sqlColumn.Name != ExecutionContext.NameProvider.TypeId);
          columns = columns.Union(dataColumns);
        }
      }
      return new QueryBuildResult(table, expression, primaryTable, columns);
    }

    private QueryBuildResult BuldUnionQuery(IndexInfo index)
    {
      SqlTable table = null;
      SqlExpression expression = null;
      SqlTable primaryTable = null;
      IEnumerable<SqlColumn> columns = null;
      foreach (IndexInfo baseIndex in index.BaseIndexes) {
        QueryBuildResult baseTable = BuildQueryParts(baseIndex);
        if (table == null) {
          table = baseTable.Table;
          expression = baseTable.Expression;
          primaryTable = baseTable.PrimaryTable;
          columns = baseTable.Columns;
        }
        else {
          table = table.UnionJoin(baseTable.Table);
          expression = CombineExpression(expression, baseTable.Expression);
        }
      }
      return new QueryBuildResult(table, expression, primaryTable, columns);
    }

    private IEnumerable<SqlColumn> GetSqlColumns(IEnumerable<ColumnInfo> modelColumns, SqlTable table)
    {
      List<SqlColumn> result = new List<SqlColumn>();
      foreach (var modelColumn in modelColumns) {
        result.Add(table.Columns[modelColumn.Name]);
      }
      return result;
    }

    private static SqlExpression CombineExpression(SqlExpression expression1, SqlExpression expression2)
    {
      if (expression1 == null)
        return expression2;
      return expression1 & expression2;
    }

    private SqlTableRef GetTableRef(IndexInfo index)
    {
      Table table = catalog.DefaultSchema.Tables[index.ReflectedType.Name];
      return Xtensive.Sql.Dom.Sql.TableRef(table);
    }

    #endregion


    private void ExecuteNonQuery(ISqlCompileUnit statement)
    {
      using (var command = new SqlCommand(connection)) {
        command.Statement = statement;
        command.Prepare();
        command.Transaction = transaction;
        command.ExecuteNonQuery();
      }
    }

    private void BuildCatalog()
    {
      SqlBatch batch = Xtensive.Sql.Dom.Sql.Batch();
      // Build tables
      foreach (TypeInfo type in ExecutionContext.Model.Types) {
        IndexInfo primaryIndex = type.Indexes.FindFirst(IndexAttributes.Real | IndexAttributes.Primary);
        if (primaryIndex!=null && !realIndexes.ContainsKey(primaryIndex)) {
          Table table = catalog.DefaultSchema.CreateTable(primaryIndex.ReflectedType.Name);
          realIndexes.Add(primaryIndex, table);
          var keyColumns = new List<TableColumn>();
          foreach (ColumnInfo column in primaryIndex.Columns) {
            TableColumn tableColumn = table.CreateColumn(column.Name, GetSqlType(column.ValueType, column.Length));
            tableColumn.IsNullable = column.IsNullable;
            if (column.IsPrimaryKey)
              keyColumns.Add(tableColumn);
          }
          table.CreatePrimaryKey(primaryIndex.Name, keyColumns.ToArray());
          batch.Add(Xtensive.Sql.Dom.Sql.Create(table));
          // Secondary indexes
          foreach (IndexInfo secondaryIndex in type.Indexes.Find(IndexAttributes.Real).Where(indexInfo => !indexInfo.IsPrimary)) {
            Index index = table.CreateIndex(secondaryIndex.Name);
            index.IsUnique = secondaryIndex.IsUnique;
            // TODO: index.FillFactor = secondaryIndex.FillFactor;
            index.Filegroup = "\"default\"";
            batch.Add(Xtensive.Sql.Dom.Sql.Create(index));
            foreach (ColumnInfo column in secondaryIndex.Columns.Where(columnInfo => !columnInfo.IsPrimaryKey && !columnInfo.IsSystem)) {
              index.CreateIndexColumn(table.TableColumns.First(tableColumn => tableColumn.Name==column.Name));
            }
          }
        }
      }
      if (batch.Count > 0)
        ExecuteNonQuery(batch);
    }

    private SqlValueType GetSqlType(Type type, int? length)
    {
      // TODO: Get this data from Connection.Driver.ServerInfo.DataTypes
      var result = (length==null)
        ? new SqlValueType(GetSqlDataType(type, null))
        : new SqlValueType(GetSqlDataType(type, length.Value), length.Value);
      return result;
    }

    private void ClearCatalog()
    {
      SqlBatch batch = Xtensive.Sql.Dom.Sql.Batch();
      Schema schema = catalog.DefaultSchema;
      foreach (View view in schema.Views)
        batch.Add(Xtensive.Sql.Dom.Sql.Drop(view));
      schema.Views.Clear();
      foreach (Table table in schema.Tables)
        batch.Add(Xtensive.Sql.Dom.Sql.Drop(table));
      schema.Tables.Clear();
      if (batch.Count > 0)
        ExecuteNonQuery(batch);
    }
  }
}
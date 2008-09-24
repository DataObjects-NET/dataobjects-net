// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: 
// Created:    2008.08.28

using System;
using Xtensive.Core.Internals.DocTemplates;
using Xtensive.Core.Tuples;
using Xtensive.Sql.Dom;
using Xtensive.Sql.Dom.Dml;
using Xtensive.Storage.Model;
using SqlFactory = Xtensive.Sql.Dom.Sql;

namespace Xtensive.Storage.Providers.Sql
{
  /// <summary>
  /// Builder of <see cref="SqlRequest"/>s.
  /// </summary>
  public class SqlRequestBuilder : InitializableHandlerBase
  {
    protected DomainHandler DomainHandler { get; private set; }

    /// <summary>
    /// Builds the request.
    /// </summary>
    /// <param name="task">The request builder task.</param>
    /// <returns><see cref="SqlModificationRequest"/> instance for the specified <paramref name="task"/>.</returns>
    public SqlModificationRequest Build(SqlRequestBuilderTask task)
    {
      SqlRequestBuilderResult builderResult = null;
      switch (task.Kind) {
      case SqlModificationRequestKind.Insert:
        builderResult = ProcessInsertTask(task);
        break;
      case SqlModificationRequestKind.Remove:
        builderResult = ProcessRemoveTask(task);
        break;
      case SqlModificationRequestKind.Update:
        builderResult = ProcessUpdateTask(task);
        break;
      }
      return CreateRequest(builderResult);
    }

    private SqlModificationRequest CreateRequest(SqlRequestBuilderResult builderResult)
    {
      var request = new SqlModificationRequest(builderResult.Batch);
      SetExpectedResult(request);
      foreach (var binding in builderResult.ParameterBindings)
        request.ParameterBindings[binding.Key] = binding.Value;

      DomainHandler.Compile(request);
      return request;
    }

    protected virtual void SetExpectedResult(SqlModificationRequest request)
    {
      request.ExpectedResult = ((SqlBatch)request.Statement).Count;
    }

    protected virtual SqlRequestBuilderResult ProcessInsertTask(SqlRequestBuilderTask task)
    {
      var result = new SqlRequestBuilderResult(task, SqlFactory.Batch());
      foreach (IndexInfo index in result.AffectedIndexes) {
        SqlTableRef table = SqlFactory.TableRef(DomainHandler.MappingSchema[index].Table);
        SqlInsert query = SqlFactory.Insert(table);

        for (int i = 0; i < index.Columns.Count; i++) {
          ColumnInfo column = index.Columns[i];
          int offset = GetOffset(task.Type, column);
          if (offset >= 0) {
            SqlParameter p = BuildParameter(result, column);
            query.Values[table[i]] = p;
            result.ParameterBindings[p] = CreateTupleFieldAccessor(offset);
          }
        }
        result.Batch.Add(query);
      }
      return result;
    }

    protected virtual SqlRequestBuilderResult ProcessUpdateTask(SqlRequestBuilderTask task)
    {
      var result = new SqlRequestBuilderResult(task, SqlFactory.Batch());
      foreach (IndexInfo index in result.AffectedIndexes) {
        SqlTableRef table = SqlFactory.TableRef(DomainHandler.MappingSchema[index].Table);
        SqlUpdate query = SqlFactory.Update(table);

        for (int i = 0; i < index.Columns.Count; i++) {
          ColumnInfo column = index.Columns[i];
          int offset = GetOffset(task.Type, column);
          if (offset >= 0 && task.FieldMap[offset]) {
            SqlParameter p = BuildParameter(result, column);
            query.Values[table[i]] = p;
            result.ParameterBindings[p] = CreateTupleFieldAccessor(offset);
          }
        }

        // There is nothing to update in this table, skipping it
        if (query.Values.Count == 0)
          continue;
        query.Where &= BuildWhereExpression(result, table);
        result.Batch.Add(query);
      }
      return result;
    }

    protected virtual SqlRequestBuilderResult ProcessRemoveTask(SqlRequestBuilderTask task)
    {
      var result = new SqlRequestBuilderResult(task, SqlFactory.Batch());
      foreach (IndexInfo index in result.AffectedIndexes) {
        SqlTableRef table = SqlFactory.TableRef(DomainHandler.MappingSchema[index].Table);
        SqlDelete query = SqlFactory.Delete(table);
        query.Where &= BuildWhereExpression(result, table);
        result.Batch.Add(query);
      }
      return result;
    }

    protected virtual SqlExpression BuildWhereExpression(SqlRequestBuilderResult result, SqlTableRef table)
    {
      SqlExpression expression = null;
      int i = 0;
      foreach (ColumnInfo column in result.PrimaryIndex.KeyColumns.Keys) {
        int offset = GetOffset(result.Task.Type, column);
        SqlParameter p = BuildParameter(result, column);
        expression &= table[i++]==p;
        result.ParameterBindings[p] = CreateTupleFieldAccessor(offset);
      }
      return expression;
    }

    private static int GetOffset(TypeInfo type, ColumnInfo column)
    {
      var field = type.Fields[column.Field.Name];
      return field == null ? -1 : field.MappingInfo.Offset;
    }

    private static SqlParameter BuildParameter(SqlRequestBuilderResult builderResult, ColumnInfo column)
    {
      SqlParameter result;
      if (!builderResult.ParameterMapping.TryGetValue(column, out result)) {
        result = new SqlParameter("p" + builderResult.ParameterMapping.Count);
        builderResult.ParameterMapping.Add(column, result);
      }
      return result;
    }

    private static Func<Tuple, object> CreateTupleFieldAccessor(int fieldIndex)
    {
      return (target => target.IsNull(fieldIndex) ? DBNull.Value : target.GetValue(fieldIndex));
    }

    /// <inheritdoc/>
    public override void Initialize()
    {
      DomainHandler = Handlers.DomainHandler as DomainHandler;
    }


    // Constructor

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    public SqlRequestBuilder()
    {
    }
  }
}
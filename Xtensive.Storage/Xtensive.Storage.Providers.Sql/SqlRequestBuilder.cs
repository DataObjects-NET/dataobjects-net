// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.08.28

using System;
using Xtensive.Core.Internals.DocTemplates;
using Xtensive.Core.Tuples;
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
    /// <returns><see cref="SqlUpdateRequest"/> instance for the specified <paramref name="task"/>.</returns>
    public SqlUpdateRequest Build(SqlRequestBuilderTask task)
    {
      SqlRequestBuilderContext context = new SqlRequestBuilderContext(task, SqlFactory.Batch());
      switch (task.Kind) {
      case SqlUpdateRequestKind.Insert:
        BuildInsertRequest(context);
        break;
      case SqlUpdateRequestKind.Remove:
        BuildRemoveRequest(context);
        break;
      case SqlUpdateRequestKind.Update:
        BuildUpdateRequest(context);
        break;
      }

      var result = new SqlUpdateRequest(context.Batch);

      foreach (var pair in context.ParameterBindings)
        result.ParameterBindings.Add(pair.Value);

      SetExpectedResult(result);
      DomainHandler.Compile(result);

      return result;
    }

    protected virtual void SetExpectedResult(SqlUpdateRequest request)
    {
      request.ExpectedResult = ((SqlBatch)request.Statement).Count;
    }

    protected virtual void BuildInsertRequest(SqlRequestBuilderContext context)
    {
      foreach (IndexInfo index in context.AffectedIndexes) {
        SqlTableRef table = SqlFactory.TableRef(DomainHandler.MappingSchema[index].Table);
        SqlInsert query = SqlFactory.Insert(table);

        for (int i = 0; i < index.Columns.Count; i++) {
          ColumnInfo column = index.Columns[i];
          int fieldIndex = GetFieldIndex(context.Type, column);
          if (fieldIndex >= 0) {
            SqlUpdateParameterBinding binding = context.GetParameterBinding(column, GetTupleFieldAccessor(fieldIndex));
            query.Values[table[i]] = binding.SqlParameter;
          }
        }
        context.Batch.Add(query);
      }
    }

    protected virtual void BuildUpdateRequest(SqlRequestBuilderContext context)
    {
      foreach (IndexInfo index in context.AffectedIndexes) {
        SqlTableRef table = SqlFactory.TableRef(DomainHandler.MappingSchema[index].Table);
        SqlUpdate query = SqlFactory.Update(table);

        for (int i = 0; i < index.Columns.Count; i++) {
          ColumnInfo column = index.Columns[i];
          int fieldIndex = GetFieldIndex(context.Type, column);
          if (fieldIndex >= 0 && context.Task.FieldMap[fieldIndex]) {
            SqlUpdateParameterBinding binding = context.GetParameterBinding(column, GetTupleFieldAccessor(fieldIndex));
            query.Values[table[i]] = binding.SqlParameter;
          }
        }

        // There is nothing to update in this table, skipping it
        if (query.Values.Count == 0)
          continue;
        query.Where &= BuildWhereExpression(context, table);
        context.Batch.Add(query);
      }
    }

    protected virtual void BuildRemoveRequest(SqlRequestBuilderContext context)
    {
      foreach (IndexInfo index in context.AffectedIndexes) {
        SqlTableRef table = SqlFactory.TableRef(DomainHandler.MappingSchema[index].Table);
        SqlDelete query = SqlFactory.Delete(table);
        query.Where &= BuildWhereExpression(context, table);
        context.Batch.Add(query);
      }
    }

    protected virtual SqlExpression BuildWhereExpression(SqlRequestBuilderContext context, SqlTableRef table)
    {
      SqlExpression expression = null;
      int i = 0;
      foreach (ColumnInfo column in context.PrimaryIndex.KeyColumns.Keys) {
        int fieldIndex = GetFieldIndex(context.Task.Type, column);
        SqlUpdateParameterBinding binding = context.GetParameterBinding(column, GetTupleFieldAccessor(fieldIndex));
        expression &= table[i++]==binding.SqlParameter;
      }
      return expression;
    }

    private static int GetFieldIndex(TypeInfo type, ColumnInfo column)
    {
      var field = type.Fields[column.Field.Name];
      return field == null ? -1 : field.MappingInfo.Offset;
    }

    private static Func<Tuple, object> GetTupleFieldAccessor(int fieldIndex)
    {
      return (tuple => tuple.IsNull(fieldIndex) ? DBNull.Value : tuple.GetValue(fieldIndex));
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
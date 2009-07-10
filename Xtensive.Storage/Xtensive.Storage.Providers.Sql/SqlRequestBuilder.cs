// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.08.28

using System.Collections.Generic;
using Xtensive.Core.Internals.DocTemplates;
using Xtensive.Sql;
using Xtensive.Sql.Dml;
using Xtensive.Sql.ValueTypeMapping;
using Xtensive.Storage.Model;

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
      var context = new SqlRequestBuilderContext(task, SqlDml.Batch());
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

      var bindings = new List<SqlUpdateParameterBinding>();
      
      foreach (var pair in context.ParameterBindings)
        bindings.Add(pair.Value);

      return new SqlUpdateRequest(context.Batch, GetExpectedResult(context.Batch), bindings);
    }

    protected virtual int GetExpectedResult(SqlBatch request)
    {
      return request.Count;
    }

    protected virtual void BuildInsertRequest(SqlRequestBuilderContext context)
    {
      foreach (IndexInfo index in context.AffectedIndexes) {
        SqlTableRef table = SqlDml.TableRef(DomainHandler.Mapping[index].Table);
        SqlInsert query = SqlDml.Insert(table);

        for (int i = 0; i < index.Columns.Count; i++) {
          ColumnInfo column = index.Columns[i];
          int fieldIndex = GetFieldIndex(context.Type, column);
          if (fieldIndex >= 0) {
            SqlUpdateParameterBinding binding;
            if (!context.ParameterBindings.TryGetValue(column, out binding)) {
              TypeMapping typeMapping = DomainHandler.ValueTypeMapper.GetTypeMapping(column);
              binding = new SqlUpdateParameterBinding(fieldIndex, typeMapping);
              context.ParameterBindings.Add(column, binding);
            }
            query.Values[table[column.Name]] = binding.ParameterReference;
          }
        }
        context.Batch.Add(query);
      }
    }

    protected virtual void BuildUpdateRequest(SqlRequestBuilderContext context)
    {
      foreach (IndexInfo index in context.AffectedIndexes) {
        SqlTableRef table = SqlDml.TableRef(DomainHandler.Mapping[index].Table);
        SqlUpdate query = SqlDml.Update(table);

        for (int i = 0; i < index.Columns.Count; i++) {
          ColumnInfo column = index.Columns[i];
          int fieldIndex = GetFieldIndex(context.Type, column);
          if (fieldIndex >= 0 && context.Task.FieldMap[fieldIndex]) {
            SqlUpdateParameterBinding binding;
            if (!context.ParameterBindings.TryGetValue(column, out binding)) {
              TypeMapping typeMapping = DomainHandler.ValueTypeMapper.GetTypeMapping(column);
              binding = new SqlUpdateParameterBinding(fieldIndex, typeMapping);
              context.ParameterBindings.Add(column, binding);
            }
            query.Values[table[column.Name]] = binding.ParameterReference;
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
      for (int i = context.AffectedIndexes.Count - 1; i >= 0; i--) {
        IndexInfo index = context.AffectedIndexes[i];
        SqlTableRef table = SqlDml.TableRef(DomainHandler.Mapping[index].Table);
        SqlDelete query = SqlDml.Delete(table);
        query.Where &= BuildWhereExpression(context, table);
        context.Batch.Add(query);
      }
    }

    protected virtual SqlExpression BuildWhereExpression(SqlRequestBuilderContext context, SqlTableRef table)
    {
      SqlExpression expression = null;
      foreach (ColumnInfo column in context.PrimaryIndex.KeyColumns.Keys) {
        int fieldIndex = GetFieldIndex(context.Task.Type, column);
        SqlUpdateParameterBinding binding;
        if (!context.ParameterBindings.TryGetValue(column, out binding)) {
          TypeMapping typeMapping = DomainHandler.ValueTypeMapper.GetTypeMapping(column);
          binding = new SqlUpdateParameterBinding(fieldIndex, typeMapping);
          context.ParameterBindings.Add(column, binding);
        }
        expression &= table[column.Name]==binding.ParameterReference;
      }
      return expression;
    }

    private static int GetFieldIndex(TypeInfo type, ColumnInfo column)
    {
      FieldInfo field;
      if (!type.Fields.TryGetValue(column.Field.Name, out field))
        return -1;
      return field.MappingInfo.Offset;
    }

    /// <inheritdoc/>
    public override void Initialize()
    {
      DomainHandler = Handlers.DomainHandler as DomainHandler;
    }
    

    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    public SqlRequestBuilder()
    {
    }
  }
}
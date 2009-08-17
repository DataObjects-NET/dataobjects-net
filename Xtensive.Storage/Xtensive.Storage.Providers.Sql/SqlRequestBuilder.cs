// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.08.28

using System.Collections.Generic;
using Xtensive.Core.Internals.DocTemplates;
using Xtensive.Sql;
using Xtensive.Sql.Dml;
using Xtensive.Sql.Model;
using Xtensive.Sql.ValueTypeMapping;
using Xtensive.Storage.Model;

namespace Xtensive.Storage.Providers.Sql
{
  /// <summary>
  /// Builder of <see cref="SqlRequest"/>s.
  /// </summary>
  public class SqlRequestBuilder : InitializableHandlerBase
  {
    private bool useLargeObjects;

    protected DomainHandler DomainHandler { get; private set; }

    /// <summary>
    /// Builds the request.
    /// </summary>
    /// <param name="task">The request builder task.</param>
    /// <returns><see cref="SqlPersistRequest"/> instance for the specified <paramref name="task"/>.</returns>
    public SqlPersistRequest Build(SqlRequestBuilderTask task)
    {
      var context = new SqlRequestBuilderContext(task, SqlDml.Batch());
      switch (task.Kind) {
      case SqlPersistRequestKind.Insert:
        BuildInsertRequest(context);
        break;
      case SqlPersistRequestKind.Remove:
        BuildRemoveRequest(context);
        break;
      case SqlPersistRequestKind.Update:
        BuildUpdateRequest(context);
        break;
      }

      var bindings = new List<SqlPersistParameterBinding>();
      
      foreach (var pair in context.ParameterBindings)
        bindings.Add(pair.Value);

      return new SqlPersistRequest(context.Batch, GetExpectedResult(context.Batch), bindings);
    }

    protected virtual int? GetExpectedResult(SqlBatch request)
    {
      return request.Count;
    }
    
    protected virtual void BuildInsertRequest(SqlRequestBuilderContext context)
    {
      foreach (IndexInfo index in context.AffectedIndexes) {
        var table = DomainHandler.Mapping[index].Table;
        var tableRef = SqlDml.TableRef(table);
        var query = SqlDml.Insert(tableRef);

        for (int i = 0; i < index.Columns.Count; i++) {
          ColumnInfo column = index.Columns[i];
          int fieldIndex = GetFieldIndex(context.Type, column);
          if (fieldIndex >= 0) {
            SqlPersistParameterBinding binding;
            if (!context.ParameterBindings.TryGetValue(column, out binding)) {
              var typeMapping = DomainHandler.Driver.GetTypeMapping(column);
              var bindingType = GetBindingType(table.TableColumns[column.Name]);
              binding = new SqlPersistParameterBinding(fieldIndex, typeMapping, bindingType);
              context.ParameterBindings.Add(column, binding);
            }
            query.Values[tableRef[column.Name]] = binding.ParameterReference;
          }
        }
        context.Batch.Add(query);
      }
    }

    protected virtual void BuildUpdateRequest(SqlRequestBuilderContext context)
    {
      foreach (IndexInfo index in context.AffectedIndexes) {
        var table = DomainHandler.Mapping[index].Table;
        var tableRef = SqlDml.TableRef(table);
        var query = SqlDml.Update(tableRef);

        for (int i = 0; i < index.Columns.Count; i++) {
          ColumnInfo column = index.Columns[i];
          int fieldIndex = GetFieldIndex(context.Type, column);
          if (fieldIndex >= 0 && context.Task.FieldMap[fieldIndex]) {
            SqlPersistParameterBinding binding;
            if (!context.ParameterBindings.TryGetValue(column, out binding)) {
              var typeMapping = DomainHandler.Driver.GetTypeMapping(column);
              var bindingType = GetBindingType(table.TableColumns[column.Name]);
              binding = new SqlPersistParameterBinding(fieldIndex, typeMapping, bindingType);
              context.ParameterBindings.Add(column, binding);
            }
            query.Values[tableRef[column.Name]] = binding.ParameterReference;
          }
        }

        // There is nothing to update in this table, skipping it
        if (query.Values.Count == 0)
          continue;
        query.Where &= BuildWhereExpression(context, tableRef);
        context.Batch.Add(query);
      }
    }

    protected virtual void BuildRemoveRequest(SqlRequestBuilderContext context)
    {
      for (int i = context.AffectedIndexes.Count - 1; i >= 0; i--) {
        var index = context.AffectedIndexes[i];
        var tableRef = SqlDml.TableRef(DomainHandler.Mapping[index].Table);
        var query = SqlDml.Delete(tableRef);
        query.Where &= BuildWhereExpression(context, tableRef);
        context.Batch.Add(query);
      }
    }

    protected virtual SqlExpression BuildWhereExpression(SqlRequestBuilderContext context, SqlTableRef table)
    {
      SqlExpression expression = null;
      foreach (ColumnInfo column in context.PrimaryIndex.KeyColumns.Keys) {
        int fieldIndex = GetFieldIndex(context.Task.Type, column);
        SqlPersistParameterBinding binding;
        if (!context.ParameterBindings.TryGetValue(column, out binding)) {
          TypeMapping typeMapping = DomainHandler.Driver.GetTypeMapping(column);
          binding = new SqlPersistParameterBinding(fieldIndex, typeMapping);
          context.ParameterBindings.Add(column, binding);
        }
        expression &= table[column.Name]==binding.ParameterReference;
      }
      return expression;
    }

    protected virtual SqlPersistParameterBindingType GetBindingType(TableColumn column)
    {
      if (!useLargeObjects)
        return SqlPersistParameterBindingType.Regular;
      switch (column.DataType.Type) {
      case SqlType.VarCharMax:
        return SqlPersistParameterBindingType.CharacterLob;
      case SqlType.VarBinaryMax:
        return SqlPersistParameterBindingType.BinaryLob;
      default:
        return SqlPersistParameterBindingType.Regular;
      }
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
      DomainHandler = (DomainHandler) Handlers.DomainHandler;
      useLargeObjects = DomainHandler.ProviderInfo.SupportsLargeObjects;
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
// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.08.28

using Xtensive.Core.Internals.DocTemplates;
using Xtensive.Sql;
using Xtensive.Sql.Dml;
using Xtensive.Sql.Model;
using Xtensive.Sql.ValueTypeMapping;
using Xtensive.Storage.Model;

namespace Xtensive.Storage.Providers.Sql
{
  /// <summary>
  /// Builder of <see cref="Request"/>s.
  /// </summary>
  public class PersistRequestBuilder : InitializableHandlerBase
  {
    private bool useLargeObjects;

    protected DomainHandler DomainHandler { get; private set; }

    /// <summary>
    /// Builds the request.
    /// </summary>
    /// <param name="task">The request builder task.</param>
    /// <returns><see cref="PersistRequest"/> instance for the specified <paramref name="task"/>.</returns>
    public PersistRequest Build(PersistRequestBuilderTask task)
    {
      var context = new PersistRequestBuilderContext(task, SqlDml.Batch());
      switch (task.Kind) {
      case PersistRequestKind.Insert:
        BuildInsertRequest(context);
        break;
      case PersistRequestKind.Remove:
        BuildRemoveRequest(context);
        break;
      case PersistRequestKind.Update:
        BuildUpdateRequest(context);
        break;
      }

      return new PersistRequest(context.Batch, context.ParameterBindings.Values);
    }
    
    protected virtual void BuildInsertRequest(PersistRequestBuilderContext context)
    {
      foreach (IndexInfo index in context.AffectedIndexes) {
        var table = DomainHandler.Mapping[index].Table;
        var tableRef = SqlDml.TableRef(table);
        var query = SqlDml.Insert(tableRef);

        for (int i = 0; i < index.Columns.Count; i++) {
          ColumnInfo column = index.Columns[i];
          int fieldIndex = GetFieldIndex(context.Type, column);
          if (fieldIndex >= 0) {
            PersistParameterBinding binding;
            if (!context.ParameterBindings.TryGetValue(column, out binding)) {
              var typeMapping = DomainHandler.Driver.GetTypeMapping(column);
              var bindingType = GetBindingType(table.TableColumns[column.Name]);
              binding = new PersistParameterBinding(fieldIndex, typeMapping, bindingType);
              context.ParameterBindings.Add(column, binding);
            }
            query.Values[tableRef[column.Name]] = binding.ParameterReference;
          }
        }
        context.Batch.Add(query);
      }
    }

    protected virtual void BuildUpdateRequest(PersistRequestBuilderContext context)
    {
      foreach (IndexInfo index in context.AffectedIndexes) {
        var table = DomainHandler.Mapping[index].Table;
        var tableRef = SqlDml.TableRef(table);
        var query = SqlDml.Update(tableRef);

        for (int i = 0; i < index.Columns.Count; i++) {
          ColumnInfo column = index.Columns[i];
          int fieldIndex = GetFieldIndex(context.Type, column);
          if (fieldIndex >= 0 && context.Task.FieldMap[fieldIndex]) {
            PersistParameterBinding binding;
            if (!context.ParameterBindings.TryGetValue(column, out binding)) {
              var typeMapping = DomainHandler.Driver.GetTypeMapping(column);
              var bindingType = GetBindingType(table.TableColumns[column.Name]);
              binding = new PersistParameterBinding(fieldIndex, typeMapping, bindingType);
              context.ParameterBindings.Add(column, binding);
            }
            query.Values[tableRef[column.Name]] = binding.ParameterReference;
          }
        }

        // There is nothing to update in this table, skipping it
        if (query.Values.Count==0)
          continue;
        query.Where &= BuildWhereExpression(context, tableRef);
        context.Batch.Add(query);
      }
    }

    protected virtual void BuildRemoveRequest(PersistRequestBuilderContext context)
    {
      for (int i = context.AffectedIndexes.Count - 1; i >= 0; i--) {
        var index = context.AffectedIndexes[i];
        var tableRef = SqlDml.TableRef(DomainHandler.Mapping[index].Table);
        var query = SqlDml.Delete(tableRef);
        query.Where &= BuildWhereExpression(context, tableRef);
        context.Batch.Add(query);
      }
    }

    protected virtual SqlExpression BuildWhereExpression(PersistRequestBuilderContext context, SqlTableRef table)
    {
      SqlExpression expression = null;
      foreach (ColumnInfo column in context.PrimaryIndex.KeyColumns.Keys) {
        int fieldIndex = GetFieldIndex(context.Task.Type, column);
        PersistParameterBinding binding;
        if (!context.ParameterBindings.TryGetValue(column, out binding)) {
          TypeMapping typeMapping = DomainHandler.Driver.GetTypeMapping(column);
          binding = new PersistParameterBinding(fieldIndex, typeMapping);
          context.ParameterBindings.Add(column, binding);
        }
        expression &= table[column.Name]==binding.ParameterReference;
      }
      return expression;
    }

    protected virtual PersistParameterBindingType GetBindingType(TableColumn column)
    {
      if (!useLargeObjects)
        return PersistParameterBindingType.Regular;
      switch (column.DataType.Type) {
      case SqlType.VarCharMax:
        return PersistParameterBindingType.CharacterLob;
      case SqlType.VarBinaryMax:
        return PersistParameterBindingType.BinaryLob;
      default:
        return PersistParameterBindingType.Regular;
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
      useLargeObjects = DomainHandler.ProviderInfo.Supports(ProviderFeatures.LargeObjects);
    }
    
    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    public PersistRequestBuilder()
    {
    }
  }
}
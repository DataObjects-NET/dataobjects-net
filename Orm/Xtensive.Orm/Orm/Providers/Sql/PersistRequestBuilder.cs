// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.08.28

using System;
using System.Collections.Generic;
using Xtensive.Collections;
using Xtensive.Internals.DocTemplates;
using Xtensive.Sql;
using Xtensive.Sql.Dml;
using Xtensive.Sql.Model;
using Xtensive.Orm.Model;

namespace Xtensive.Orm.Providers.Sql
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
    public IEnumerable<PersistRequest> Build(PersistRequestBuilderTask task)
    {
      var context = new PersistRequestBuilderContext(task);
      List<PersistRequest> result = null;
      switch (task.Kind) {
        case PersistRequestKind.Insert:
          result = BuildInsertRequest(context);
          break;
        case PersistRequestKind.Remove:
          result = BuildRemoveRequest(context);
          break;
        case PersistRequestKind.Update:
          result = BuildUpdateRequest(context);
          break;
      }

      // Merging requests for servers which support batching
      if (DomainHandler.ProviderInfo.Supports(ProviderFeatures.Batches) && result.Count > 1) {
        var batch = SqlDml.Batch();
        var bindings = new HashSet<PersistParameterBinding>();
        foreach (var request in result) {
          batch.Add((SqlStatement) request.Statement);
          bindings.UnionWith(request.ParameterBindings);
        }
        return EnumerableUtils.One(new PersistRequest(batch, bindings));
      }
      return result;
    }
    
    protected virtual List<PersistRequest> BuildInsertRequest(PersistRequestBuilderContext context)
    {
      var result = new List<PersistRequest>();
      foreach (Orm.Model.IndexInfo index in context.AffectedIndexes) {
        var table = DomainHandler.Mapping[index].Table;
        var tableRef = SqlDml.TableRef(table);
        var query = SqlDml.Insert(tableRef);
        var bindings = new List<PersistParameterBinding>();

        for (int i = 0; i < index.Columns.Count; i++) {
          var column = index.Columns[i];
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
            bindings.Add(binding);
          }
        }
        result.Add(new PersistRequest(query, bindings));
      }
      return result;
    }

    protected virtual List<PersistRequest> BuildUpdateRequest(PersistRequestBuilderContext context)
    {
      var result = new List<PersistRequest>();
      foreach (IndexInfo index in context.AffectedIndexes) {
        var table = DomainHandler.Mapping[index].Table;
        var tableRef = SqlDml.TableRef(table);
        var query = SqlDml.Update(tableRef);
        var bindings = new List<PersistParameterBinding>();

        PersistParameterBinding binding;
        int fieldIndex;
        for (int i = 0; i < index.Columns.Count; i++) {
          ColumnInfo column = index.Columns[i];
          fieldIndex = GetFieldIndex(context.Type, column);
          if (fieldIndex >= 0 && context.Task.FieldMap[fieldIndex]) {
            if (!context.ParameterBindings.TryGetValue(column, out binding)) {
              var typeMapping = DomainHandler.Driver.GetTypeMapping(column);
              var bindingType = GetBindingType(table.TableColumns[column.Name]);
              binding = new PersistParameterBinding(fieldIndex, typeMapping, bindingType);
              context.ParameterBindings.Add(column, binding);
            }
            query.Values[tableRef[column.Name]] = binding.ParameterReference;
            bindings.Add(binding);
          }
        }

        // There is nothing to update in this table, skipping it
        if (query.Values.Count==0)
          continue;

        SqlExpression expression = null;
        foreach (ColumnInfo column1 in context.PrimaryIndex.KeyColumns.Keys) {
          fieldIndex = GetFieldIndex(context.Task.Type, column1);
          if (!context.ParameterBindings.TryGetValue(column1, out binding)) {
            TypeMapping typeMapping1 = DomainHandler.Driver.GetTypeMapping(column1);
            binding = new PersistParameterBinding(fieldIndex, typeMapping1);
            context.ParameterBindings.Add(column1, binding);
          }
          expression &= tableRef[column1.Name]==binding.ParameterReference;
          bindings.Add(binding);
        }
        query.Where &= expression;
        result.Add(new PersistRequest(query, bindings));
      }
      return result;
    }

    protected virtual List<PersistRequest> BuildRemoveRequest(PersistRequestBuilderContext context)
    {
      var result = new List<PersistRequest>();
      for (int i = context.AffectedIndexes.Count - 1; i >= 0; i--) {
        var index = context.AffectedIndexes[i];
        var tableRef = SqlDml.TableRef(DomainHandler.Mapping[index].Table);
        var query = SqlDml.Delete(tableRef);
        var bindings = new List<PersistParameterBinding>();

        SqlExpression expression = null;
        foreach (ColumnInfo column in context.PrimaryIndex.KeyColumns.Keys) {
          int fieldIndex = GetFieldIndex(context.Task.Type, column);
          PersistParameterBinding binding;
          if (!context.ParameterBindings.TryGetValue(column, out binding)) {
            TypeMapping typeMapping = DomainHandler.Driver.GetTypeMapping(column);
            binding = new PersistParameterBinding(fieldIndex, typeMapping);
            context.ParameterBindings.Add(column, binding);
          }
          expression &= tableRef[column.Name]==binding.ParameterReference;
          bindings.Add(binding);
        }
        query.Where &= expression;
        result.Add(new PersistRequest(query, bindings));
      }
      return result;
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
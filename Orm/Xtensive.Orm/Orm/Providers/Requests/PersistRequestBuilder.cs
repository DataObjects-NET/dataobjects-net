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

namespace Xtensive.Orm.Providers
{
  /// <summary>
  /// Builder of <see cref="PersistRequest"/>s.
  /// </summary>
  public class PersistRequestBuilder : HandlerBase
  {
    private bool useLargeObjects;

    private Providers.DomainHandler domainHandler;
    private ProviderInfo providerInfo;
    private StorageDriver driver;

    /// <summary>
    /// Builds the request.
    /// </summary>
    /// <param name="task">The request builder task.</param>
    /// <returns><see cref="PersistRequest"/> instance for the specified <paramref name="task"/>.</returns>
    public IEnumerable<PersistRequest> Build(PersistRequestBuilderTask task)
    {
      var context = new PersistRequestBuilderContext(task);
      List<PersistRequest> result;
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
        default:
          throw new ArgumentOutOfRangeException("task.Kind");
      }

      // Merging requests for servers which support batching
      if (providerInfo.Supports(ProviderFeatures.Batches) && result.Count > 1) {
        var batch = SqlDml.Batch();
        var bindings = new HashSet<PersistParameterBinding>();
        foreach (var request in result) {
          batch.Add(request.Statement);
          bindings.UnionWith(request.ParameterBindings);
        }
        var batchRequest = new PersistRequest(driver, batch, bindings);
        batchRequest.Prepare();
        return EnumerableUtils.One(batchRequest);
      }

      foreach (var item in result)
        item.Prepare();

      return result;
    }
    
    protected virtual List<PersistRequest> BuildInsertRequest(PersistRequestBuilderContext context)
    {
      var result = new List<PersistRequest>();
      foreach (Orm.Model.IndexInfo index in context.AffectedIndexes) {
        var table = domainHandler.Mapping[index.ReflectedType];
        var tableRef = SqlDml.TableRef(table);
        var query = SqlDml.Insert(tableRef);
        var bindings = new List<PersistParameterBinding>();

        for (int i = 0; i < index.Columns.Count; i++) {
          var column = index.Columns[i];
          int fieldIndex = GetFieldIndex(context.Type, column);
          if (fieldIndex >= 0) {
            PersistParameterBinding binding;
            if (!context.ParameterBindings.TryGetValue(column, out binding)) {
              var typeMapping = driver.GetTypeMapping(column);
              var bindingType = GetBindingType(table.TableColumns[column.Name]);
              binding = new PersistParameterBinding(fieldIndex, typeMapping, bindingType);
              context.ParameterBindings.Add(column, binding);
            }
            query.Values[tableRef[column.Name]] = binding.ParameterReference;
            bindings.Add(binding);
          }
        }
        result.Add(new PersistRequest(driver, query, bindings));
      }
      return result;
    }

    protected virtual List<PersistRequest> BuildUpdateRequest(PersistRequestBuilderContext context)
    {
      var result = new List<PersistRequest>();
      foreach (IndexInfo index in context.AffectedIndexes) {
        var table = domainHandler.Mapping[index.ReflectedType];
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
              var typeMapping = driver.GetTypeMapping(column);
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
            TypeMapping typeMapping1 = driver.GetTypeMapping(column1);
            binding = new PersistParameterBinding(fieldIndex, typeMapping1);
            context.ParameterBindings.Add(column1, binding);
          }
          expression &= tableRef[column1.Name]==binding.ParameterReference;
          bindings.Add(binding);
        }
        query.Where &= expression;
        result.Add(new PersistRequest(driver, query, bindings));
      }
      return result;
    }

    protected virtual List<PersistRequest> BuildRemoveRequest(PersistRequestBuilderContext context)
    {
      var result = new List<PersistRequest>();
      for (int i = context.AffectedIndexes.Count - 1; i >= 0; i--) {
        var index = context.AffectedIndexes[i];
        var tableRef = SqlDml.TableRef(domainHandler.Mapping[index.ReflectedType]);
        var query = SqlDml.Delete(tableRef);
        var bindings = new List<PersistParameterBinding>();

        SqlExpression expression = null;
        foreach (ColumnInfo column in context.PrimaryIndex.KeyColumns.Keys) {
          int fieldIndex = GetFieldIndex(context.Task.Type, column);
          PersistParameterBinding binding;
          if (!context.ParameterBindings.TryGetValue(column, out binding)) {
            TypeMapping typeMapping = driver.GetTypeMapping(column);
            binding = new PersistParameterBinding(fieldIndex, typeMapping);
            context.ParameterBindings.Add(column, binding);
          }
          expression &= tableRef[column.Name]==binding.ParameterReference;
          bindings.Add(binding);
        }
        query.Where &= expression;
        result.Add(new PersistRequest(driver, query, bindings));
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
      domainHandler = Handlers.DomainHandler;
      driver = Handlers.StorageDriver;
      providerInfo = Handlers.ProviderInfo;
      useLargeObjects = Handlers.ProviderInfo.Supports(ProviderFeatures.LargeObjects);
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
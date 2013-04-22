// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.08.28

using System;
using System.Collections.Generic;
using Xtensive.Collections;
using Xtensive.Sql;
using Xtensive.Sql.Dml;
using Xtensive.Sql.Model;
using Xtensive.Orm.Model;

namespace Xtensive.Orm.Providers
{
  /// <summary>
  /// Builder of <see cref="PersistRequest"/>s.
  /// </summary>
  public class PersistRequestBuilder : DomainBoundHandler
  {
    private bool useLargeObjects;

    private DomainHandler domainHandler;
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
      // unless version validation is requested.
      if (providerInfo.Supports(ProviderFeatures.Batches) && result.Count > 1 && !task.ValidateVersion) {
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
      foreach (var index in context.AffectedIndexes) {
        var table = domainHandler.Mapping[index.ReflectedType];
        var tableRef = SqlDml.TableRef(table);
        var query = SqlDml.Insert(tableRef);
        var bindings = new List<PersistParameterBinding>();

        foreach (var column in index.Columns) {
          int fieldIndex = GetFieldIndex(context.Type, column);
          if (fieldIndex >= 0) {
            var binding = GetBinding(context, column, table, fieldIndex);
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
      foreach (var index in context.AffectedIndexes) {
        var table = domainHandler.Mapping[index.ReflectedType];
        var tableRef = SqlDml.TableRef(table);
        var query = SqlDml.Update(tableRef);
        var bindings = new List<PersistParameterBinding>();

        foreach (var column in index.Columns) {
          int fieldIndex = GetFieldIndex(context.Type, column);
          if (fieldIndex >= 0 && context.Task.FieldMap[fieldIndex]) {
            var binding = GetBinding(context, column, table, fieldIndex);
            query.Values[tableRef[column.Name]] = binding.ParameterReference;
            bindings.Add(binding);
          }
        }

        // There is nothing to update in this table, skipping it
        if (query.Values.Count==0)
          continue;

        query.Where = BuildKeyFilter(context, tableRef, bindings);
        if (context.Task.ValidateVersion)
          query.Where &= BuildVersionFilter(context, tableRef, bindings);

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
        query.Where = BuildKeyFilter(context, tableRef, bindings);
        if (context.Task.ValidateVersion)
          query.Where &= BuildVersionFilter(context, tableRef, bindings);
        result.Add(new PersistRequest(driver, query, bindings));
      }
      return result;
    }

    private SqlExpression BuildKeyFilter(PersistRequestBuilderContext context,
      SqlTableRef filteredTable, List<PersistParameterBinding> currentBindings)
    {
      return BuildColumnFilter(context, context.PrimaryIndex.KeyColumns.Keys,
        filteredTable, currentBindings, context.ParameterBindings, false, PersistParameterBindingType.Regular);
    }

    private SqlExpression BuildVersionFilter(PersistRequestBuilderContext context,
      SqlTableRef filteredTable, List<PersistParameterBinding> currentBindings)
    {
      return BuildColumnFilter(context, context.Type.GetVersionColumns(),
        filteredTable, currentBindings, context.VersionParameterBindings, true, PersistParameterBindingType.VersionFilter);
    }

    private SqlExpression BuildColumnFilter(PersistRequestBuilderContext context, IEnumerable<ColumnInfo> columns,
      SqlTableRef filteredTable, List<PersistParameterBinding> currentBindings,
      Dictionary<ColumnInfo, PersistParameterBinding> knownBindings, bool allowNull, PersistParameterBindingType parameterBindingType)
    {
      SqlExpression result = null;
      foreach (var column in columns) {
        var fieldIndex = GetFieldIndex(context.Type, column);
        PersistParameterBinding binding;
        if (!knownBindings.TryGetValue(column, out binding)) {
          var typeMapping = driver.GetTypeMapping(column);
          binding = new PersistParameterBinding(typeMapping, fieldIndex, ParameterTransmissionType.Regular, parameterBindingType);
          knownBindings.Add(column, binding);
        }
        var filteredColumn = filteredTable[column.Name];
        var filter = filteredColumn==binding.ParameterReference;
        if (allowNull)
          result &= SqlDml.Variant(binding, filter, SqlDml.IsNull(filteredColumn));
        else
          result &= filter;
        currentBindings.Add(binding);
      }
      return result;
    }

    private PersistParameterBinding GetBinding(PersistRequestBuilderContext context, ColumnInfo column, Table table, int fieldIndex)
    {
      PersistParameterBinding binding;
      if (!context.ParameterBindings.TryGetValue(column, out binding)) {
        var typeMapping = driver.GetTypeMapping(column);
        var bindingType = GetTransmissionType(table.TableColumns[column.Name]);
        binding = new PersistParameterBinding(typeMapping, fieldIndex, bindingType);
        context.ParameterBindings.Add(column, binding);
      }
      return binding;
    }

    private ParameterTransmissionType GetTransmissionType(TableColumn column)
    {
      if (!useLargeObjects)
        return ParameterTransmissionType.Regular;
      switch (column.DataType.Type) {
      case SqlType.VarCharMax:
        return ParameterTransmissionType.CharacterLob;
      case SqlType.VarBinaryMax:
        return ParameterTransmissionType.BinaryLob;
      default:
        return ParameterTransmissionType.Regular;
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
    protected override void Initialize()
    {
      domainHandler = Handlers.DomainHandler;
      driver = Handlers.StorageDriver;
      providerInfo = Handlers.ProviderInfo;
      useLargeObjects = Handlers.ProviderInfo.Supports(ProviderFeatures.LargeObjects);
    }
    
    // Constructors

    /// <summary>
    /// Initializes a new instance of this class.
    /// </summary>
    public PersistRequestBuilder()
    {
    }
  }
}
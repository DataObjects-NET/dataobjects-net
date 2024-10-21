// Copyright (C) 2008-2024 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Dmitri Maximov
// Created:    2008.08.28

using System;
using System.Collections.Generic;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Model;
using Xtensive.Sql;
using Xtensive.Sql.Dml;
using Xtensive.Sql.Model;

namespace Xtensive.Orm.Providers
{
  /// <summary>
  /// Builder of <see cref="PersistRequest"/>s.
  /// </summary>
  public class PersistRequestBuilder : DomainBoundHandler
  {
    private bool useLargeObjects;
    private ProviderInfo providerInfo;
    private StorageDriver driver;

    internal IReadOnlyCollection<PersistRequest> Build(StorageNode node, PersistRequestBuilderTask task)
    {
      var context = new PersistRequestBuilderContext(task, node.Mapping, node.Configuration);
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
        return new List<PersistRequest> {batchRequest}.AsReadOnly();
      }

      foreach (var item in result) {
        item.Prepare();
      }

      return result.AsReadOnly();
    }

    protected virtual List<PersistRequest> BuildInsertRequest(PersistRequestBuilderContext context)
    {
      var result = new List<PersistRequest>();
      foreach (var index in context.AffectedIndexes) {
        var table = context.Mapping[index.ReflectedType];
        var tableRef = SqlDml.TableRef(table);
        var query = SqlDml.Insert(tableRef);
        var bindings = new List<PersistParameterBinding>();

        var row = new Dictionary<SqlColumn, SqlExpression>(index.Columns.Count);
        foreach (var column in index.Columns) {
          var fieldIndex = GetFieldIndex(context.Type, column);
          if (fieldIndex >= 0) {
            var binding = GetBinding(context, column, table, fieldIndex);
            row.Add(tableRef[column.Name], binding.ParameterReference);
            bindings.Add(binding);
          }
        }
        query.ValueRows.Add(row);

        result.Add(new PersistRequest(driver, query, bindings));
      }
      return result;
    }

    protected virtual List<PersistRequest> BuildUpdateRequest(PersistRequestBuilderContext context)
    {
      var result = new List<PersistRequest>();
      foreach (var index in context.AffectedIndexes) {
        var table = context.Mapping[index.ReflectedType];
        var tableRef = SqlDml.TableRef(table);
        var query = SqlDml.Update(tableRef);
        var bindings = new List<PersistParameterBinding>();

        foreach (var column in index.Columns) {
          var fieldIndex = GetFieldIndex(context.Type, column);
          if (fieldIndex >= 0 && context.Task.ChangedFields[fieldIndex]) {
            var binding = GetBinding(context, column, table, fieldIndex);
            query.Values[tableRef[column.Name]] = binding.ParameterReference;
            bindings.Add(binding);
          }
        }

        // There is nothing to update in this table, skip update
        // unless this table has version columns
        // in this case we issue a dummy update that changes
        // only version column(s).

        var hasColumnUpdates = query.Values.Count > 0;
        var requiresVersionValidation = context.Task.ValidateVersion;
        var isValidRequest = hasColumnUpdates
          || requiresVersionValidation && AddFakeVersionColumnUpdate(context, query, tableRef);

        if (!isValidRequest) {
          continue;
        }

        query.Where = BuildKeyFilter(context, tableRef, bindings);
        if (requiresVersionValidation) {
          query.Where &= BuildVersionFilter(context, tableRef, bindings);
        }
        result.Add(new PersistRequest(driver, query, bindings));
      }

      return result;
    }

    protected virtual List<PersistRequest> BuildRemoveRequest(PersistRequestBuilderContext context)
    {
      var result = new List<PersistRequest>();
      for (var i = context.AffectedIndexes.Count - 1; i >= 0; i--) {
        var index = context.AffectedIndexes[i];
        var tableRef = SqlDml.TableRef(context.Mapping[index.ReflectedType]);
        var query = SqlDml.Delete(tableRef);
        var bindings = new List<PersistParameterBinding>();
        query.Where = BuildKeyFilter(context, tableRef, bindings);
        if (context.Task.ValidateVersion) {
          query.Where &= BuildVersionFilter(context, tableRef, bindings);
        }
        result.Add(new PersistRequest(driver, query, bindings));
      }
      return result;
    }

    private SqlExpression BuildKeyFilter(PersistRequestBuilderContext context, SqlTableRef filteredTable, List<PersistParameterBinding> currentBindings)
    {
      SqlExpression result = null;
      foreach (var column in context.PrimaryIndex.KeyColumns.Keys) {
        if (!context.ParameterBindings.TryGetValue(column, out var binding)) {
          var typeMapping = driver.GetTypeMapping(column);
          var fieldIndex = GetFieldIndex(context.Type, column);
          binding = new PersistParameterBinding(typeMapping, fieldIndex);
          context.ParameterBindings.Add(column, binding);
        }
        result &= filteredTable[column.Name] == binding.ParameterReference;
        currentBindings.Add(binding);
      }
      return result;
    }

    private SqlExpression BuildVersionFilter(PersistRequestBuilderContext context, SqlTableRef filteredTable, List<PersistParameterBinding> currentBindings)
    {
      SqlExpression result = null;
      foreach (var column in context.Type.GetVersionColumns()) {
        var fieldIndex = GetFieldIndex(context.Type, column);
        if (!context.Task.AvailableFields[fieldIndex]) {
          continue;
        }

        if (!context.VersionParameterBindings.TryGetValue(column, out var binding)) {
          var typeMapping = driver.GetTypeMapping(column);
          binding = new PersistParameterBinding(typeMapping, fieldIndex, ParameterTransmissionType.Regular, PersistParameterBindingType.VersionFilter);
          context.VersionParameterBindings.Add(column, binding);
        }
        var filteredColumn = filteredTable[column.Name];
        if (filteredColumn is null) {
          continue;
        }
        var filterValue = binding.ParameterReference;
        // Handle decimal precision issue
        if (Type.GetTypeCode(column.ValueType) == TypeCode.Decimal) {
          filterValue = SqlDml.Cast(filterValue, driver.MapValueType(column));
        }

        result &= SqlDml.Variant(binding, filteredColumn == filterValue, SqlDml.IsNull(filteredColumn));
        currentBindings.Add(binding);
      }
      return result;
    }

    private bool AddFakeVersionColumnUpdate(PersistRequestBuilderContext context, SqlUpdate update, SqlTableRef filteredTable)
    {
      foreach (var column in context.Type.GetVersionColumns()) {
        var columnExpression = filteredTable[column.Name];
        if (columnExpression is null) {
          continue;
        }
        var index = GetFieldIndex(context.Type, column);
        if (index < 0 || !context.Task.AvailableFields[index]) {
          continue;
        }
        update.Values.Add(columnExpression, columnExpression);
        return true;
      }
      return false;
    }

    private PersistParameterBinding GetBinding(PersistRequestBuilderContext context, ColumnInfo column, Table table, int fieldIndex)
    {
      if (!context.ParameterBindings.TryGetValue(column, out var binding)) {
        var typeMapping = driver.GetTypeMapping(column);
        var bindingType = GetTransmissionType(table.TableColumns[column.Name]);
        binding = new PersistParameterBinding(typeMapping, fieldIndex, bindingType);
        context.ParameterBindings.Add(column, binding);
      }
      return binding;
    }

    private ParameterTransmissionType GetTransmissionType(TableColumn column)
    {
      if (!useLargeObjects) {
        return ParameterTransmissionType.Regular;
      }

      if (column.DataType.Type == SqlType.VarCharMax) {
        return ParameterTransmissionType.CharacterLob;
      }

      return column.DataType.Type == SqlType.VarBinaryMax
        ? ParameterTransmissionType.BinaryLob
        : ParameterTransmissionType.Regular;
    }

    private static int GetFieldIndex(TypeInfo type, ColumnInfo column)
    {
      if (!type.Fields.TryGetValue(column.Field.Name, out var field)
        || field.Column == null
        || field.Column.ValueType != column.ValueType) {
        return -1;
      }
      return field.MappingInfo.Offset;
    }

    /// <inheritdoc/>
    protected override void Initialize()
    {
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
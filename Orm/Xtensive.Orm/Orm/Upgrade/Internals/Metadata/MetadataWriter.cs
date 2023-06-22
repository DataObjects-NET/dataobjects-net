// Copyright (C) 2012-2020 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Denis Krjuchkov
// Created:    2012.03.22

using System;
using System.Collections.Generic;
using System.Linq;
using Xtensive.Orm.Providers;
using Xtensive.Reflection;
using Xtensive.Sql;
using Xtensive.Sql.Dml;
using Xtensive.Sql.Model;
using Xtensive.Tuples;
using ArgumentValidator = Xtensive.Core.ArgumentValidator;
using Tuple = Xtensive.Tuples.Tuple;

namespace Xtensive.Orm.Upgrade
{
  internal sealed class MetadataWriter
  {
    private static readonly TupleDescriptor IntStringDescriptor =
      TupleDescriptor.Create(new[] {WellKnownTypes.Int32, WellKnownTypes.String});
    private static readonly TupleDescriptor StringStringDescriptor =
      TupleDescriptor.Create(new[] {WellKnownTypes.String, WellKnownTypes.String});

    private sealed class Descriptor : IPersistDescriptor
    {
      public PersistRequest StoreRequest { get; set; }
      public PersistRequest ClearRequest { get; set; }
    }

    private readonly StorageDriver driver;
    private readonly MetadataMapping mapping;
    private readonly SqlExtractionTask task;
    private readonly IProviderExecutor executor;

    public void Write(MetadataSet metadata)
    {
      WriteTypes(metadata.Types);
      WriteAssemblies(metadata.Assemblies);
      WriteExtensions(metadata.Extensions);
    }

    #region Private / internal methods

    private void WriteExtensions(IEnumerable<ExtensionMetadata> extensions)
    {
      var extensionTextTransmissionType =
        driver.ProviderInfo.Supports(ProviderFeatures.LargeObjects)
        ? ParameterTransmissionType.CharacterLob
        : ParameterTransmissionType.Regular;
      var descriptor = CreateDescriptor(mapping.Extension,
        mapping.StringMapping, mapping.ExtensionName, ParameterTransmissionType.Regular,
        mapping.StringMapping, mapping.ExtensionText, extensionTextTransmissionType,
        ProvideExtensionMetadataFilter);

      executor.Overwrite(descriptor, extensions.Select(item => (Tuple) Tuple.Create(StringStringDescriptor, item.Name, item.Value)));
    }

    private void ProvideExtensionMetadataFilter(SqlDelete delete)
    {
      var knownExtensions = SqlDml.Array(WellKnown.DomainModelExtensionName, WellKnown.PartialIndexDefinitionsExtensionName);
      delete.Where = SqlDml.In(delete.Delete[mapping.ExtensionName], knownExtensions);
    }

    private void WriteTypes(IEnumerable<TypeMetadata> types)
    {
      var descriptor = CreateDescriptor(mapping.Type,
        mapping.IntMapping, mapping.TypeId, ParameterTransmissionType.Regular,
        mapping.StringMapping, mapping.TypeName, ParameterTransmissionType.Regular);

      executor.Overwrite(descriptor, types.Select(item => (Tuple) Tuple.Create(IntStringDescriptor, item.Id, item.Name)));
    }

    private void WriteAssemblies(IEnumerable<AssemblyMetadata> assemblies)
    {
      var descriptor = CreateDescriptor(mapping.Assembly,
        mapping.StringMapping, mapping.AssemblyName, ParameterTransmissionType.Regular,
        mapping.StringMapping, mapping.AssemblyVersion, ParameterTransmissionType.Regular);

      executor.Overwrite(descriptor, assemblies.Select(item => (Tuple) Tuple.Create(StringStringDescriptor, item.Name, item.Version)));
    }

    private IPersistDescriptor CreateDescriptor(string tableName,
      TypeMapping mapping1, string columnName1, ParameterTransmissionType transmissionType1,
      TypeMapping mapping2, string columnName2, ParameterTransmissionType transmissionType2,
      Action<SqlDelete> deleteTransform = null)
    {
      var catalog = new Catalog(task.Catalog);
      var schema = catalog.CreateSchema(task.Schema);
      var table = schema.CreateTable(tableName);

      var columnNames = new[] {columnName1, columnName2};
      var mappings = new[] {mapping1, mapping2};
      var transmissionTypes = new[] {transmissionType1, transmissionType2};

      var columns = columnNames.Select(table.CreateColumn).ToList();
      var tableRef = SqlDml.TableRef(table);

      var insert = SqlDml.Insert(tableRef);
      var bindings = new PersistParameterBinding[columns.Count];
      var row = new Dictionary<SqlColumn, SqlExpression>(columns.Count);
      for (int i = 0; i < columns.Count; i++) {
        var binding = new PersistParameterBinding(mappings[i], i, transmissionTypes[i]);
        row.Add(tableRef.Columns[i], binding.ParameterReference);
        bindings[i] = binding;
      }
      insert.ValueRows.Add(row);

      var delete = SqlDml.Delete(tableRef);
      if (deleteTransform!=null)
        deleteTransform.Invoke(delete);

      var storeRequest = new PersistRequest(driver, insert, bindings);
      var clearRequest = new PersistRequest(driver, delete, null);

      storeRequest.Prepare();
      clearRequest.Prepare();

      return new Descriptor {
        StoreRequest = storeRequest,
        ClearRequest = clearRequest
      };
    }

    #endregion

    // Constructors

    public MetadataWriter(StorageDriver driver, MetadataMapping mapping, SqlExtractionTask task, IProviderExecutor executor)
    {
      ArgumentValidator.EnsureArgumentNotNull(driver, "driver");
      ArgumentValidator.EnsureArgumentNotNull(mapping, "mapping");
      ArgumentValidator.EnsureArgumentNotNull(task, "task");
      ArgumentValidator.EnsureArgumentNotNull(executor, "executor");

      this.driver = driver;
      this.mapping = mapping;
      this.task = task;
      this.executor = executor;
    }
  }
}
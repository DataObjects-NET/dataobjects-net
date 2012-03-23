// Copyright (C) 2012 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2012.03.22

using System.Collections.Generic;
using System.Linq;
using Xtensive.Orm.Providers;
using Xtensive.Sql;
using Xtensive.Sql.Model;
using Xtensive.Tuples;
using ArgumentValidator = Xtensive.Core.ArgumentValidator;

namespace Xtensive.Orm.Upgrade
{
  internal sealed class MetadataWriter
  {
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
      var descriptor = CreateDescriptor(mapping.Extension,
        mapping.StringMapping, mapping.ExtensionName,
        mapping.StringMapping, mapping.ExtensionText);

      executor.Overwrite(descriptor, extensions.Select(item => Tuple.Create(item.Name, item.Value)));
    }

    private void WriteTypes(IEnumerable<TypeMetadata> types)
    {
      var descriptor = CreateDescriptor(mapping.Type,
        mapping.IntMapping, mapping.TypeId,
        mapping.StringMapping, mapping.TypeName);

      executor.Overwrite(descriptor, types.Select(item => Tuple.Create(item.Id, item.Name)));
    }

    private void WriteAssemblies(IEnumerable<AssemblyMetadata> assemblies)
    {
      var descriptor = CreateDescriptor(mapping.Assembly,
        mapping.StringMapping, mapping.AssemblyName,
        mapping.StringMapping, mapping.AssemblyVersion);

      executor.Overwrite(descriptor, assemblies.Select(item => Tuple.Create(item.Name, item.Version)));
    }

    private IPersistDescriptor CreateDescriptor(string tableName,
      TypeMapping mapping1, string columnName1, TypeMapping mapping2, string columnName2)
    {
      var catalog = new Catalog(task.Catalog);
      var schema = catalog.CreateSchema(task.Schema);
      var table = schema.CreateTable(tableName);

      var columnNames = new[] {columnName1, columnName2};
      var mappings = new[] {mapping1, mapping2};

      var columns = columnNames.Select(table.CreateColumn).ToList();
      var tableRef = SqlDml.TableRef(table);

      var insert = SqlDml.Insert(tableRef);
      var bindings = new PersistParameterBinding[columns.Count];
      for (int i = 0; i < columns.Count; i++) {
        var binding = new PersistParameterBinding(mappings[i], i);
        insert.Values[tableRef.Columns[i]] = binding.ParameterReference;
        bindings[i] = binding;
      }

      var storeRequest = new PersistRequest(driver, insert, bindings);
      var clearRequest = new PersistRequest(driver, SqlDml.Delete(tableRef), null);

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
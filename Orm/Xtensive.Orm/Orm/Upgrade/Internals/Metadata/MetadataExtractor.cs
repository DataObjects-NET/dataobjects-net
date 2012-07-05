// Copyright (C) 2012 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2012.02.16

using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using Xtensive.Core;
using Xtensive.Orm.Providers;
using Xtensive.Sql;
using Xtensive.Sql.Dml;
using Xtensive.Sql.Model;

namespace Xtensive.Orm.Upgrade
{
  internal sealed class MetadataExtractor
  {
    private readonly MetadataMapping mapping;
    private readonly ISqlExecutor executor;
    private readonly SqlExtractionTask task;

    public void Extract(MetadataSet output)
    {
      // Make sure we fill output only when all records has been read.

      var assemblies = new List<AssemblyMetadata>();
      var types = new List<TypeMetadata>();
      var extensions = new List<ExtensionMetadata>();

      ExtractAssemblies(assemblies);
      ExtractTypes(types);
      ExtractExtensions(extensions);

      output.Assemblies.AddRange(assemblies);
      output.Types.AddRange(types);
      output.Extensions.AddRange(extensions);
    }

    #region Private / internal methods

    private void ExtractAssemblies(ICollection<AssemblyMetadata> output)
    {
      var query = CreateQuery(mapping.Assembly, mapping.AssemblyName, mapping.AssemblyVersion);
      ExecuteQuery(output, query, ParseAssembly);
    }

    private void ExtractTypes(ICollection<TypeMetadata> output)
    {
      var query = CreateQuery(mapping.Type, mapping.TypeId, mapping.TypeName);
      ExecuteQuery(output, query, ParseType);
    }

    private void ExtractExtensions(ICollection<ExtensionMetadata> output)
    {
      var query = CreateQuery(mapping.Extension, mapping.ExtensionName, mapping.ExtensionText);
      ExecuteQuery(output, query, ParseExtension);
    }

    private ExtensionMetadata ParseExtension(DbDataReader reader)
    {
      var name = ReadString(reader, 0);
      var text = ReadString(reader, 1);
      return new ExtensionMetadata(name, text);
    }

    private AssemblyMetadata ParseAssembly(DbDataReader reader)
    {
      var name = ReadString(reader, 0);
      var version = ReadString(reader, 1);
      return new AssemblyMetadata(name, version);
    }

    private TypeMetadata ParseType(DbDataReader reader)
    {
      var id = ReadInt(reader, 0);
      var name = ReadString(reader, 1);
      return new TypeMetadata(id, name);
    }

    private void ExecuteQuery<T>(ICollection<T> output, ISqlCompileUnit query, Func<DbDataReader, T> parser)
    {
      using (var command = executor.ExecuteReader(query)) {
        var reader = command.Reader;
        while (reader.Read())
          output.Add(parser.Invoke(reader));
      }
    }
 
    private SqlSelect CreateQuery(string tableName, params string[] columnNames)
    {
      var catalog = new Catalog(task.Catalog);
      var schema = catalog.CreateSchema(task.Schema);
      var table = schema.CreateTable(tableName);
      foreach (var column in columnNames)
        table.CreateColumn(column);
      var tableRef = SqlDml.TableRef(table);
      var select = SqlDml.Select(tableRef);
      var columnRefs = columnNames
        .Select((c, i) => SqlDml.ColumnRef(tableRef.Columns[i], c));
      foreach (var columnRef in columnRefs)
        select.Columns.Add(columnRef);
      return select;
    }

    private string ReadString(DbDataReader reader, int index)
    {
      return reader.IsDBNull(index) ? null : (string) mapping.StringMapping.ReadValue(reader, index);
    }

    private int ReadInt(DbDataReader reader, int index)
    {
      return (int) mapping.IntMapping.ReadValue(reader, index);
    }

    #endregion

    // Constructors

    public MetadataExtractor(MetadataMapping mapping, SqlExtractionTask task, ISqlExecutor executor)
    {
      ArgumentValidator.EnsureArgumentNotNull(mapping, "mapping");
      ArgumentValidator.EnsureArgumentNotNull(task, "task");
      ArgumentValidator.EnsureArgumentNotNull(executor, "executor");

      this.mapping = mapping;
      this.task = task;
      this.executor = executor;
    }
  }
}
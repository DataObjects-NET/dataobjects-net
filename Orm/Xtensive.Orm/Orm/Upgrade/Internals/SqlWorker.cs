// Copyright (C) 2012 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2012.03.15

using System;
using System.Collections.Generic;
using System.Linq;
using Xtensive.Orm.Providers;
using Xtensive.Sql;
using Xtensive.Sql.Model;

namespace Xtensive.Orm.Upgrade
{
  internal static class SqlWorker
  {
    public static Func<SqlWorkerResult> Create(UpgradeServiceAccessor services, SqlWorkerTask task)
    {
      return () => Run(services, task);
    }

    private static SqlWorkerResult Run(UpgradeServiceAccessor services, SqlWorkerTask task)
    {
      var result = new SqlWorkerResult();
      using (var connection = services.Driver.CreateConnection(null)) {
        connection.Open();
        var executor = new SqlExecutor(services.Driver, connection);
        if ((task & SqlWorkerTask.DropSchema) > 0)
          DropSchema(services, executor);
        if ((task & SqlWorkerTask.ExtractSchema) > 0)
          result.Schema = ExtractSchema(services, executor);
        if ((task & SqlWorkerTask.ExtractMetadata) > 0)
          ExtractMetadata(services, executor, result);

        connection.Close();
      }
      return result;
    }

    private static void ExtractMetadata(UpgradeServiceAccessor services, ISqlExecutor executor, SqlWorkerResult result)
    {
      var extractors = new List<MetadataExtractor>();
      var mapping = new MetadataMapping(services.Driver, services.NameBuilder);

      result.Assemblies = new List<AssemblyMetadata>();
      result.Extensions = new List<ExtensionMetadata>();
      result.Types = new List<TypeMetadata>();

      foreach (var task in services.Resolver.GetMetadataTasks()) {
        var extractor = new MetadataExtractor(mapping, task, executor);
        extractor.GetAssemblies(result.Assemblies);
        extractor.GetExtensions(result.Extensions);
        extractor.GetTypes(result.Types);
      }
    }

    private static SqlExtractionResult ExtractSchema(UpgradeServiceAccessor services, ISqlExecutor executor)
    {
      return executor.Extract(services.Resolver.GetSchemaTasks());
    }

    private static void DropSchema(UpgradeServiceAccessor services, ISqlExecutor executor)
    {
      var driver = services.Driver;
      var extractionResult = ExtractSchema(services, executor);
      var schemas = extractionResult.Catalogs.SelectMany(c => c.Schemas).ToList();
      var tables = schemas.SelectMany(s => s.Tables).ToList();
      var sequences = schemas.SelectMany(s => s.Sequences);

      DropForeignKeys(driver, tables, executor);
      DropTables(driver, tables, executor);
      DropSequences(driver, sequences, executor);
    }

    private static void DropSequences(StorageDriver driver, IEnumerable<Sequence> sequences, ISqlExecutor executor)
    {
      var statements = sequences
        .Select(s => driver.Compile(SqlDdl.Drop(s)).GetCommandText())
        .ToList();
      executor.ExecuteDdl(statements);
    }

    private static void DropTables(StorageDriver driver, IEnumerable<Table> tables, ISqlExecutor executor)
    {
      var statements = tables
        .Select(t => driver.Compile(SqlDdl.Drop(t)).GetCommandText())
        .ToList();
      executor.ExecuteDdl(statements);
    }

    private static void DropForeignKeys(StorageDriver driver, IEnumerable<Table> tables, ISqlExecutor executor)
    {
      var statements = tables
        .SelectMany(t => t.TableConstraints.OfType<ForeignKey>())
        .Select(fk => driver.Compile(SqlDdl.Alter(fk.Table, SqlDdl.DropConstraint(fk))).GetCommandText())
        .ToList();
      executor.ExecuteDdl(statements);
    }
  }
}
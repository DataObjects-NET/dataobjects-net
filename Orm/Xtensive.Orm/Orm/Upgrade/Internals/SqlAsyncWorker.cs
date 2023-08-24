// Copyright (C) 2020 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xtensive.Core;
using Xtensive.Orm.Providers;
using Xtensive.Sql;
using Xtensive.Sql.Model;

namespace Xtensive.Orm.Upgrade
{
  internal static class SqlAsyncWorker
  {
    public static Func<Task<SqlWorkerResult>> Create(
      UpgradeServiceAccessor services, SqlWorkerTask task, CancellationToken token) =>
      () => RunAsync(services, task, token);

    private static async Task<SqlWorkerResult> RunAsync(
      UpgradeServiceAccessor services, SqlWorkerTask task, CancellationToken token)
    {
      var result = new SqlWorkerResult();
      var executor = new SqlExecutor(services.StorageDriver, services.Connection);
      if ((task & SqlWorkerTask.DropSchema) > 0) {
        await DropSchemaAsync(services, executor, token).ConfigureAwaitFalse();
      }

      if ((task & SqlWorkerTask.ExtractSchema) > 0) {
        result.Schema = await ExtractSchemaAsync(services, executor, token).ConfigureAwaitFalse();
      }

      if ((task & (SqlWorkerTask.ExtractMetadataTypes | SqlWorkerTask.ExtractMetadataAssemblies | SqlWorkerTask.ExtractMetadataExtension)) > 0) {
        await ExtractMetadataAsync(services, executor, result, task, token).ConfigureAwaitFalse();
      }

      return result;
    }

    private static async Task ExtractMetadataAsync(
      UpgradeServiceAccessor services, SqlExecutor executor, SqlWorkerResult result, SqlWorkerTask task,
      CancellationToken token)
    {
      var set = new MetadataSet();
      var mapping = new MetadataMapping(services.StorageDriver, services.NameBuilder);
      var metadataExtractor = new MetadataExtractor(mapping, executor);
      foreach (var metadataTask in services.MappingResolver.GetMetadataTasks()
        .Where(metadataTask => !ShouldSkipMetadataExtraction(mapping, result, metadataTask))) {
        try {
          if (task.HasFlag(SqlWorkerTask.ExtractMetadataAssemblies)) {
            await metadataExtractor.ExtractAssembliesAsync(set, metadataTask, token).ConfigureAwaitFalse();
          }

          if (task.HasFlag(SqlWorkerTask.ExtractMetadataTypes)) {
            await metadataExtractor.ExtractTypesAsync(set, metadataTask, token).ConfigureAwaitFalse();
          }

          if (task.HasFlag(SqlWorkerTask.ExtractMetadataExtension)) {
            await metadataExtractor.ExtractExtensionsAsync(set, metadataTask, token).ConfigureAwaitFalse();
          }
        }
        catch (Exception exception) {
          UpgradeLog.Warning(
            Strings.LogFailedToExtractMetadataFromXYZ, metadataTask.Catalog, metadataTask.Schema, exception);
        }
      }
      result.Metadata = set;
    }

    private static bool ShouldSkipMetadataExtraction(MetadataMapping mapping, SqlWorkerResult result,
      SqlExtractionTask task)
    {
      if (result.Schema==null) {
        return false;
      }

      var tables = GetSchemaTables(result, task);
      return tables[mapping.Assembly]==null && tables[mapping.Type]==null && tables[mapping.Extension]==null;
    }

    private static PairedNodeCollection<Schema, Table> GetSchemaTables(SqlWorkerResult result, SqlExtractionTask task)
    {
      var catalog = GetCatalog(result, task.Catalog);
      var schema = GetSchema(catalog, task.Schema);
      return schema.Tables;
    }

    private static Catalog GetCatalog(SqlWorkerResult result, string catalogName) =>
      catalogName.IsNullOrEmpty()
        ? result.Schema.Catalogs.Single(c => c.Name == catalogName)
        : result.Schema.Catalogs[catalogName];

    private static Schema GetSchema(Catalog catalog, string schemaName) =>
      schemaName.IsNullOrEmpty()
        ? catalog.Schemas.Single(s => s.Name==schemaName)
        : catalog.Schemas[schemaName];

    private static async Task<SchemaExtractionResult> ExtractSchemaAsync(
      UpgradeServiceAccessor services, ISqlExecutor executor, CancellationToken token)
    {
      var extractionTasks = services.MappingResolver.GetSchemaTasks();
      var extractionResult = await executor.ExtractAsync(extractionTasks, token).ConfigureAwaitFalse();
      var schema = new SchemaExtractionResult(extractionResult);
      return new IgnoreRulesHandler(schema, services.Configuration, services.MappingResolver).Handle();
    }

    private static async Task DropSchemaAsync(
      UpgradeServiceAccessor services, ISqlExecutor executor, CancellationToken token)
    {
      var driver = services.StorageDriver;
      var extractionResult = await ExtractSchemaAsync(services, executor, token).ConfigureAwaitFalse();
      var schemas = extractionResult.Catalogs.SelectMany(c => c.Schemas).ToList();
      var tables = schemas.SelectMany(s => s.Tables).ToList();
      var sequences = schemas.SelectMany(s => s.Sequences);

      await DropForeignKeysAsync(driver, tables, executor, token).ConfigureAwaitFalse();
      await DropTablesAsync(driver, tables, executor, token).ConfigureAwaitFalse();
      await DropSequencesAsync(driver, sequences, executor,token).ConfigureAwaitFalse();
    }

    private static Task DropSequencesAsync(
      StorageDriver driver, IEnumerable<Sequence> sequences, ISqlExecutor executor, CancellationToken token)
    {
      var statements = sequences
        .Select(s => driver.Compile(SqlDdl.Drop(s)).GetCommandText())
        .ToList();
      return executor.ExecuteManyAsync(statements, token);
    }

    private static Task DropTablesAsync(
      StorageDriver driver, IEnumerable<Table> tables, ISqlExecutor executor, CancellationToken token)
    {
      var statements = tables
        .Select(t => driver.Compile(SqlDdl.Drop(t)).GetCommandText())
        .ToList();
      return executor.ExecuteManyAsync(statements, token);
    }

    private static Task DropForeignKeysAsync(
      StorageDriver driver, IEnumerable<Table> tables, ISqlExecutor executor, CancellationToken token)
    {
      var statements = tables
        .SelectMany(t => t.TableConstraints.OfType<ForeignKey>())
        .Select(fk => driver.Compile(SqlDdl.Alter(fk.Table, SqlDdl.DropConstraint(fk))).GetCommandText())
        .ToList();
      return executor.ExecuteManyAsync(statements, token);
    }
  }
}
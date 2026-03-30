// Copyright (C) 2017-2023 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Julian Mamokin
// Created:    2017.03.01

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using Xtensive.Core;
using Xtensive.Orm.Building.Builders;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Providers;
using Xtensive.Orm.Tests.Issues;
using Xtensive.Orm.Upgrade;
using Xtensive.Reflection;
using Xtensive.Sql;
using Xtensive.Sql.Model;

namespace Xtensive.Orm.Tests.Upgrade
{
  [TestFixture]
  public class SqlWorkerAndSqlAsyncWorkerTest
  {
    private const string TestEntityName = "SqlWorkerTestEntity";
    private const string TestEntityField = "SomeStringField";
    private const string ErrorMessage = "Failed on task: {0}";

    private UpgradeServiceAccessor accessor;

    [OneTimeSetUp]
    public void TestFixtureSetup() => CreateAccessor();

    [SetUp]
    public void Setup() => PrepareDatabase();

    [Test]
    public void ExtractMetadataTypesTest() =>
      ValidateWorkerResult(SqlWorkerTask.ExtractMetadataTypes);

    [Test]
    public async Task ExtractMetadataTypesAsyncTest() =>
      await ValidateWorkerResultAsync(SqlWorkerTask.ExtractMetadataTypes);

    [Test]
    public void ExtractMetadataExtensionTest() =>
      ValidateWorkerResult(SqlWorkerTask.ExtractMetadataExtension);

    [Test]
    public async Task ExtractMetadataExtensionAsyncTest() =>
      await ValidateWorkerResultAsync(SqlWorkerTask.ExtractMetadataExtension);

    [Test]
    public void ExtractMetadataAssembliesTest() =>
      ValidateWorkerResult(SqlWorkerTask.ExtractMetadataAssemblies);

    [Test]
    public async Task ExtractMetadataAssembliesAsycTest() =>
      await ValidateWorkerResultAsync(SqlWorkerTask.ExtractMetadataAssemblies);

    [Test]
    public void ExtractMetadataTest() =>
      ValidateWorkerResult(SqlWorkerTask.ExtractMetadata);

    [Test]
    public async Task ExtractMetadataAsyncTest() =>
      await ValidateWorkerResultAsync(SqlWorkerTask.ExtractMetadata);

    [Test]
    public void DropSchemaTest() =>
      ValidateWorkerResult(SqlWorkerTask.DropSchema);

    [Test]
    public async Task DropSchemaAsyncTest() =>
      await ValidateWorkerResultAsync(SqlWorkerTask.DropSchema);

    [Test]
    public void ExtractSchemaTest() =>
      ValidateWorkerResult(SqlWorkerTask.ExtractSchema);

    [Test]
    public async Task ExtractSchemaAsyncTest() =>
      await ValidateWorkerResultAsync(SqlWorkerTask.ExtractSchema);

    [Test]
    public void ExtractMetadataTypesAndExtensionTest() =>
      ValidateWorkerResult(SqlWorkerTask.ExtractMetadataTypes | SqlWorkerTask.ExtractMetadataExtension);

    [Test]
    public async Task ExtractMetadataTypesAndExtensionAsyncTest() =>
      await ValidateWorkerResultAsync(SqlWorkerTask.ExtractMetadataTypes | SqlWorkerTask.ExtractMetadataExtension);

    [Test]
    public void ExtractMetadataTypesAndAssembliesTest() =>
      ValidateWorkerResult(SqlWorkerTask.ExtractMetadataTypes | SqlWorkerTask.ExtractMetadataAssemblies);

    [Test]
    public async Task ExtractMetadataTypesAndAssembliesAsyncTest() =>
      await ValidateWorkerResultAsync(SqlWorkerTask.ExtractMetadataTypes | SqlWorkerTask.ExtractMetadataAssemblies);

    [Test]
    public void ExtractMetadataAssembliesAndExtensionTest() =>
      ValidateWorkerResult(SqlWorkerTask.ExtractMetadataAssemblies | SqlWorkerTask.ExtractMetadataExtension);

    [Test]
    public async Task ExtractMetadataAssembliesAndExtensionAsyncTest() =>
      await ValidateWorkerResultAsync(SqlWorkerTask.ExtractMetadataAssemblies | SqlWorkerTask.ExtractMetadataExtension);

    [Test]
    public void DropSchemaAndExtractMetadataTypesTest() =>
      ValidateWorkerResult(SqlWorkerTask.DropSchema | SqlWorkerTask.ExtractMetadataTypes);

    [Test]
    public async Task DropSchemaAndExtractMetadataTypesAsyncTest() =>
      await ValidateWorkerResultAsync(SqlWorkerTask.DropSchema | SqlWorkerTask.ExtractMetadataTypes);

    [Test]
    public void DropSchemaAndExtractMetadataExtensionTest() =>
      ValidateWorkerResult(SqlWorkerTask.DropSchema | SqlWorkerTask.ExtractMetadataExtension);

    [Test]
    public async Task DropSchemaAndExtractMetadataExtensionAsyncTest() =>
      await ValidateWorkerResultAsync(SqlWorkerTask.DropSchema | SqlWorkerTask.ExtractMetadataExtension);

    [Test]
    public void DropSchemaAndExtractMetadataAssembliesTest() =>
      ValidateWorkerResult(SqlWorkerTask.DropSchema | SqlWorkerTask.ExtractMetadataAssemblies);

    [Test]
    public async Task DropSchemaAndExtractMetadataAssembliesAsyncTest() =>
      await ValidateWorkerResultAsync(SqlWorkerTask.DropSchema | SqlWorkerTask.ExtractMetadataAssemblies);

    [Test]
    public void DropSchemaAndExtractMetadataTest() =>
      ValidateWorkerResult(SqlWorkerTask.DropSchema | SqlWorkerTask.ExtractMetadata);

    [Test]
    public async Task DropSchemaAndExtractMetadataAsyncTest() =>
      await ValidateWorkerResultAsync(SqlWorkerTask.DropSchema | SqlWorkerTask.ExtractMetadata);

    [Test]
    public void DropSchemaAndExtractSchemaTest() =>
      ValidateWorkerResult(SqlWorkerTask.DropSchema | SqlWorkerTask.ExtractSchema);

    [Test]
    public async Task DropSchemaAndExtractSchemaAsyncTest() =>
      await ValidateWorkerResultAsync(SqlWorkerTask.DropSchema | SqlWorkerTask.ExtractSchema);

    [Test]
    public void ExtractSchemaAndMetadataTypesTest() =>
      ValidateWorkerResult(SqlWorkerTask.ExtractSchema | SqlWorkerTask.ExtractMetadataTypes);

    [Test]
    public async Task ExtractSchemaAndMetadataTypesAsyncTest() =>
      await ValidateWorkerResultAsync(SqlWorkerTask.ExtractSchema | SqlWorkerTask.ExtractMetadataTypes);

    [Test]
    public void ExtractSchemaAndMetadataExtensionTest() =>
      ValidateWorkerResult(SqlWorkerTask.ExtractSchema | SqlWorkerTask.ExtractMetadataExtension);

    [Test]
    public async Task ExtractSchemaAndMetadataExtensionAsyncTest() =>
      await ValidateWorkerResultAsync(SqlWorkerTask.ExtractSchema | SqlWorkerTask.ExtractMetadataExtension);

    [Test]
    public void ExtractSchemaAndMetadataAssemblyTest() =>
      ValidateWorkerResult(SqlWorkerTask.ExtractSchema | SqlWorkerTask.ExtractMetadataAssemblies);

    [Test]
    public async Task ExtractSchemaAndMetadataAssemblyAsycTest() =>
      await ValidateWorkerResultAsync(SqlWorkerTask.ExtractSchema | SqlWorkerTask.ExtractMetadataAssemblies);

    [Test]
    public void ExtractSchemaAndMetadataTest() =>
      ValidateWorkerResult(SqlWorkerTask.ExtractSchema | SqlWorkerTask.ExtractMetadata);

    [Test]
    public async Task ExtractSchemaAndMetadataAsyncTest() =>
      await ValidateWorkerResultAsync(SqlWorkerTask.ExtractSchema | SqlWorkerTask.ExtractMetadata);

    [Test]
    public void ExtractSchemaMetadataTypesAndExtensionTest() =>
      ValidateWorkerResult(
        SqlWorkerTask.ExtractSchema
        | SqlWorkerTask.ExtractMetadataTypes
        | SqlWorkerTask.ExtractMetadataExtension);

    [Test]
    public async Task ExtractSchemaMetadataTypesAndExtensionAsyncTest() =>
      await ValidateWorkerResultAsync(
        SqlWorkerTask.ExtractSchema
        | SqlWorkerTask.ExtractMetadataTypes
        | SqlWorkerTask.ExtractMetadataExtension);

    [Test]
    public void ExtractSchemaMetadataTypesAndAssembliesTest() =>
      ValidateWorkerResult(
        SqlWorkerTask.ExtractSchema 
        | SqlWorkerTask.ExtractMetadataTypes 
        | SqlWorkerTask.ExtractMetadataAssemblies);

    [Test]
    public async Task ExtractSchemaMetadataTypesAndAssembliesAsyncTest() =>
      await ValidateWorkerResultAsync(
        SqlWorkerTask.ExtractSchema
        | SqlWorkerTask.ExtractMetadataTypes
        | SqlWorkerTask.ExtractMetadataAssemblies);

    [Test]
    public void ExtractSchemaMetadataAssembliesAndExtensionTest() =>
      ValidateWorkerResult(
        SqlWorkerTask.ExtractSchema
        | SqlWorkerTask.ExtractMetadataAssemblies
        | SqlWorkerTask.ExtractMetadataExtension);

    [Test]
    public async Task ExtractSchemaMetadataAssembliesAndExtensionAsyncTest() =>
      await ValidateWorkerResultAsync(
        SqlWorkerTask.ExtractSchema
        | SqlWorkerTask.ExtractMetadataAssemblies
        | SqlWorkerTask.ExtractMetadataExtension);

    [Test]
    public void DropSchemaAndExtractSchemaAndMetadataTypesTest() =>
      ValidateWorkerResult(
        SqlWorkerTask.DropSchema
        | SqlWorkerTask.ExtractSchema
        | SqlWorkerTask.ExtractMetadataTypes);

    [Test]
    public async Task DropSchemaAndExtractSchemaAndMetadataTypesAsyncTest() =>
      await ValidateWorkerResultAsync(
        SqlWorkerTask.DropSchema
        | SqlWorkerTask.ExtractSchema
        | SqlWorkerTask.ExtractMetadataTypes);

    [Test]
    public void DropSchemaAndExtractSchemaAndMetadataExtensionTest() =>
      ValidateWorkerResult(
        SqlWorkerTask.DropSchema
        | SqlWorkerTask.ExtractSchema
        | SqlWorkerTask.ExtractMetadataExtension);

    [Test]
    public async Task DropSchemaAndExtractSchemaAndMetadataExtensionAsyncTest() =>
      await ValidateWorkerResultAsync(
        SqlWorkerTask.DropSchema
        | SqlWorkerTask.ExtractSchema
        | SqlWorkerTask.ExtractMetadataExtension);

    [Test]
    public void DropSchemaAndExtractSchemaAndMetadataAssembliesTest() =>
      ValidateWorkerResult(
        SqlWorkerTask.DropSchema
        | SqlWorkerTask.ExtractSchema
        | SqlWorkerTask.ExtractMetadataAssemblies);

    [Test]
    public async Task DropSchemaAndExtractSchemaAndMetadataAssembliesAsyncTest() =>
      await ValidateWorkerResultAsync(
        SqlWorkerTask.DropSchema
        | SqlWorkerTask.ExtractSchema
        | SqlWorkerTask.ExtractMetadataAssemblies);

    [Test]
    public void DropSchemaAndExtractSchemaAndMetadataTest() =>
      ValidateWorkerResult(
        SqlWorkerTask.DropSchema
        | SqlWorkerTask.ExtractSchema
        | SqlWorkerTask.ExtractMetadata);

    [Test]
    public async Task DropSchemaAndExtractSchemaAndMetadataAsyncTest() =>
      await ValidateWorkerResultAsync(
        SqlWorkerTask.DropSchema
        | SqlWorkerTask.ExtractSchema
        | SqlWorkerTask.ExtractMetadata);

    [Test]
    public void DropSchemaAndExtractMetadataTypesAndExtensionTest() =>
      ValidateWorkerResult(
        SqlWorkerTask.DropSchema
        | SqlWorkerTask.ExtractMetadataTypes
        | SqlWorkerTask.ExtractMetadataExtension);

    [Test]
    public async Task DropSchemaAndExtractMetadataTypesAndExtensionAsyncTest() => 
      await ValidateWorkerResultAsync(
        SqlWorkerTask.DropSchema
        | SqlWorkerTask.ExtractMetadataTypes
        | SqlWorkerTask.ExtractMetadataExtension);

    [Test]
    public void DropSchemaAndExtractMetadataTypesAndAssembliesTest() =>
      ValidateWorkerResult(
        SqlWorkerTask.DropSchema
        | SqlWorkerTask.ExtractMetadataTypes
        | SqlWorkerTask.ExtractMetadataAssemblies);

    [Test]
    public async Task DropSchemaAndExtractMetadataTypesAndAssembliesAsyncTest() =>
      await ValidateWorkerResultAsync(
        SqlWorkerTask.DropSchema
        | SqlWorkerTask.ExtractMetadataTypes
        | SqlWorkerTask.ExtractMetadataAssemblies);

    [Test]
    public void DropSchemaAndExtractMetadataAssembliesAndExtensionTest() =>
      ValidateWorkerResult(
        SqlWorkerTask.DropSchema
        | SqlWorkerTask.ExtractMetadataAssemblies
        | SqlWorkerTask.ExtractMetadataExtension);

    [Test]
    public async Task DropSchemaAndExtractMetadataAssembliesAndExtensionAsyncTest() =>
      await ValidateWorkerResultAsync(
        SqlWorkerTask.DropSchema
        | SqlWorkerTask.ExtractMetadataAssemblies
        | SqlWorkerTask.ExtractMetadataExtension);

    [Test]
    public void DropSchemaAndExtractSchemaAndMetadataTypesAndExtensionTest() =>
      ValidateWorkerResult(
        SqlWorkerTask.DropSchema
        | SqlWorkerTask.ExtractSchema
        | SqlWorkerTask.ExtractMetadataTypes
        | SqlWorkerTask.ExtractMetadataExtension);

    [Test]
    public async Task DropSchemaAndExtractSchemaAndMetadataTypesAndExtensionAsyncTest() => 
      await ValidateWorkerResultAsync(
        SqlWorkerTask.DropSchema
        | SqlWorkerTask.ExtractSchema
        | SqlWorkerTask.ExtractMetadataTypes
        | SqlWorkerTask.ExtractMetadataExtension);
    

    [Test]
    public void DropSchemaAndExtractSchemaAndMetadataTypesAndAssembliesTest() =>
      ValidateWorkerResult(
        SqlWorkerTask.DropSchema
        | SqlWorkerTask.ExtractSchema 
        | SqlWorkerTask.ExtractMetadataTypes 
        | SqlWorkerTask.ExtractMetadataAssemblies);

    [Test]
    public async Task DropSchemaAndExtractSchemaAndMetadataTypesAndAssembliesAsyncTest() =>
      await ValidateWorkerResultAsync(
        SqlWorkerTask.DropSchema
        | SqlWorkerTask.ExtractSchema
        | SqlWorkerTask.ExtractMetadataTypes
        | SqlWorkerTask.ExtractMetadataAssemblies);

    [Test]
    public void DropSchemaAndExtractSchemaAndMetadataAssembliesAndExtensionTest() =>
      ValidateWorkerResult(
        SqlWorkerTask.DropSchema
        | SqlWorkerTask.ExtractSchema
        | SqlWorkerTask.ExtractMetadataAssemblies
        | SqlWorkerTask.ExtractMetadataExtension);

    [Test]
    public async Task DropSchemaAndExtractSchemaAndMetadataAssembliesAndExtensionAsyncTest() =>
      await ValidateWorkerResultAsync(
        SqlWorkerTask.DropSchema
        | SqlWorkerTask.ExtractSchema
        | SqlWorkerTask.ExtractMetadataAssemblies
        | SqlWorkerTask.ExtractMetadataExtension);

    private void ValidateWorkerResult(SqlWorkerTask task)
    {
      var result = SqlWorker.Create(accessor, task).Invoke();
      var error = string.Format(ErrorMessage, task);

      ValidateSchema(task, result);
      if (task == SqlWorkerTask.DropSchema) {
        Assert.IsTrue(result.Schema == null, error);
        Assert.IsTrue(result.Metadata == null, error);
        return;
      }
      ValidateMetadata(task, result, task.HasFlag(SqlWorkerTask.DropSchema));
    }

    private async Task ValidateWorkerResultAsync(SqlWorkerTask task)
    {
      var result = await SqlAsyncWorker.Create(accessor, task, CancellationToken.None).Invoke();
      var error = string.Format(ErrorMessage, task);

      ValidateSchema(task, result);
      if (task == SqlWorkerTask.DropSchema) {
        Assert.IsTrue(result.Schema == null, error);
        Assert.IsTrue(result.Metadata == null, error);
      }
      else {
        ValidateMetadata(task, result, task.HasFlag(SqlWorkerTask.DropSchema));
      }
    }

    private void ValidateSchema(SqlWorkerTask task, SqlWorkerResult result)
    {
      var error = string.Format(ErrorMessage, task);

      if (!task.HasFlag(SqlWorkerTask.ExtractSchema)) {
        Assert.IsNull(result.Schema, error);
      }
      else {
        Assert.IsNotNull(result.Schema, error);
        Assert.IsNotEmpty(result.Schema.Catalogs, error);
      }
    }

    private void ValidateMetadata(SqlWorkerTask task, SqlWorkerResult result, bool isSchemaDropped)
    {
      var error = string.Format(ErrorMessage, task);
      if (!IsMetdataMemberExtracted(task)) {
        Assert.IsNull(result.Metadata);
      }
      else {
        Assert.That(result.Metadata.Types.Any(), Is.EqualTo(!isSchemaDropped && task.HasFlag(SqlWorkerTask.ExtractMetadataTypes)), error);
        Assert.That(result.Metadata.Assemblies.Any(), Is.EqualTo(!isSchemaDropped && task.HasFlag(SqlWorkerTask.ExtractMetadataAssemblies)), error);
        Assert.That(result.Metadata.Extensions.Any(), Is.EqualTo(!isSchemaDropped && task.HasFlag(SqlWorkerTask.ExtractMetadataExtension)), error);
      }
    }

    private bool IsMetdataMemberExtracted(SqlWorkerTask task)
    {
      return (task & (
        SqlWorkerTask.ExtractMetadataTypes |
        SqlWorkerTask.ExtractMetadataAssemblies |
        SqlWorkerTask.ExtractMetadataExtension)) > 0;
    }

    #region database preapartion
    private void PrepareDatabase()
    {
      var schema = ExtractCurrentSchema();
      PrepareMetadata(schema);
      PrepareUserDefinedTables(schema);
    }

    private Schema ExtractCurrentSchema()
    {
      var connection = accessor.Connection;
      return connection.ConnectionInfo.Provider == WellKnown.Provider.SqlServerCe
        ? accessor.StorageDriver.Extract(connection, new[] { new SqlExtractionTask("", "default") }).Catalogs.First().DefaultSchema
        : accessor.Connection.Driver.ExtractDefaultSchema(connection);
    }

    private void PrepareMetadata(Schema schema)
    {
      var mapping = new MetadataMapping(accessor.StorageDriver, accessor.NameBuilder);

      DropExistingMetadataTables(schema, mapping);
      PrepareAssemblyTable(schema, mapping);
      PrepareExtensionTable(schema, mapping);
      PreapareTypeTable(schema, mapping);
    }

    private void DropExistingMetadataTables(Schema schema, MetadataMapping mapping)
    {
      var assemblyTable = schema.Tables[mapping.Assembly];
      var extensionTable = schema.Tables[mapping.Extension];
      var typeTable = schema.Tables[mapping.Type];

      if (assemblyTable != null) {
        Execute(SqlDdl.Drop(assemblyTable));
        _ = schema.Tables.Remove(assemblyTable);
      }
      if (extensionTable != null) {
        Execute(SqlDdl.Drop(extensionTable));
        _ = schema.Tables.Remove(extensionTable);
      }
      if (typeTable != null) {
        Execute(SqlDdl.Drop(typeTable));
        _ = schema.Tables.Remove(typeTable);
      }
    }

    private void PrepareAssemblyTable(Schema schema, MetadataMapping mapping)
    {
      var columnsTypeMap = GetColumnsTypeMap(typeof(Xtensive.Orm.Metadata.Assembly));

      var table = schema.CreateTable(mapping.Assembly);
      var create = SqlDdl.Create(table);

      _ = table.CreateColumn(mapping.AssemblyName, columnsTypeMap[mapping.AssemblyName]);
      _ = table.CreateColumn(mapping.AssemblyVersion, columnsTypeMap[mapping.AssemblyVersion]);
      Execute(create);

      var tableRef = SqlDml.TableRef(table);
      var insert = SqlDml.Insert(tableRef);
      insert.AddValueRow((tableRef[mapping.AssemblyName], "name"), (tableRef[mapping.AssemblyVersion], "version"));
      Execute(insert);
    }

    private void PreapareTypeTable(Schema schema, MetadataMapping mapping)
    {
      var columnsTypeMap = GetColumnsTypeMap(typeof(Xtensive.Orm.Metadata.Type));

      var table = schema.CreateTable(mapping.Type);
      var create = SqlDdl.Create(table);
      _ = table.CreateColumn(mapping.TypeId, columnsTypeMap[mapping.TypeId]);
      _ = table.CreateColumn(mapping.TypeName, columnsTypeMap[mapping.TypeName]);
      Execute(create);

      var tableRef = SqlDml.TableRef(table);
      var insert = SqlDml.Insert(tableRef);
      insert.AddValueRow((tableRef[mapping.TypeId], 1),(tableRef[mapping.TypeName], "name"));
      Execute(insert);
    }

    private void PrepareExtensionTable(Schema schema, MetadataMapping mapping)
    {
      var columnsTypeMap = GetColumnsTypeMap(typeof(Xtensive.Orm.Metadata.Extension));

      var table = schema.CreateTable(mapping.Extension);
      var create = SqlDdl.Create(table);
      _ = table.CreateColumn(mapping.ExtensionName, columnsTypeMap[mapping.ExtensionName]);
      _ = table.CreateColumn(mapping.ExtensionText, columnsTypeMap[mapping.ExtensionText]);
      Execute(create);

      var tableRef = SqlDml.TableRef(table);
      var insert = SqlDml.Insert(tableRef);
      insert.AddValueRow((tableRef[mapping.ExtensionName], "name"), (tableRef[mapping.ExtensionText], "text"));
      Execute(insert);
    }

    private void PrepareUserDefinedTables(Schema schema)
    {
      if (schema.Tables[TestEntityName] != null) {
        return;
      }
      var table = schema.CreateTable(TestEntityName);
      var create = SqlDdl.Create(table);
      _ = table.CreateColumn(TestEntityField, new SqlValueType(SqlType.VarCharMax));
      Execute(create);
    }

    private void Execute(ISqlCompileUnit statement)
    {
      using var command = accessor.Connection.CreateCommand(statement);
      _ = command.ExecuteNonQuery();
    }

    private IDictionary<string, SqlValueType> GetColumnsTypeMap(Type metadataTableType)
    {
      var map = new Dictionary<string, SqlValueType>();
      var storageDriver = accessor.StorageDriver;

      var fields = metadataTableType.GetProperties()
        .Select(p => new {PropertyInfo = p, FieldAttribute = p.GetAttribute<FieldAttribute>()})
        .Where(i => i.FieldAttribute != null);

      foreach (var field in fields) {
        var property = field.PropertyInfo;
        var attribute = field.FieldAttribute;
        var sqlType = storageDriver.MapValueType(property.PropertyType, attribute.length, attribute.precision, attribute.scale);
        map.Add(property.Name, sqlType);
      }
      return map;
    }

    private void CreateAccessor()
    {
      var nodeConfiguration = new NodeConfiguration();
      var configuration = DomainConfigurationFactory.Create();
      var descriptor = ProviderDescriptor.Get(configuration.ConnectionInfo.Provider);
      var driverFactory = (SqlDriverFactory) Activator.CreateInstance(descriptor.DriverFactory);
      var storageDriver = StorageDriver.Create(driverFactory, configuration);
      var nameBuilder = new NameBuilder(configuration, storageDriver.ProviderInfo);
      var handlerFactory = (HandlerFactory) Activator.CreateInstance(descriptor.HandlerFactory);

      var accessor = new UpgradeServiceAccessor {
        Configuration = configuration,
        StorageDriver = storageDriver,
        NameBuilder = nameBuilder,
        HandlerFactory = handlerFactory
      };
      configuration.Lock();
      var connection = storageDriver.CreateConnection(null);
      connection.Open();
      accessor.MappingResolver = MappingResolver.Create(configuration, nodeConfiguration, storageDriver.GetDefaultSchema(connection));
      if (storageDriver.ProviderInfo.Supports(ProviderFeatures.SingleConnection)) {
        accessor.RegisterTemporaryResource(connection);
      }
      else {
        accessor.RegisterResource(connection);
      }
      accessor.Connection = connection;

      this.accessor = accessor;
    }
    #endregion
  }
}

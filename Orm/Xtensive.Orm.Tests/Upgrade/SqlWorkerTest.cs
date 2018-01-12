// Copyright (C) 2003-2016 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Julian Mamokin
// Created:    2017.03.01

using System;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using Xtensive.Core;
using Xtensive.Orm.Building.Builders;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Providers;
using Xtensive.Orm.Upgrade;
using Xtensive.Sql;
using Xtensive.Sql.Model;

namespace Xtensive.Orm.Tests.Upgrade
{
  public class SqlWorkerTest
  {
    private const string TestEntityName = "SqlWorkerTestEntity";
    private const string TestEntityField = "SomeStringField";
    private const string ErrorMessage = "Failed on task: {0}";

    private UpgradeServiceAccessor accessor;

    [TestFixtureSetUp]
    public void TestFixtureSetup()
    {
      CreateAccessor();
    }

    [SetUp]
    public void Setup()
    {
      PrepareDatabase();
    }

    [Test]
    public void Test01()
    {
      ValidateWorkerResult(SqlWorkerTask.ExtractMetadataTypes);
    }

    [Test]
    public void Test02()
    {
      ValidateWorkerResult(SqlWorkerTask.ExtractMetadataExtension);
    }

    [Test]
    public void Test03()
    {
      ValidateWorkerResult(SqlWorkerTask.ExtractMetadataAssemblies);
    }

    [Test]
    public void Test04()
    {
      ValidateWorkerResult(SqlWorkerTask.ExtractMetadata);
    }

    [Test]
    public void Test05()
    {
      ValidateWorkerResult(SqlWorkerTask.DropSchema);
    }

    [Test]
    public void Test06()
    {
      ValidateWorkerResult(SqlWorkerTask.ExtractSchema);
    }

    [Test]
    public void Test07()
    {
      ValidateWorkerResult(SqlWorkerTask.ExtractMetadataTypes | SqlWorkerTask.ExtractMetadataExtension);
    }

    [Test]
    public void Test08()
    {
      ValidateWorkerResult(SqlWorkerTask.ExtractMetadataTypes | SqlWorkerTask.ExtractMetadataAssemblies);
    }

    [Test]
    public void Test09()
    {
      ValidateWorkerResult(SqlWorkerTask.ExtractMetadataAssemblies | SqlWorkerTask.ExtractMetadataExtension);
    }

    [Test]
    public void Test10()
    {
      ValidateWorkerResult(SqlWorkerTask.DropSchema | SqlWorkerTask.ExtractMetadataTypes);
    }

    [Test]
    public void Test11()
    {
      ValidateWorkerResult(SqlWorkerTask.DropSchema | SqlWorkerTask.ExtractMetadataExtension);
    }

    [Test]
    public void Test12()
    {
      ValidateWorkerResult(SqlWorkerTask.DropSchema | SqlWorkerTask.ExtractMetadataAssemblies);
    }

    [Test]
    public void Test13()
    {
      ValidateWorkerResult(SqlWorkerTask.DropSchema | SqlWorkerTask.ExtractMetadata);
    }

    [Test]
    public void Test14()
    {
      ValidateWorkerResult(SqlWorkerTask.DropSchema | SqlWorkerTask.ExtractSchema);
    }

    [Test]
    public void Test15()
    {
      ValidateWorkerResult(SqlWorkerTask.ExtractSchema | SqlWorkerTask.ExtractMetadataTypes);
    }

    [Test]
    public void Test16()
    {
      ValidateWorkerResult(SqlWorkerTask.ExtractSchema | SqlWorkerTask.ExtractMetadataExtension);
    }

    [Test]
    public void Test17()
    {
      ValidateWorkerResult(SqlWorkerTask.ExtractSchema | SqlWorkerTask.ExtractMetadataAssemblies);
    }

    [Test]
    public void Test18()
    {
      ValidateWorkerResult(SqlWorkerTask.ExtractSchema | SqlWorkerTask.ExtractMetadata);
    }

    [Test]
    public void Test19()
    {
      ValidateWorkerResult(
        SqlWorkerTask.ExtractSchema
        | SqlWorkerTask.ExtractMetadataTypes
        | SqlWorkerTask.ExtractMetadataExtension);
    }

    [Test]
    public void Test20()
    {
      ValidateWorkerResult(
        SqlWorkerTask.ExtractSchema 
        | SqlWorkerTask.ExtractMetadataTypes 
        | SqlWorkerTask.ExtractMetadataAssemblies);
    }

    [Test]
    public void Test21()
    {
      ValidateWorkerResult(
        SqlWorkerTask.ExtractSchema
        | SqlWorkerTask.ExtractMetadataAssemblies
        | SqlWorkerTask.ExtractMetadataExtension);
    }

    [Test]
    public void Test22()
    {
      ValidateWorkerResult(
        SqlWorkerTask.DropSchema 
        | SqlWorkerTask.ExtractSchema 
        | SqlWorkerTask.ExtractMetadataTypes);
    }

    [Test]
    public void Test23()
    {
      ValidateWorkerResult(
        SqlWorkerTask.DropSchema
        | SqlWorkerTask.ExtractSchema
        | SqlWorkerTask.ExtractMetadataExtension);
    }

    [Test]
    public void Test24()
    {
      ValidateWorkerResult(
        SqlWorkerTask.DropSchema
        | SqlWorkerTask.ExtractSchema
        | SqlWorkerTask.ExtractMetadataAssemblies);
    }

    [Test]
    public void Test25()
    {
      ValidateWorkerResult(
        SqlWorkerTask.DropSchema
        | SqlWorkerTask.ExtractSchema
        | SqlWorkerTask.ExtractMetadata);
    }

    [Test]
    public void Test26()
    {
      ValidateWorkerResult(
        SqlWorkerTask.DropSchema
        | SqlWorkerTask.ExtractMetadataTypes
        | SqlWorkerTask.ExtractMetadataExtension);
    }

    [Test]
    public void Test27()
    {
      ValidateWorkerResult(
        SqlWorkerTask.DropSchema
        | SqlWorkerTask.ExtractMetadataTypes
        | SqlWorkerTask.ExtractMetadataAssemblies);
    }

    [Test]
    public void Test28()
    {
      ValidateWorkerResult(
        SqlWorkerTask.DropSchema
        | SqlWorkerTask.ExtractMetadataAssemblies
        | SqlWorkerTask.ExtractMetadataExtension);
    }

    [Test]
    public void Test29()
    {
      ValidateWorkerResult(
        SqlWorkerTask.DropSchema
        | SqlWorkerTask.ExtractSchema
        | SqlWorkerTask.ExtractMetadataTypes
        | SqlWorkerTask.ExtractMetadataExtension);
    }

    [Test]
    public void Test30()
    {
      ValidateWorkerResult(
        SqlWorkerTask.DropSchema
        | SqlWorkerTask.ExtractSchema 
        | SqlWorkerTask.ExtractMetadataTypes 
        | SqlWorkerTask.ExtractMetadataAssemblies);
    }

    [Test]
    public void Test31()
    {
      ValidateWorkerResult(
        SqlWorkerTask.DropSchema
        | SqlWorkerTask.ExtractSchema
        | SqlWorkerTask.ExtractMetadataAssemblies
        | SqlWorkerTask.ExtractMetadataExtension);
    }

    private void ValidateWorkerResult(SqlWorkerTask task)
    {
      var result = SqlWorker.Create(accessor, task).Invoke();
      var error = string.Format(ErrorMessage, task);

      ValidateSchema(task, result);
      if (task==SqlWorkerTask.DropSchema) {
        Assert.IsTrue(result.Schema==null, error);
        Assert.IsTrue(result.Metadata==null, error);
        return;
      }
      ValidateMetadata(task, result, task.HasFlag(SqlWorkerTask.DropSchema));
    }

    private void ValidateSchema(SqlWorkerTask task, SqlWorkerResult result)
    {
      var error = string.Format(ErrorMessage, task);

      if (!task.HasFlag(SqlWorkerTask.ExtractSchema))
        Assert.IsNull(result.Schema, error);
      else {
        Assert.IsNotNull(result.Schema, error);
        Assert.IsNotEmpty(result.Schema.Catalogs, error);
      }
    }

    private void ValidateMetadata(SqlWorkerTask task, SqlWorkerResult result, bool isSchemaDropped)
    {
      var error = string.Format(ErrorMessage, task);
      if (!IsMetdataMemberExtracted(task))
        Assert.IsNull(result.Metadata);
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
      return connection.ConnectionInfo.Provider==WellKnown.Provider.SqlServerCe ?
        accessor.StorageDriver.Extract(connection, new[] {new SqlExtractionTask("", "default")}).Catalogs.First().DefaultSchema :
        accessor.Connection.Driver.ExtractDefaultSchema(connection);
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

      if (assemblyTable!=null) {
        Execute(SqlDdl.Drop(assemblyTable));
        schema.Tables.Remove(assemblyTable);
      }
      if (extensionTable!=null) {
        Execute(SqlDdl.Drop(extensionTable));
        schema.Tables.Remove(extensionTable);
      }
      if (typeTable!=null) {
        Execute(SqlDdl.Drop(typeTable));
        schema.Tables.Remove(typeTable);
      }
    }

    private void PrepareAssemblyTable(Schema schema, MetadataMapping mapping)
    {
      var table = schema.CreateTable(mapping.Assembly);
      var create = SqlDdl.Create(table);

      table.CreateColumn(mapping.AssemblyName, new SqlValueType(SqlType.VarCharMax));
      table.CreateColumn(mapping.AssemblyVersion, new SqlValueType(SqlType.VarCharMax));
      Execute(create);

      var tableRef = SqlDml.TableRef(table);
      var insert = SqlDml.Insert(tableRef);
      insert.Values.Add(tableRef[mapping.AssemblyName], "name");
      insert.Values.Add(tableRef[mapping.AssemblyVersion], "version");
      Execute(insert);
    }

    private void PreapareTypeTable(Schema schema, MetadataMapping mapping)
    {
      var table = schema.CreateTable(mapping.Type);
      var create = SqlDdl.Create(table);
      table.CreateColumn(mapping.TypeId, new SqlValueType(SqlType.Int32));
      table.CreateColumn(mapping.TypeName, new SqlValueType(SqlType.VarCharMax));
      Execute(create);

      var tableRef = SqlDml.TableRef(table);
      var insert = SqlDml.Insert(tableRef);
      insert.Values.Add(tableRef[mapping.TypeId], 1);
      insert.Values.Add(tableRef[mapping.TypeName], "name");
      Execute(insert);
    }

    private void PrepareExtensionTable(Schema schema, MetadataMapping mapping)
    {
      var table = schema.CreateTable(mapping.Extension);
      var create = SqlDdl.Create(table);
      table.CreateColumn(mapping.ExtensionName, new SqlValueType(SqlType.VarCharMax));
      table.CreateColumn(mapping.ExtensionText, new SqlValueType(SqlType.VarCharMax));
      Execute(create);

      var tableRef = SqlDml.TableRef(table);
      var insert = SqlDml.Insert(tableRef);
      insert.Values.Add(tableRef[mapping.ExtensionName], "name");
      insert.Values.Add(tableRef[mapping.ExtensionText], "text");
      Execute(insert);
    }

    private void PrepareUserDefinedTables(Schema schema)
    {
      if (schema.Tables[TestEntityName]!=null)
        return;
      var table = schema.CreateTable(TestEntityName);
      var create = SqlDdl.Create(table);
      table.CreateColumn(TestEntityField, new SqlValueType(SqlType.VarCharMax));
      Execute(create);
    }

    private void Execute(ISqlCompileUnit statement)
    {
      using (var command = accessor.Connection.CreateCommand(statement))
        command.ExecuteNonQuery();
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
      if (storageDriver.ProviderInfo.Supports(ProviderFeatures.SingleConnection))
        accessor.RegisterTemporaryResource(connection);
      else
        accessor.RegisterResource(connection);
      accessor.Connection = connection;
      accessor.Connection.Extensions.AddOne(new TypeDelegator(typeof (IProviderExecutor)));

      this.accessor = accessor;
    }
    #endregion
  }
}

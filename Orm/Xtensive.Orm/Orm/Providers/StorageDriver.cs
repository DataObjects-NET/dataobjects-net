// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.08.14

using System;
using System.Collections.Generic;
using System.Linq;
using Xtensive.Core;
using Xtensive.Diagnostics;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Model;
using Xtensive.Orm.Providers;
using Xtensive.Sql;
using Xtensive.Sql.Compiler;
using Xtensive.Sql.Info;
using Xtensive.Threading;
using Xtensive.Tuples;

namespace Xtensive.Orm.Providers
{
  /// <summary>
  /// SQL storage driver.
  /// </summary>
  public sealed partial class StorageDriver
  {
    private readonly DomainConfiguration configuration;
    private readonly SqlDriver underlyingDriver;
    private readonly SqlTranslator translator;
    private readonly TypeMappingCollection allMappings;
    private readonly bool isLoggingEnabled;
    private readonly bool hasSavepoints;

    private ThreadSafeDictionary<TupleDescriptor, DbDataReaderAccessor> accessorCache;

    public ProviderInfo ProviderInfo { get; private set; }

    public StorageExceptionBuilder ExceptionBuilder { get; private set; }

    public string BuildBatch(string[] statements)
    {
      return translator.BuildBatch(statements);
    }

    public string BuildParameterReference(string parameterName)
    {
      return translator.ParameterPrefix + parameterName;
    }

    public SqlExtractionResult Extract(SqlConnection connection, IEnumerable<SqlExtractionTask> tasks)
    {
      var result = underlyingDriver.Extract(connection, tasks);
      FixExtractionResult(result);
      return result;
    }

    public SqlCompilationResult Compile(ISqlCompileUnit statement)
    {
      var options = new SqlCompilerConfiguration {
        DatabaseQualifiedObjects = configuration.IsMultidatabase
      };
      return underlyingDriver.Compile(statement, options);
    }

    public DbDataReaderAccessor GetDataReaderAccessor(TupleDescriptor descriptor)
    {
      return accessorCache.GetValue(descriptor, CreateDataReaderAccessor);
    }

    public StorageDriver CreateNew(Domain domain)
    {
      ArgumentValidator.EnsureArgumentNotNull(domain, "domain");
      return new StorageDriver(underlyingDriver, ProviderInfo, domain.Configuration, GetModelProvider(domain));
    }

    private DbDataReaderAccessor CreateDataReaderAccessor(TupleDescriptor descriptor)
    {
      return new DbDataReaderAccessor(descriptor, descriptor.Select(GetTypeMapping));
    }

    private static DomainModel GetNullModel()
    {
      return null;
    }

    private static Func<DomainModel> GetModelProvider(Domain domain)
    {
      return () => domain.Model;
    }

    private void FixExtractionResult(SqlExtractionResult result)
    {
      var isSqlServerFamily = ProviderInfo.ProviderName.In(
        WellKnown.Provider.SqlServer, WellKnown.Provider.SqlServerCe);

      // This code works for SQL Server and SQL Server CE
      if (!isSqlServerFamily)
        return;

      foreach (var schema in result.Catalogs.SelectMany(c => c.Schemas)) {
        var tables = schema.Tables;
        var sysdiagrams = tables["sysdiagrams"];
        if (sysdiagrams!=null)
          tables.Remove(sysdiagrams);
      }
    }

    // Constructors

    public static StorageDriver Create(SqlDriverFactory driverFactory, DomainConfiguration configuration)
    {
      ArgumentValidator.EnsureArgumentNotNull(driverFactory, "driverFactory");
      ArgumentValidator.EnsureArgumentNotNull(configuration, "configuration");

      var driver = driverFactory.GetDriver(configuration.ConnectionInfo, configuration.ForcedServerVersion);
      var providerInfo = ProviderInfoBuilder.Build(configuration.ConnectionInfo.Provider, driver);

      return new StorageDriver(driver, providerInfo, configuration, GetNullModel);
    }

    private StorageDriver(
      SqlDriver driver, ProviderInfo providerInfo, DomainConfiguration configuration, Func<DomainModel> modelProvider)
    {
      underlyingDriver = driver;
      ProviderInfo = providerInfo;
      this.configuration = configuration;
      ExceptionBuilder = new StorageExceptionBuilder(driver, configuration, modelProvider);
      accessorCache = ThreadSafeDictionary<TupleDescriptor, DbDataReaderAccessor>.Create(new object());
      allMappings = underlyingDriver.TypeMappings;
      translator = underlyingDriver.Translator;
      hasSavepoints = underlyingDriver.ServerInfo.ServerFeatures.Supports(ServerFeatures.Savepoints);
      isLoggingEnabled = Log.IsLogged(LogEventTypes.Info); // Just to cache this value
    }
  }
}
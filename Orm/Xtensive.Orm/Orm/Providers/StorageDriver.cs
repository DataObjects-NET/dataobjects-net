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
using Xtensive.Orm.Providers.Sql;
using Xtensive.Sql;
using Xtensive.Sql.Compiler;
using Xtensive.Sql.Info;
using Xtensive.Sql.Model;
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
      return underlyingDriver.Extract(connection, tasks);
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
      return new StorageDriver(underlyingDriver, domain.Configuration, GetModelProvider(domain));
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

    // Constructors

    public static StorageDriver Create(SqlDriverFactory driverFactory, Domain domain)
    {
      ArgumentValidator.EnsureArgumentNotNull(driverFactory, "driverFactory");
      ArgumentValidator.EnsureArgumentNotNull(domain, "domain");

      var configuration = domain.Configuration;
      var driver = driverFactory.GetDriver(configuration.ConnectionInfo);

      return new StorageDriver(driver, configuration, GetModelProvider(domain));
    }

    public static StorageDriver Create(SqlDriverFactory driverFactory, DomainConfiguration configuration)
    {
      ArgumentValidator.EnsureArgumentNotNull(driverFactory, "driverFactory");
      ArgumentValidator.EnsureArgumentNotNull(configuration, "configuration");

      var driver = driverFactory.GetDriver(configuration.ConnectionInfo);

      return new StorageDriver(driver, configuration, GetNullModel);
    }

    private StorageDriver(SqlDriver driver, DomainConfiguration configuration, Func<DomainModel> modelProvider)
    {
      this.configuration = configuration;
      underlyingDriver = driver;

      ProviderInfo = ProviderInfoBuilder.Build(configuration.ConnectionInfo.Provider, underlyingDriver);
      ExceptionBuilder = new StorageExceptionBuilder(underlyingDriver, configuration, modelProvider);

      accessorCache = ThreadSafeDictionary<TupleDescriptor, DbDataReaderAccessor>.Create(new object());

      allMappings = underlyingDriver.TypeMappings;
      translator = underlyingDriver.Translator;

      hasSavepoints = underlyingDriver.ServerInfo.ServerFeatures.Supports(ServerFeatures.Savepoints);
      isLoggingEnabled = Sql.Log.IsLogged(LogEventTypes.Info); // Just to cache this value
    }
  }
}
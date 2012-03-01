// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.08.14

using System;
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

    public Schema ExtractSchema(SqlConnection connection)
    {
      string schemaName = configuration.DefaultSchema.IsNullOrEmpty()
        ? underlyingDriver.CoreServerInfo.DefaultSchemaName
        : configuration.DefaultSchema;
      var schema = underlyingDriver.ExtractSchema(connection, schemaName);
      return schema;
    }

    public SqlCompilationResult Compile(ISqlCompileUnit statement)
    {
      return underlyingDriver.Compile(statement);
    }

    public DbDataReaderAccessor GetDataReaderAccessor(TupleDescriptor descriptor)
    {
      return accessorCache.GetValue(descriptor, CreateDataReaderAccessor);
    }

    private DbDataReaderAccessor CreateDataReaderAccessor(TupleDescriptor descriptor)
    {
      return new DbDataReaderAccessor(descriptor, descriptor.Select(GetTypeMapping));
    }

    private static DomainModel GetNullModel()
    {
      return null;
    }

    private static Func<DomainModel> GetDomainModelProvider(Domain domain)
    {
      return () => domain.Model;
    }

    // Constructors

    internal StorageDriver(SqlDriverFactory driverFactory, Domain domain)
      : this(driverFactory, domain.Configuration, GetDomainModelProvider(domain))
    {
    }

    internal StorageDriver(SqlDriverFactory driverFactory, DomainConfiguration configuration)
      : this(driverFactory, configuration, GetNullModel)
    {
    }

    private StorageDriver(SqlDriverFactory driverFactory, DomainConfiguration configuration, Func<DomainModel> modelProvider)
    {
      ArgumentValidator.EnsureArgumentNotNull(driverFactory, "driverFactory");
      ArgumentValidator.EnsureArgumentNotNull(configuration, "configuration");
      ArgumentValidator.EnsureArgumentNotNull(modelProvider, "modelProvider");

      this.configuration = configuration;
      underlyingDriver = driverFactory.GetDriver(configuration.ConnectionInfo);

      ProviderInfo = ProviderInfoBuilder.Build(underlyingDriver);
      ExceptionBuilder = new StorageExceptionBuilder(underlyingDriver, configuration, modelProvider);

      accessorCache = ThreadSafeDictionary<TupleDescriptor, DbDataReaderAccessor>.Create(new object());

      allMappings = underlyingDriver.TypeMappings;
      translator = underlyingDriver.Translator;

      hasSavepoints = underlyingDriver.ServerInfo.ServerFeatures.Supports(ServerFeatures.Savepoints);
      isLoggingEnabled = Sql.Log.IsLogged(LogEventTypes.Info); // Just to cache this value
    }
  }
}
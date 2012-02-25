// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.08.14

using System.Linq;
using Xtensive.Core;
using Xtensive.Diagnostics;
using Xtensive.Sql;
using Xtensive.Sql.Compiler;
using Xtensive.Sql.Info;
using Xtensive.Sql.Model;
using Xtensive.Threading;
using Xtensive.Tuples;

namespace Xtensive.Orm.Providers.Sql
{
  /// <summary>
  /// SQL storage driver.
  /// </summary>
  public sealed partial class StorageDriver
  {
    private readonly Domain domain;
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
      string schemaName = domain.Configuration.DefaultSchema.IsNullOrEmpty()
        ? underlyingDriver.CoreServerInfo.DefaultSchemaName
        : domain.Configuration.DefaultSchema;
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

    // Constructors

    public StorageDriver(Domain domain, SqlDriver underlyingDriver)
    {
      this.domain = domain;
      this.underlyingDriver = underlyingDriver;

      ProviderInfo = ProviderInfoBuilder.Build(underlyingDriver);
      ExceptionBuilder = new StorageExceptionBuilder(domain, underlyingDriver);

      accessorCache = ThreadSafeDictionary<TupleDescriptor, DbDataReaderAccessor>.Create(new object());

      allMappings = underlyingDriver.TypeMappings;
      translator = underlyingDriver.Translator;

      hasSavepoints = underlyingDriver.ServerInfo.ServerFeatures.Supports(ServerFeatures.Savepoints);
      isLoggingEnabled = Log.IsLogged(LogEventTypes.Info); // Just to cache this value
    }
  }
}
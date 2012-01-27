// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.08.14

using Xtensive.Core;
using Xtensive.Diagnostics;
using Xtensive.Sql;
using Xtensive.Sql.Compiler;
using Xtensive.Sql.Model;

namespace Xtensive.Orm.Providers.Sql
{
  /// <summary>
  /// SQL provider driver.
  /// </summary>
  public sealed partial class Driver
  {
    private readonly Orm.Domain domain;
    private readonly SqlDriver underlyingDriver;
    private readonly SqlTranslator translator;
    private readonly TypeMappingCollection allMappings;

    private readonly bool includeSqlInExceptions;
    private readonly bool isLoggingEnabled;

    public string BatchBegin { get { return translator.BatchBegin; } }
    public string BatchEnd { get { return translator.BatchEnd; } }

    public ProviderInfo BuildProviderInfo()
    {
      // We extracted this method to a separate class for tests.
      // It's very desirable to have a way of getting ProviderInfo without building a domain.
      return ProviderInfoBuilder.Build(underlyingDriver);
    }

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


    // Constructors

    public Driver(Orm.Domain domain, SqlDriver underlyingDriver)
    {
      this.domain = domain;
      this.underlyingDriver = underlyingDriver;

      allMappings = underlyingDriver.TypeMappings;
      translator = underlyingDriver.Translator;

      isLoggingEnabled = Log.IsLogged(LogEventTypes.Info); // Just to cache this value
      includeSqlInExceptions = domain.Configuration.IncludeSqlInExceptions;
    }
  }
}
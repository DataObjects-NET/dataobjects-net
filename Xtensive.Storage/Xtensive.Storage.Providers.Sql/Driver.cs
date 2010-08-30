// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.08.14

using System.Linq;
using Xtensive.Core;
using Xtensive.Core.Diagnostics;
using Xtensive.Sql;
using Xtensive.Sql.Compiler;
using Xtensive.Sql.Info;
using Xtensive.Sql.Model;
using Xtensive.Sql.ValueTypeMapping;
using System;

namespace Xtensive.Storage.Providers.Sql
{
  public sealed partial class Driver
  {
    private readonly Domain domain;
    private readonly SqlDriver underlyingDriver;
    private readonly SqlTranslator translator;
    private readonly TypeMappingCollection allMappings;
    
    private readonly bool isDebugLoggingEnabled;

    public string BatchBegin { get { return translator.BatchBegin; } }
    public string BatchEnd { get { return translator.BatchEnd; } }

    public Location StorageLocation { get; private set; }

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

      if (underlyingDriver.CoreServerInfo.ServerLocation.Provider.ToLower().StartsWith("sqlserver")) {
        // This code works for Microsoft SQL Server and Microsoft SQL Server CE
        var tables = schema.Tables;
        var sysdiagrams = tables["sysdiagrams"];
        if (sysdiagrams!=null)
          tables.Remove(sysdiagrams);
      }

      return schema;
    }

    public SqlCompilationResult Compile(ISqlCompileUnit statement)
    {
      return underlyingDriver.Compile(statement);
    }


    // Constructors

    public Driver(Domain domain)
    {
      this.domain = domain;

      underlyingDriver = SqlDriver.Create(domain.Configuration.ConnectionInfo);
      allMappings = underlyingDriver.TypeMappings;
      translator = underlyingDriver.Translator;

      StorageLocation = underlyingDriver.CoreServerInfo.ServerLocation;

      isDebugLoggingEnabled = Log.IsLogged(LogEventTypes.Debug); // Just to cache this value
    }
  }
}
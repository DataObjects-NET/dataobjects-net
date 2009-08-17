// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.08.14

using System;
using System.Data.Common;
using Xtensive.Core;
using Xtensive.Sql;
using Xtensive.Sql.Compiler;
using Xtensive.Sql.Info;
using Xtensive.Sql.Model;
using Xtensive.Sql.ValueTypeMapping;

namespace Xtensive.Storage.Providers.Sql
{
  public sealed partial class Driver
  {
    private readonly SqlDriver underlyingDriver;
    private readonly SqlTranslator translator;
    private readonly TypeMappingCollection allMappings;

    public string BatchBegin { get { return underlyingDriver.Translator.BatchBegin; } }
    public string BatchEnd { get { return underlyingDriver.Translator.BatchEnd; } }
    public string BatchItemDelimiter { get { return underlyingDriver.Translator.BatchItemDelimiter; } }

    public ProviderInfo BuildProviderInfo()
    {
      var result = new ProviderInfo();
      var serverInfo = underlyingDriver.ServerInfo;
      
      var queryFeatures = serverInfo.Query.Features;
      var indexFeatures = serverInfo.Index.Features;
      var foreignKeyFeatures = serverInfo.ForeignKey.Features;
      // TODO: add corresponding feature to Sql.Info and read it here
      result.SupportsEnlist = false;
      result.SupportsBatches = queryFeatures.Supports(QueryFeatures.Batches);
      result.SupportsClusteredIndexes = indexFeatures.Supports(IndexFeatures.Clustered);
      result.SupportsCollations = serverInfo.Collation!=null;
      result.SupportsForeignKeyConstraints = serverInfo.ForeignKey!=null;
      if (serverInfo.ForeignKey!=null)
        result.SupportsDeferredForeignKeyConstraints = foreignKeyFeatures.Supports(ForeignKeyConstraintFeatures.Deferrable);
      result.SupportsIncludedColumns = indexFeatures.Supports(IndexFeatures.NonKeyColumns);
      result.SupportsKeyColumnSortOrder = indexFeatures.Supports(IndexFeatures.SortOrder);
      result.SupportsSequences = serverInfo.Sequence!=null;
      result.SupportsAutoincrementColumns = serverInfo.Identity!=null;
      result.SupportsApplyProvider = queryFeatures.Supports(QueryFeatures.CrossApply);
      result.SupportsPaging = queryFeatures.Supports(QueryFeatures.Paging);
      result.SupportsLargeObjects = queryFeatures.Supports(QueryFeatures.LargeObjects);
      result.SupportsAllBooleanExpressions = queryFeatures.Supports(QueryFeatures.FullBooleanExpressionSupport);
      result.NamedParameters = queryFeatures.Supports(QueryFeatures.NamedParameters);
      result.ParameterPrefix = serverInfo.Query.ParameterPrefix;

      var dataTypes = serverInfo.DataTypes;
      var binaryTypeInfo = dataTypes.VarBinary ?? dataTypes.VarBinaryMax;
      result.EmptyBlobIsNull = binaryTypeInfo!=null
        ? binaryTypeInfo.Features.Supports(DataTypeFeatures.ZeroLengthValueIsNull)
        : false;
      var stringTypeInfo = dataTypes.VarChar ?? dataTypes.VarCharMax;
      result.EmptyStringIsNull = stringTypeInfo!=null
        ? stringTypeInfo.Features.Supports(DataTypeFeatures.ZeroLengthValueIsNull)
        : false;

      result.Version = (Version) serverInfo.Version.ProductVersion.Clone();
      return result;
    }

    public string BuildBatch(string[] statements)
    {
      return translator.BuildBatch(statements);
    }

    public Schema ExtractSchema(SqlConnection connection, DbTransaction transaction)
    {
      return underlyingDriver.ExtractDefaultSchema(connection, transaction);
    }

    public SqlCompilationResult Compile(ISqlCompileUnit statement)
    {
      return underlyingDriver.Compile(statement);
    }

    public SqlCompilationResult Compile(ISqlCompileUnit statement, SqlCompilerOptions options)
    {
      return underlyingDriver.Compile(statement, options);
    }
    
    // Constructors

    public Driver(UrlInfo url)
    {
      underlyingDriver = SqlDriver.Create(url);
      allMappings = underlyingDriver.TypeMappings;
      translator = underlyingDriver.Translator;
    }
  }
}
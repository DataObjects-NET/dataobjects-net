// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.08.14

using System;
using System.Data.Common;
using System.Linq;
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

    public string BatchBegin { get { return translator.BatchBegin; } }
    public string BatchEnd { get { return translator.BatchEnd; } }
    public string BatchItemDelimiter { get { return translator.BatchItemDelimiter; } }

    public ProviderInfo BuildProviderInfo()
    {
      var si = underlyingDriver.ServerInfo;
      var queryFeatures = si.Query.Features;
      var indexFeatures = si.Index.Features;
      var foreignKeyFeatures = si.ForeignKey.Features;

      var f = ProviderFeatures.None;
      if (queryFeatures.Supports(QueryFeatures.Batches))
        f |= ProviderFeatures.Batches;
      if (indexFeatures.Supports(IndexFeatures.Clustered))
        f |= ProviderFeatures.ClusteredIndexes;
      if (si.Collation!=null)
        f |= ProviderFeatures.Collations;
      if (si.ForeignKey!=null) {
        f |= ProviderFeatures.ForeignKeyConstraints;
        if (foreignKeyFeatures.Supports(ForeignKeyConstraintFeatures.Deferrable))
          f |= ProviderFeatures.DeferrableConstraints;
      }
      if (indexFeatures.Supports(IndexFeatures.NonKeyColumns))
        f |= ProviderFeatures.IncludedColumns;
      if (indexFeatures.Supports(IndexFeatures.SortOrder))
        f |= ProviderFeatures.KeyColumnSortOrder;
      if (si.Sequence!=null)
        f |= ProviderFeatures.Sequences;
      if (queryFeatures.Supports(QueryFeatures.CrossApply))
        f |= ProviderFeatures.CrossApply;
      if (queryFeatures.Supports(QueryFeatures.Paging))
        f |= ProviderFeatures.Paging;
      if (queryFeatures.Supports(QueryFeatures.LargeObjects))
        f |= ProviderFeatures.LargeObjects;
      if (queryFeatures.Supports(QueryFeatures.FullBooleanExpressionSupport))
        f |= ProviderFeatures.FullBooleanExpressionSupport;
      if (queryFeatures.Supports(QueryFeatures.NamedParameters))
        f |= ProviderFeatures.NamedParameters;

      var dataTypes = si.DataTypes;
      var binaryTypeInfo = dataTypes.VarBinary ?? dataTypes.VarBinaryMax;
      if (binaryTypeInfo!=null && binaryTypeInfo.Features.Supports(DataTypeFeatures.ZeroLengthValueIsNull))
        f |= ProviderFeatures.TreatEmptyBlobAsNull;
      var stringTypeInfo = dataTypes.VarChar ?? dataTypes.VarCharMax;
      if (stringTypeInfo!=null && stringTypeInfo.Features.Supports(DataTypeFeatures.ZeroLengthValueIsNull))
        f |= ProviderFeatures.TreatEmptyStringAsNull;

      var storageVersion = (Version) si.Version.ProductVersion.Clone();
      var maxIdentifierLength = new EntityInfo[] {si.Column, si.ForeignKey, si.Index, si.PrimaryKey, si.Sequence, si.Table, si.TemporaryTable, si.UniqueConstraint}.Select(e => e == null ? int.MaxValue : e.MaxIdentifierLength).Min();
      return new ProviderInfo(storageVersion, f, maxIdentifierLength);
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
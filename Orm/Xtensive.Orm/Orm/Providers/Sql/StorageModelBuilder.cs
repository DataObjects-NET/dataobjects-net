// Copyright (C) 2011 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2011.10.10

using System;
using System.Linq;
using Xtensive.Core;
using Xtensive.Orm.Providers.Sql.Expressions;
using Xtensive.Sql;
using Xtensive.Sql.Dml;
using Xtensive.Sql.Model;
using Xtensive.Orm.Upgrade.Model;
using IndexInfo = Xtensive.Orm.Model.IndexInfo;
using PartialIndexFilterInfo = Xtensive.Orm.Upgrade.Model.PartialIndexFilterInfo;

namespace Xtensive.Orm.Providers.Sql
{
  /// <summary>
  /// <see cref="StorageModelBuilder"/> for SQL storages.
  /// </summary>
  public class StorageModelBuilder : Orm.Upgrade.StorageModelBuilder
  {
    private readonly Providers.DomainHandler domainHandler;
    private readonly ProviderInfo providerInfo;
    private readonly StorageDriver driver;
    private readonly HandlerAccessor handlers;
    private readonly PartialIndexFilterNormalizer indexFilterNormalizer;

    public override StorageTypeInfo CreateType(Type type, int? length, int? precision, int? scale)
    {
      var sqlValueType = driver.BuildValueType(type, length, precision, scale);
      return new StorageTypeInfo(
        sqlValueType.Type.ToClrType(),
        sqlValueType.Length,
        sqlValueType.Scale,
        sqlValueType.Precision,
        sqlValueType);
    }

    public override SecondaryIndexInfo CreateSecondaryIndex(TableInfo owningTable, string indexName, IndexInfo originalModelIndex)
    {
      var index = base.CreateSecondaryIndex(owningTable, indexName, originalModelIndex);
      if (originalModelIndex.Filter != null) {
        if (providerInfo.Supports(ProviderFeatures.PartialIndexes))
          index.Filter = TranslateFilterExpression(originalModelIndex);
        else
          Log.Warning(Strings.LogStorageXDoesNotSupportPartialIndexesIgnoringFilterForPartialIndexY,
            domainHandler.Domain.Configuration.ConnectionInfo.Provider, originalModelIndex);
      }
      return index;
    }

    private PartialIndexFilterInfo TranslateFilterExpression(IndexInfo index)
    {
      var table = SqlDml.TableRef(CreateStubTable(index.ReflectedType.MappingName, index.Filter.Fields.Count));
      // Translation of ColumnRefs without alias seems broken, use original name as alias.
      var columns = index.Filter.Fields
        .Select(field => field.Column.Name)
        .Select((name, i) => SqlDml.ColumnRef(table.Columns[i], name))
        .Cast<SqlExpression>()
        .ToList();
      var processor = new ExpressionProcessor(index.Filter.Expression, handlers, null, columns);
      var fragment = SqlDml.Fragment(processor.Translate());
      var expression = driver.Compile(fragment).GetCommandText();
      return new PartialIndexFilterInfo(expression, indexFilterNormalizer.Normalize(expression));
    }

    private Table CreateStubTable(string name, int columnsCount)
    {
      var catalog = new Catalog(string.Empty);
      var schema = catalog.CreateSchema(string.Empty);
      var table = schema.CreateTable(name);
      for (int i = 0; i < columnsCount; i++)
        table.CreateColumn("c" + i);
      return table;
    }


    // Constructors

    public StorageModelBuilder(HandlerAccessor handlers, PartialIndexFilterNormalizer indexFilterNormalizer)
    {
      ArgumentValidator.EnsureArgumentNotNull(handlers, "handlers");
      ArgumentValidator.EnsureArgumentNotNull(indexFilterNormalizer, "indexFilterNormalizer");

      this.handlers = handlers;
      this.indexFilterNormalizer = indexFilterNormalizer;

      providerInfo = handlers.ProviderInfo;
      domainHandler = handlers.DomainHandler;
      driver = handlers.StorageDriver;
    }
  }
}
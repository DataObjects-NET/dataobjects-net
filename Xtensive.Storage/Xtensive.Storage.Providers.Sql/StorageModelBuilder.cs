// Copyright (C) 2011 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2011.10.10

using System;
using System.Collections.Generic;
using System.Linq;
using Xtensive.Core;
using Xtensive.Sql;
using Xtensive.Sql.Dml;
using Xtensive.Sql.Model;
using Xtensive.Storage.Indexing.Model;
using Xtensive.Storage.Providers.Sql.Expressions;
using Xtensive.Storage.Providers.Sql.Resources;
using Xtensive.Storage.Upgrade;
using IndexInfo = Xtensive.Storage.Model.IndexInfo;

namespace Xtensive.Storage.Providers.Sql
{
  /// <summary>
  /// <see cref="StorageModelBuilder"/> for SQL storages.
  /// </summary>
  public class StorageModelBuilder : Upgrade.StorageModelBuilder
  {
    private readonly DomainHandler domainHandler;

    public override TypeInfo CreateType(Type type, int? length, int? precision, int? scale)
    {
      var sqlValueType = domainHandler.Driver.BuildValueType(type, length, precision, scale);
      return new TypeInfo(
        sqlValueType.Type.ToClrType(),
        sqlValueType.Length,
        sqlValueType.Scale,
        sqlValueType.Precision,
        sqlValueType);
    }

    public override SecondaryIndexInfo CreateSecondaryIndex(TableInfo owningTable, string indexName, Model.IndexInfo originalModelIndex)
    {
      var index = base.CreateSecondaryIndex(owningTable, indexName, originalModelIndex);
      if (originalModelIndex.Filter != null) {
        if (domainHandler.ProviderInfo.Supports(ProviderFeatures.PartialIndexes))
          index.Filter = new PartialIndexFilterInfo(TranslateFilterExpression(originalModelIndex));
        else
          Log.Warning(Strings.LogStorageXDoesNotSupportPartialIndexesIgnoringFilterForPartialIndexY,
            domainHandler.Domain.Configuration.ConnectionInfo.Provider, originalModelIndex);
      }
      return index;
    }

    private string TranslateFilterExpression(IndexInfo index)
    {
      var table = SqlDml.TableRef(CreateStubTable(index.ReflectedType.MappingName, index.Filter.Fields.Count));
      // Translation of ColumnRefs without alias seems broken, use original name as alias.
      var columns = index.Filter.Fields
        .Select(field => field.Column.Name)
        .Select((name, i) => SqlDml.ColumnRef(table.Columns[i], name))
        .Cast<SqlExpression>()
        .ToList();
      var processor = new ExpressionProcessor(index.Filter.Expression, domainHandler, null, columns);
      var fragment = SqlDml.Fragment(processor.Translate());
      return domainHandler.Driver.Compile(fragment).GetCommandText();
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

    public StorageModelBuilder(DomainHandler domainHandler)
    {
      ArgumentValidator.EnsureArgumentNotNull(domainHandler, "domainHandler");
      this.domainHandler = domainHandler;
    }
  }
}
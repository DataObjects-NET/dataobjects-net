// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.09.10

using Xtensive.Sql.Common;
using Xtensive.Sql.Dom;
using Xtensive.Sql.Dom.Database;
using Xtensive.Storage.Providers.Sql;
using SqlFactory = Xtensive.Sql.Dom.Sql;
using Xtensive.Sql.Dom.Dml;
using Xtensive.Storage.Model;

namespace Xtensive.Storage.Providers.MsSql
{
  /// <summary>
  /// Generator factory
  /// </summary>
  public sealed class GeneratorFactory : Providers.GeneratorFactory
  {
    /// <inheritdoc/>
    protected override Generator CreateGenerator<TFieldType>(HierarchyInfo hierarchy)
    {
      DomainHandler dh = (DomainHandler)Handlers.DomainHandler;
      Schema schema = dh.Schema;
      SqlBatch sqlCreate = null;
      Table genTable = schema.Tables[hierarchy.MappingName];
      SqlDataType idColumnType = dh.GetSqlDataType(typeof (TFieldType), null);
      var cacheSize = hierarchy.GeneratorCacheSize==0 ? 1 : hierarchy.GeneratorCacheSize;

      if (genTable == null) {
        genTable = schema.CreateTable(hierarchy.MappingName);
        var column = genTable.CreateColumn("ID", new SqlValueType(idColumnType));
        column.SequenceDescriptor = new SequenceDescriptor(column, 0, cacheSize);
        sqlCreate = SqlFactory.Batch();
        sqlCreate.Add(SqlFactory.Create(genTable));
      }

      SqlBatch sqlNext = SqlFactory.Batch();
      SqlInsert insert = SqlFactory.Insert(SqlFactory.TableRef(genTable));
      sqlNext.Add(insert);
      SqlSelect select = SqlFactory.Select();
      select.Columns.Add(SqlFactory.Cast(SqlFactory.FunctionCall("SCOPE_IDENTITY"), idColumnType));
      sqlNext.Add(select);

      return new SqlCachingGenerator<TFieldType>(hierarchy, cacheSize, sqlNext, sqlCreate);
    }
  }
}
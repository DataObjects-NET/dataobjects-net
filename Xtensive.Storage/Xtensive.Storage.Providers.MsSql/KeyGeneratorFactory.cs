// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.09.10

using System;
using System.Linq;
using Xtensive.Sql.Dom.Database;
using Xtensive.Sql.Dom.Dml;
using Xtensive.Storage.Model;
using Xtensive.Storage.Providers.Sql;
using SqlFactory = Xtensive.Sql.Dom.Sql;
using Xtensive.Sql.Dom;

namespace Xtensive.Storage.Providers.MsSql
{
  /// <summary>
  /// Generator factory.
  /// </summary>
  public sealed class KeyGeneratorFactory : Sql.KeyGeneratorFactory
  {
    /// <inheritdoc/>
    /// <exception cref="DomainBuilderException"><c>DomainBuilderException</c>.</exception>
    protected override KeyGenerator CreateGenerator<TFieldType>(GeneratorInfo generatorInfo)
    {
      var dh = (DomainHandler) Handlers.DomainHandler;
      var schema = dh.Schema;
      var columnType = dh.ValueTypeMapper.BuildSqlValueType(generatorInfo.TupleDescriptor[0], 0);

      var genTable = schema.Tables.FirstOrDefault(t => t.Name==generatorInfo.MappingName);
      if (genTable==null)
        throw new DomainBuilderException(
          string.Format("Can not find table '{0}' in storage.", generatorInfo.MappingName));
      var column = genTable.Columns.FirstOrDefault(c => c.Name=="ID") as TableColumn;
      if (column==null)
        throw new DomainBuilderException(
          string.Format("Can not find column '{0}' in table '{1}'.", "ID", genTable.Name));

      SqlBatch sqlNext = SqlFactory.Batch();
      SqlInsert insert = SqlFactory.Insert(SqlFactory.TableRef(genTable));
      sqlNext.Add(insert);
      SqlSelect select = SqlFactory.Select();
      select.Columns.Add(SqlFactory.Cast(SqlFactory.FunctionCall("SCOPE_IDENTITY"), columnType.DataType));
      sqlNext.Add(select);
      return new SqlCachingKeyGenerator<TFieldType>(generatorInfo, sqlNext);
    }
  }
}
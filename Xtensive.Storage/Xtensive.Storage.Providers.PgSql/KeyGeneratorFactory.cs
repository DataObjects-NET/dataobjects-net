// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.09.10

using Xtensive.Sql.Dom.Database;
using Xtensive.Storage.Providers.Sql;
using SqlFactory = Xtensive.Sql.Dom.Sql;
using Xtensive.Sql.Dom.Dml;
using Xtensive.Storage.Model;

namespace Xtensive.Storage.Providers.PgSql
{
  /// <summary>
  /// Generator factory
  /// </summary>
  public sealed class KeyGeneratorFactory : Providers.KeyGeneratorFactory
  {
    /// <inheritdoc/>
    protected override KeyGenerator CreateGenerator<TFieldType>(HierarchyInfo hierarchy)
    {
      DomainHandler dh = (DomainHandler)Handlers.DomainHandler;
      Schema schema = dh.Schema;
      SqlBatch sqlCreate = null;
      Sequence sequence = schema.Sequences[hierarchy.MappingName];

      if (sequence == null) {
        sequence = schema.CreateSequence(hierarchy.MappingName);
        sequence.SequenceDescriptor = new SequenceDescriptor(sequence, hierarchy.KeyGeneratorCacheSize, hierarchy.KeyGeneratorCacheSize);
        sequence.DataType = dh.ValueTypeMapper.BuildSqlValueType(hierarchy.KeyColumns[0]);
        sqlCreate = SqlFactory.Batch();
        sqlCreate.Add(SqlFactory.Create(sequence));
      }

      SqlSelect select = SqlFactory.Select();
      select.Columns.Add(SqlFactory.NextValue(sequence));

      return new SqlCachingKeyGenerator<TFieldType>(hierarchy, hierarchy.KeyGeneratorCacheSize, select, sqlCreate);
    }
  }
}
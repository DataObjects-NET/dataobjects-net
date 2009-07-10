// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.09.10

using System.Linq;
using Xtensive.Sql;
using Xtensive.Storage.Model;
using Xtensive.Storage.Providers.Sql;

namespace Xtensive.Storage.Providers.PgSql
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
      var sequence = schema.Sequences.FirstOrDefault(s => s.Name==generatorInfo.MappingName);
      if (sequence==null)
        throw new DomainBuilderException(
          string.Format(Resources.Strings.ExSequenceXIsNotFound, generatorInfo.MappingName));
      dh.ValueTypeMapper.BuildSqlValueType(generatorInfo.TupleDescriptor[0], 0);
      
      var sqlNext = SqlDml.Select();
      sqlNext.Columns.Add(SqlDml.NextValue(sequence));

      return new SqlCachingKeyGenerator<TFieldType>(generatorInfo, sqlNext);
    }
  }
}
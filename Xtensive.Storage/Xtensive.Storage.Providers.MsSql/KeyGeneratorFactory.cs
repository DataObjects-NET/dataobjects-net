// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.09.10

using System.Linq;
using Xtensive.Sql;
using Xtensive.Sql.Model;
using Xtensive.Storage.Model;
using Xtensive.Storage.Providers.Sql;

namespace Xtensive.Storage.Providers.MsSql
{
  /// <summary>
  /// Generator factory.
  /// </summary>
  public sealed class KeyGeneratorFactory : Sql.KeyGeneratorFactory
  {
    private const string ScopeIdentityFunctionName = "SCOPE_IDENTITY";

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
          string.Format(Resources.Strings.ExTableXIsNotFound, generatorInfo.MappingName));
      var column = genTable.Columns.FirstOrDefault(c => c.Name==WellKnown.GeneratorColumnName) as TableColumn;
      if (column==null)
        throw new DomainBuilderException(
          string.Format(Resources.Strings.ExColumnXIsNotFoundInTableY, WellKnown.GeneratorColumnName, genTable.Name));

      var sqlNext = SqlDml.Batch();
      var insert = SqlDml.Insert(SqlDml.TableRef(genTable));
      sqlNext.Add(insert);
      var select = SqlDml.Select();
      select.Columns.Add(SqlDml.Cast(SqlDml.FunctionCall(ScopeIdentityFunctionName), columnType));
      sqlNext.Add(select);
      return new SqlCachingKeyGenerator<TFieldType>(generatorInfo, sqlNext);
    }
  }
}
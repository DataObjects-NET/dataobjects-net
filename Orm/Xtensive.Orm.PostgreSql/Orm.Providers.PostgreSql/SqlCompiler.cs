// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.04.27

using System.Collections.Generic;
using System.Linq;
using Xtensive.Core;
using Xtensive.Orm.Rse;
using Xtensive.Orm.Rse.Compilation;
using Xtensive.Orm.Rse.Providers;
using Xtensive.Sql;
using Xtensive.Sql.Dml;

namespace Xtensive.Orm.Providers.PostgreSql
{
  internal class SqlCompiler : Providers.SqlCompiler
  {
    protected override SqlProvider VisitFreeText(FreeTextProvider provider)
    {
      var rankColumnName = provider.Header.Columns.Last().Name;
      var stringTypeMapping = Driver.GetTypeMapping(typeof(string));
      var binding = new QueryParameterBinding(stringTypeMapping,
        provider.SearchCriteria.Invoke, QueryParameterBindingType.Regular);

      var select = SqlDml.Select();
      var realPrimaryIndex = provider.PrimaryIndex.Resolve(Handlers.Domain.Model);
      var index = realPrimaryIndex.ReflectedType.Indexes.PrimaryIndex;
      var query = BuildProviderQuery(index);
      var table = Mapping[realPrimaryIndex.ReflectedType];
      var fromTable = SqlDml.FreeTextTable(table, binding.ParameterReference,
        table.Columns.Select(column => column.Name).Append(rankColumnName).ToList());
      var fromTableRef = SqlDml.QueryRef(fromTable);
      foreach (var column in query.Columns) {
        select.Columns.Add(fromTableRef.Columns[column.Name] ?? column);
      }

      select.Columns.Add(SqlDml.Cast(fromTableRef.Columns[rankColumnName], SqlType.Double), rankColumnName);
      select.From = fromTableRef;
      if (provider.TopN == null) {
        return CreateProvider(select, binding, provider);
      }

      var intTypeMapping = Driver.GetTypeMapping(typeof(int));
      var topNBinding = new QueryParameterBinding(
        intTypeMapping, context => provider.TopN.Invoke(context), QueryParameterBindingType.Regular);
      select.Limit = topNBinding.ParameterReference;
      select.OrderBy.Add(select.Columns[rankColumnName], false);
      return CreateProvider(select, new[] {binding, topNBinding}, provider);

    }

    protected override SqlExpression ProcessAggregate(SqlProvider source, List<SqlExpression> sourceColumns, AggregateColumn aggregateColumn)
    {
      var result = base.ProcessAggregate(source, sourceColumns, aggregateColumn);
      if (aggregateColumn.AggregateType==AggregateType.Sum || aggregateColumn.AggregateType==AggregateType.Avg)
        result = SqlDml.Cast(result, Driver.MapValueType(aggregateColumn.Type));
      return result;
    }

    public SqlCompiler(HandlerAccessor handlers, CompilerConfiguration configuration)
      : base(handlers, configuration)
    {
    }
  }
}
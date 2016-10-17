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
      SqlFreeTextTable fromTable;
      QueryParameterBinding [] bindings;

      var stringTypeMapping = Driver.GetTypeMapping(typeof (string));
      var criteriaBinding = new QueryParameterBinding(stringTypeMapping,
        provider.SearchCriteria.Invoke, QueryParameterBindingType.Regular);
      var realPrimaryIndex = provider.PrimaryIndex.Resolve(Handlers.Domain.Model);
      var index = realPrimaryIndex.ReflectedType.Indexes.PrimaryIndex;
      var query = BuildProviderQuery(index);
      var table = Mapping[realPrimaryIndex.ReflectedType];
      var rankColumnName = provider.Header.Columns.Last().Name;

      if (provider.TopN==null) {
        fromTable = SqlDml.FreeTextTable(
          table, criteriaBinding.ParameterReference, table.Columns.Select(column => column.Name).AddOne(rankColumnName).ToList());
        bindings = new[] { criteriaBinding };
      }
      else {
        var intTypeMapping = Driver.GetTypeMapping(typeof(int));
        var topNBinding = new QueryParameterBinding(intTypeMapping,
          () => provider.TopN.Invoke(), QueryParameterBindingType.Regular);
        fromTable = SqlDml.FreeTextTable(
          table, criteriaBinding.ParameterReference, table.Columns.Select(column => column.Name).AddOne(rankColumnName).ToList(), topNBinding.ParameterReference);
        bindings = new[] { criteriaBinding, topNBinding };
      }
      var fromTableRef = SqlDml.QueryRef(fromTable);
      SqlSelect select = SqlDml.Select();
      foreach (var column in query.Columns)
        select.Columns.Add(fromTableRef.Columns[column.Name] ?? column);
      select.Columns.Add(SqlDml.Cast(fromTableRef.Columns[rankColumnName], SqlType.Double), rankColumnName);
      select.From = fromTableRef;
      return CreateProvider(select, bindings, provider);
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
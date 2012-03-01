// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.04.27

using System;
using System.Collections.Generic;
using System.Linq;
using Xtensive.Collections;
using Xtensive.Core;
using Xtensive.Orm.Providers.Sql;
using Xtensive.Sql.Dml;
using Xtensive.Sql;
using Xtensive.Orm.Rse;
using Xtensive.Orm.Rse.Providers.Compilable;

namespace Xtensive.Orm.Providers.PostgreSql
{
  internal class SqlCompiler : Sql.SqlCompiler
  {
    protected override SqlProvider VisitFreeText(FreeTextProvider provider)
    {
      var domainHandler = Handlers.DomainHandler;
      var rankColumnName = provider.Header.Columns.Last().Name;

      var stringTypeMapping = Driver.GetTypeMapping(typeof (string));
      var binding = new QueryParameterBinding(
        provider.SearchCriteria.Invoke,
        stringTypeMapping,
        QueryParameterBindingType.Regular);

      SqlSelect select = SqlDml.Select();
      var realPrimaryIndex = provider.PrimaryIndex.Resolve(Handlers.Domain.Model);
      var index = realPrimaryIndex.ReflectedType.Indexes.PrimaryIndex;
      var query = BuildProviderQuery(index);
      var table = domainHandler.Schema.Tables[realPrimaryIndex.ReflectedType.MappingName];
      var fromTable = SqlDml.FreeTextTable(table, binding.ParameterReference, table.Columns.Select(column => column.Name).AddOne(rankColumnName).ToList());
      var fromTableRef = SqlDml.QueryRef(fromTable);
      foreach (var column in query.Columns)
        select.Columns.Add(fromTableRef.Columns[column.Name] ?? column);
      select.Columns.Add(SqlDml.Cast(fromTableRef.Columns[rankColumnName], SqlType.Double), rankColumnName);
      select.From = fromTableRef;
      return CreateProvider(select, binding, provider);
    }

    protected override SqlExpression ProcessAggregate(SqlProvider source, List<SqlExpression> sourceColumns, AggregateColumn aggregateColumn)
    {
      var result = base.ProcessAggregate(source, sourceColumns, aggregateColumn);
      if (aggregateColumn.AggregateType==AggregateType.Sum || aggregateColumn.AggregateType==AggregateType.Avg)
        result = SqlDml.Cast(result, Driver.BuildValueType(aggregateColumn.Type));
      return result;
    }

    public SqlCompiler(HandlerAccessor handlers)
      : base(handlers)
    {
    }
  }
}
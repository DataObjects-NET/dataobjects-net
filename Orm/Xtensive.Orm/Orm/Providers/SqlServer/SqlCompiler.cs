// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Vakhtina Elena
// Created:    2009.02.13

using System;
using System.Collections.Generic;
using System.Linq;
using Xtensive.Collections;
using Xtensive.Orm.Rse;
using Xtensive.Orm.Rse.Compilation;
using Xtensive.Orm.Rse.Providers;
using Xtensive.Sql;
using Xtensive.Sql.Dml;

namespace Xtensive.Orm.Providers.SqlServer
{
  internal class SqlCompiler : Providers.SqlCompiler
  {
    protected override SqlProvider VisitFreeText(FreeTextProvider provider)
    {
      SqlFreeTextTable fromTable;
      QueryParameterBinding[] bindings;

      var stringTypeMapping = Driver.GetTypeMapping(typeof(string));
      var criteriaBinding = new QueryParameterBinding(
       stringTypeMapping, provider.SearchCriteria.Invoke, QueryParameterBindingType.Regular);
     
      var index = provider.PrimaryIndex.Resolve(Handlers.Domain.Model);
      var table = Mapping[index.ReflectedType];
      var columns = provider.Header.Columns.Select(column => column.Name).ToList();

      if (provider.TopN==null) {
        fromTable = SqlDml.FreeTextTable(table, criteriaBinding.ParameterReference, columns);
        bindings = new[] {criteriaBinding};
      }
      else {
        var intTypeMapping = Driver.GetTypeMapping(typeof(int));
        var topNBinding = new QueryParameterBinding(intTypeMapping, () => provider.TopN.Invoke(), QueryParameterBindingType.Regular);
        fromTable = SqlDml.FreeTextTable(table, criteriaBinding.ParameterReference, columns, topNBinding.ParameterReference);
        bindings = new[] { criteriaBinding, topNBinding };
      }
      var fromTableRef = SqlDml.QueryRef(fromTable);
      SqlSelect select = SqlDml.Select();
      select.Columns.Add(fromTableRef.Columns[0]);
      select.Columns.Add(SqlDml.Cast(fromTableRef.Columns[1], SqlType.Double), "RANK");
      select.From = fromTableRef;

      return CreateProvider(select, bindings, provider);
    }

    protected override SqlExpression ProcessAggregate(
      SqlProvider source, List<SqlExpression> sourceColumns, AggregateColumn aggregateColumn)
    {
      var aggregateType = aggregateColumn.Type;
      var result = base.ProcessAggregate(source, sourceColumns, aggregateColumn);
      if (aggregateColumn.AggregateType==AggregateType.Avg) {
        var originType = source.Origin.Header.Columns[aggregateColumn.SourceIndex].Type;
        // floats are promoted to doubles, but we need the same type
        if (originType==aggregateType && originType!=typeof (float))
          return result;
        var sqlType = Driver.MapValueType(aggregateType);
        return SqlDml.Cast(SqlDml.Avg(SqlDml.Cast(sourceColumns[aggregateColumn.SourceIndex], sqlType)), sqlType);
      }
      // cast to decimal is dangerous, because 'decimal' defaults to integer type
      if (aggregateColumn.AggregateType==AggregateType.Sum && aggregateType!=typeof(decimal))
        return SqlDml.Cast(result, Driver.MapValueType(aggregateType));
      return result;
    }


    // Constructors

    public SqlCompiler(HandlerAccessor handlers, CompilerConfiguration configuration)
      : base(handlers, configuration)
    {
    }
  }
}
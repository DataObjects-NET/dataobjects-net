// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Vakhtina Elena
// Created:    2009.02.13

using System.Collections.Generic;
using System.Linq;
using Xtensive.Orm.Rse;
using Xtensive.Orm.Rse.Providers;
using Xtensive.Sql;
using Xtensive.Sql.Dml;

namespace Xtensive.Orm.Providers.SqlServer
{
  internal class SqlCompiler : Providers.SqlCompiler
  {
    protected override SqlProvider VisitFreeText(FreeTextProvider provider)
    {
      var domainHandler = DomainHandler;
      var stringTypeMapping = Driver.GetTypeMapping(typeof (string));
      var binding = new QueryParameterBinding(stringTypeMapping,
        provider.SearchCriteria.Invoke, QueryParameterBindingType.Regular);

      SqlSelect select = SqlDml.Select();
      var index = provider.PrimaryIndex.Resolve(Handlers.Domain.Model);
      var table = Mapping[index.ReflectedType];
      var fromTable = SqlDml.FreeTextTable(table, binding.ParameterReference,
        provider.Header.Columns.Select(column=>column.Name).ToList());
      var fromTableRef = SqlDml.QueryRef(fromTable);
      select.Columns.Add(fromTableRef.Columns[0]);
      select.Columns.Add(SqlDml.Cast(fromTableRef.Columns[1], SqlType.Double), "RANK");
      select.From = fromTableRef;
      return CreateProvider(select, binding, provider);
    }

        protected override SqlProvider VisitContainsTable(ContainsTableProvider provider)
        {
            var stringTypeMapping = Driver.GetTypeMapping(typeof (string));
            var binding = new QueryParameterBinding(stringTypeMapping,
                provider.SearchCriteria.Invoke, QueryParameterBindingType.Regular);

            SqlSelect select = SqlDml.Select();
            var index = provider.PrimaryIndex.Resolve(Handlers.Domain.Model);
            var table = Mapping[index.ReflectedType];
            var fromTable = SqlDml.ContainsTable(table, binding.ParameterReference,
                    provider.Header.Columns.Select(column => column.Name).ToList(),
                    provider.TargetColumnNames.Invoke());
            var fromTableRef = SqlDml.QueryRef(fromTable);
            select.Columns.Add(fromTableRef.Columns[0]);
            select.Columns.Add(SqlDml.Cast(fromTableRef.Columns[1], SqlType.Double), "RANK");
            select.From = fromTableRef;
            return CreateProvider(select, binding, provider);
        }

        protected override SqlExpression ProcessAggregate(
            SqlProvider source, List<SqlExpression> sourceColumns, AggregateColumn aggregateColumn)
        {
            var aggregateType = aggregateColumn.Type;
            var result = base.ProcessAggregate(source, sourceColumns, aggregateColumn);
            if (aggregateColumn.AggregateType == AggregateType.Avg)
            {
                var originType = source.Origin.Header.Columns[aggregateColumn.SourceIndex].Type;
                // floats are promoted to doubles, but we need the same type
                if (originType == aggregateType && originType != typeof (float))
                    return result;
                var sqlType = Driver.MapValueType(aggregateType);
                return SqlDml.Cast(SqlDml.Avg(SqlDml.Cast(sourceColumns[aggregateColumn.SourceIndex], sqlType)), sqlType);
            }
            // cast to decimal is dangerous, because 'decimal' defaults to integer type
            if (aggregateColumn.AggregateType == AggregateType.Sum && aggregateType != typeof (decimal))
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
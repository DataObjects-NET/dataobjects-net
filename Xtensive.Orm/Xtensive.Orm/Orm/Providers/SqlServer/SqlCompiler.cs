// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Vakhtina Elena
// Created:    2009.02.13

using System.Collections.Generic;
using Xtensive.Collections;
using Xtensive.Sql;
using Xtensive.Sql.Dml;
using Xtensive.Storage.Rse;
using Xtensive.Storage.Rse.Providers.Compilable;
using System.Linq;

namespace Xtensive.Orm.Providers.Sql.Servers.SqlServer
{
  internal class SqlCompiler : ManualPagingSqlCompiler
  {
    protected override SqlProvider VisitFreeText(FreeTextProvider provider)
    {
      var domainHandler = (DomainHandler) Handlers.DomainHandler;
      var stringTypeMapping = domainHandler.Driver.GetTypeMapping(typeof (string));
      var binding = new QueryParameterBinding(
        provider.SearchCriteria.Invoke,
        stringTypeMapping,
        QueryParameterBindingType.Regular);

      SqlSelect select = SqlDml.Select();
      var index = provider.PrimaryIndex.Resolve(Handlers.Domain.Model);
      var table = domainHandler.Schema.Tables[index.ReflectedType.MappingName];
      var fromTable = SqlDml.FreeTextTable(table, binding.ParameterReference, provider.Header.Columns.Select(column=>column.Name).ToList());
      var fromTableRef = SqlDml.QueryRef(fromTable);
      select.Columns.Add(fromTableRef.Columns[0]);
      select.Columns.Add(SqlDml.Cast(fromTableRef.Columns[1], SqlType.Double), "RANK");
      select.From = fromTableRef;
      return CreateProvider(select, binding, provider);
    }

    protected override SqlProvider VisitTake(TakeProvider provider)
    {
      var compiledSource = Compile(provider.Source);

      var query = ExtractSqlSelect(provider, compiledSource);
      var binding = CreateLimitOffsetParameterBinding(provider.Count);
      query.Limit = binding.ParameterReference;
      return CreateProvider(query, binding, provider, compiledSource);
    }

    protected override SqlExpression ProcessAggregate(SqlProvider source, List<SqlExpression> sourceColumns, AggregateColumn aggregateColumn)
    {
      var aggregateType = aggregateColumn.Type;
      var result = base.ProcessAggregate(source, sourceColumns, aggregateColumn);
      if (aggregateColumn.AggregateType==AggregateType.Avg) {
        var originType = source.Origin.Header.Columns[aggregateColumn.SourceIndex].Type;
        // floats are promoted to doubles, but we need the same type
        if (originType==aggregateType && originType!=typeof (float))
          return result;
        var sqlType = Driver.BuildValueType(aggregateType);
        return SqlDml.Cast(SqlDml.Avg(SqlDml.Cast(sourceColumns[aggregateColumn.SourceIndex], sqlType)), sqlType);
      }
      // cast to decimal is dangerous, because 'decimal' defaults to integer type
      if (aggregateColumn.AggregateType==AggregateType.Sum && aggregateType!=typeof(decimal))
        return SqlDml.Cast(result, Driver.BuildValueType(aggregateType));
      return result;
    }


    // Constructors

    public SqlCompiler(HandlerAccessor handlers)
      : base(handlers)
    {
    }
  }
}
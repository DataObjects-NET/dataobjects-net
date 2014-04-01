// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Vakhtina Elena
// Created:    2009.02.13

using System.Collections.Generic;
using Xtensive.Orm.Rse;
using Xtensive.Orm.Rse.Compilation;
using Xtensive.Sql;
using Xtensive.Sql.Dml;

namespace Xtensive.Orm.Providers.SqlServerCe
{
  internal class SqlCompiler : Providers.SqlCompiler
  {
    protected override SqlExpression ProcessAggregate(SqlProvider source, List<SqlExpression> sourceColumns, AggregateColumn aggregateColumn)
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
      if (aggregateColumn.AggregateType==AggregateType.Count) {
        var sqlType = Driver.MapValueType(typeof(long));
        return SqlDml.Cast(SqlDml.Count(sourceColumns[aggregateColumn.SourceIndex]), sqlType);
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
// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.04.27

using System;
using System.Collections.Generic;
using Xtensive.Core.Collections;
using Xtensive.Sql.Dml;
using Xtensive.Sql;
using Xtensive.Storage.Providers.Sql;
using Xtensive.Storage.Rse;
using Xtensive.Storage.Rse.Providers;

namespace Xtensive.Storage.Providers.PgSql
{
  [Serializable]
  internal class PgSqlCompiler : SqlCompiler
  {
    protected override SqlExpression TranslateAggregate(SqlProvider source, List<SqlTableColumn> sourceColumns, AggregateColumn aggregateColumn)
    {
      var result = base.TranslateAggregate(source, sourceColumns, aggregateColumn);
      if (aggregateColumn.AggregateType == AggregateType.Sum || aggregateColumn.AggregateType == AggregateType.Avg) {
        Type type = aggregateColumn.Type;
        return SqlDml.Cast(result, ValueTypeMapper.BuildSqlValueType(type, null));
      }
      return result;
    }

    public PgSqlCompiler(HandlerAccessor handlers, BindingCollection<object, ExecutableProvider> compiledSources)
      : base(handlers, compiledSources)
    {
    }
  }
}
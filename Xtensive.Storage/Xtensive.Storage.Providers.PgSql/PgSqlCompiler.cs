// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.04.27

using System;
using System.Collections.Generic;
using Xtensive.Core.Collections;
using Xtensive.Sql.Dom.Dml;
using Xtensive.Storage.Providers.Sql;
using Xtensive.Storage.Rse;
using Xtensive.Storage.Rse.Providers;
using SqlFactory = Xtensive.Sql.Dom.Sql;

namespace Xtensive.Storage.Providers.PgSql
{
  [Serializable]
  internal class PgSqlCompiler : SqlCompiler
  {
    protected override SqlExpression TranslateAggregate(SqlProvider source, List<SqlTableColumn> sourceColumns, AggregateColumn aggregateColumn)
    {
      var result = base.TranslateAggregate(source, sourceColumns, aggregateColumn);
      if (aggregateColumn.AggregateType == AggregateType.Sum || aggregateColumn.AggregateType == AggregateType.Avg) 
        return SqlFactory.Cast(result, GetSqlDataType(aggregateColumn.Type));
      return result;
    }

    public PgSqlCompiler(HandlerAccessor handlers, BindingCollection<object, ExecutableProvider> compiledSources)
      : base(handlers, compiledSources)
    {
    }
  }
}
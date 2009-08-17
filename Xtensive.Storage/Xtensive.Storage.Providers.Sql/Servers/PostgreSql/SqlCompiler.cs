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
using Xtensive.Storage.Rse;
using Xtensive.Storage.Rse.Providers;

namespace Xtensive.Storage.Providers.Sql.Servers.PostgreSql
{
  [Serializable]
  internal class SqlCompiler : Sql.SqlCompiler
  {
    protected override SqlExpression ProcessAggregate(SqlProvider source, List<SqlTableColumn> sourceColumns, AggregateColumn aggregateColumn)
    {
      var result = base.ProcessAggregate(source, sourceColumns, aggregateColumn);
      if (aggregateColumn.AggregateType == AggregateType.Sum || aggregateColumn.AggregateType == AggregateType.Avg) {
        Type type = aggregateColumn.Type;
        return SqlDml.Cast(result, Driver.BuildValueType(type, null, null, null));
      }
      return result;
    }

    public SqlCompiler(HandlerAccessor handlers, BindingCollection<object, ExecutableProvider> compiledSources)
      : base(handlers, compiledSources)
    {
    }
  }
}
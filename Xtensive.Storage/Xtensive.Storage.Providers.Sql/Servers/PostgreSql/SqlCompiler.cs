// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.04.27

using System;
using System.Collections.Generic;
using Xtensive.Sql.Dml;
using Xtensive.Sql;
using Xtensive.Storage.Providers.Sql.Resources;
using Xtensive.Storage.Rse;
using Xtensive.Storage.Rse.Providers.Compilable;

namespace Xtensive.Storage.Providers.Sql.Servers.PostgreSql
{
  internal class SqlCompiler : Sql.SqlCompiler
  {
    private bool supportsRowNumber;

    protected override SqlExpression ProcessAggregate(SqlProvider source, List<SqlExpression> sourceColumns, AggregateColumn aggregateColumn)
    {
      var result = base.ProcessAggregate(source, sourceColumns, aggregateColumn);
      if (aggregateColumn.AggregateType==AggregateType.Sum || aggregateColumn.AggregateType==AggregateType.Avg)
        result = SqlDml.Cast(result, Driver.BuildValueType(aggregateColumn.Type));
      return result;
    }

    protected override SqlProvider VisitRowNumber(RowNumberProvider provider)
    {
      if (!supportsRowNumber)
        throw new NotSupportedException(Strings.ExRowNumberWindowFunctionIsNotSupportedOnThisVersionOfPostgreSql);
      return base.VisitRowNumber(provider);
    }

    public SqlCompiler(HandlerAccessor handlers)
      : base(handlers)
    {
      var version = handlers.DomainHandler.ProviderInfo.StorageVersion;
      supportsRowNumber = version.Major > 8 || version.Major==8 && version.Minor >= 4;
    }
  }
}
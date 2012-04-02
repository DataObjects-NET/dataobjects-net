// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.08.07

using System;
using System.Collections.Generic;
using Xtensive.Core;
using Xtensive.Orm.Providers;
using Tuple = Xtensive.Tuples.Tuple;
using Xtensive.Sql;
using Xtensive.Sql.Dml;
using Xtensive.Orm.Rse;
using Xtensive.Orm.Rse.Providers.Compilable;

namespace Xtensive.Orm.Providers.Oracle
{
  internal class SqlCompiler : Providers.SqlCompiler
  {
    protected override string ProcessAliasedName(string name)
    {
      return Handlers.NameBuilder.ApplyNamingRules(name);
    }

    protected override SqlExpression ProcessAggregate(SqlProvider source, List<SqlExpression> sourceColumns, AggregateColumn aggregateColumn)
    {
      var result = base.ProcessAggregate(source, sourceColumns, aggregateColumn);
      if (aggregateColumn.AggregateType==AggregateType.Avg) {
        switch (Type.GetTypeCode(aggregateColumn.Type)) {
        case TypeCode.Single:
        case TypeCode.Double:
          result = SqlDml.Cast(result, Driver.BuildValueType(aggregateColumn.Type));
          break;
        }
      }
      return result;
    }


    // Constructors
    
    public SqlCompiler(HandlerAccessor handlers)
      : base(handlers)
    {
    }
  }
}
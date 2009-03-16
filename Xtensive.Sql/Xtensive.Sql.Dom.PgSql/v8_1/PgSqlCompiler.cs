// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.03.16

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xtensive.Sql.Dom.Dml;

namespace Xtensive.Sql.Dom.PgSql.v8_1
{
  internal class PgSqlCompiler : v8_0.PgSqlCompiler
  {
    public override void Visit(SqlFunctionCall node)
    {
      if (node.FunctionType!=SqlFunctionType.IntervalExtract)
        base.Visit(node);

      var intervalPart = ((SqlLiteral<SqlIntervalPart>) node.Arguments[0]).Value;

      if (intervalPart!=SqlIntervalPart.Day)
        base.Visit(node);

      Visit(Sql.FunctionCall("justify_hours", RealExtractDays(node.Arguments[1])));
    }

    public PgSqlCompiler(PgSqlDriver driver)
      : base(driver)
    {
      
    }
  }
}

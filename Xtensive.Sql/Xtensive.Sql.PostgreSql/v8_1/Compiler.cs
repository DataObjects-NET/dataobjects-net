// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.03.16

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xtensive.Sql.Dml;

namespace Xtensive.Sql.PostgreSql.v8_1
{
  internal class Compiler : v8_0.Compiler
  {
    public override void Visit(SqlFunctionCall node)
    {
      if (node.FunctionType!=SqlFunctionType.IntervalExtract) {
        base.Visit(node);
        return;
      }

      var intervalPart = ((SqlLiteral<SqlIntervalPart>) node.Arguments[0]).Value;

      if (intervalPart!=SqlIntervalPart.Day) {
        base.Visit(node);
        return;
      }

      Visit(RealExtractDays(SqlDml.FunctionCall("justify_hours", node.Arguments[1])));
    }

    // Constructors

    public Compiler(SqlDriver driver)
      : base(driver)
    {
    }
  }
}

// Copyright (C) 2019-2020 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Alexey Kulakov
// Created:    2019.09.25

using System;
using System.Collections.Generic;
using Xtensive.Sql.Dml;

namespace Xtensive.Sql.Drivers.PostgreSql.v10_0
{
  internal class Compiler : v9_1.Compiler
  {
    protected override SqlExpression ConstructDateTime(IList<SqlExpression> arguments) => MakeDateTime(arguments[0], arguments[1], arguments[2]);

    protected static SqlUserFunctionCall MakeDateTime(SqlExpression year, SqlExpression month, SqlExpression day) =>
      SqlDml.FunctionCall("MAKE_TIMESTAMP", year, month, day, SqlDml.Literal(0), SqlDml.Literal(0), SqlDml.Literal(0.0));


    // Constructors

    public Compiler(PostgreSql.Driver driver)
      : base(driver)
    {
    } 
  }
}
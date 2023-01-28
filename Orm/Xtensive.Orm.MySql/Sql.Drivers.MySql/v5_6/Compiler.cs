// Copyright (C) 2013-2023 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Alena Mikshina
// Created:    2013.12.30

using System;
using Xtensive.Sql.Dml;

namespace Xtensive.Sql.Drivers.MySql.v5_6
{
  internal class Compiler : v5_5.Compiler
  {
#if NET6_0_OR_GREATER //DO_DATEONLY
    public override void Visit(SqlFunctionCall node)
    {
      if (node.FunctionType == SqlFunctionType.TimeConstruct) {
        var arguments = node.Arguments;
        Visit(MakeTime(arguments[0], arguments[1], arguments[2], arguments[3]));
        return;
      }
      base.Visit(node);
    }

    protected SqlUserFunctionCall MakeTime(
        SqlExpression hours, SqlExpression minutes, SqlExpression seconds, SqlExpression milliseconds) =>
      SqlDml.FunctionCall("MAKETIME", hours, minutes, seconds + (milliseconds / 1000));

#endif

    // Constructors

    public Compiler(SqlDriver driver)
      : base(driver)
    {
    }
  }
}

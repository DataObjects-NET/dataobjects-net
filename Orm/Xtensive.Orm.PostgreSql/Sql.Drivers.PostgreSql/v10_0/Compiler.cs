// Copyright (C) 2019-2023 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Alexey Kulakov
// Created:    2019.09.25

using Xtensive.Sql.Dml;

namespace Xtensive.Sql.Drivers.PostgreSql.v10_0
{
  internal class Compiler : v9_1.Compiler
  {
    public override void Visit(SqlFunctionCall node)
    {
      var arguments = node.Arguments;
      switch (node.FunctionType) {
        case SqlFunctionType.DateTimeConstruct:
          Visit(MakeDateTime(arguments[0], arguments[1], arguments[2]));
          return;
#if NET6_0_OR_GREATER //DO_DATEONLY
        case SqlFunctionType.DateConstruct:
          Visit(MakeDate(arguments[0], arguments[1], arguments[2]));
          return;
        case SqlFunctionType.TimeConstruct:
          Visit(MakeTime(arguments[0], arguments[1], arguments[2], arguments[3]));
          return;
#endif
        default:
          base.Visit(node);
          return;
      }
    }

    protected static SqlUserFunctionCall MakeDateTime(SqlExpression year, SqlExpression month, SqlExpression day) =>
      SqlDml.FunctionCall("MAKE_TIMESTAMP", year, month, day, SqlDml.Literal(0), SqlDml.Literal(0), SqlDml.Literal(0.0));

#if NET6_0_OR_GREATER //DO_DATEONLY
    protected static SqlUserFunctionCall MakeDate(SqlExpression year, SqlExpression month, SqlExpression day) =>
      SqlDml.FunctionCall("MAKE_DATE", year, month, day);

    protected static SqlUserFunctionCall MakeTime(
       SqlExpression hours, SqlExpression minutes, SqlExpression seconds, SqlExpression milliseconds) =>
     SqlDml.FunctionCall("MAKE_TIME", hours, minutes, seconds + (SqlDml.Cast(milliseconds, SqlType.Double) / 1000));
#endif

    // Constructors

    public Compiler(SqlDriver driver)
      : base(driver)
    {
    } 
  }
}
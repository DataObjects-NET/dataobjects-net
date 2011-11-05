// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.08.17

using System.Linq;
using Xtensive.Sql;
using Xtensive.Sql.Dml;

namespace Xtensive.Orm.Providers.Sql.Expressions
{
  internal sealed class BooleanExpressionConverter
  {
    private readonly SqlValueType booleanType;

    public SqlExpression IntToBoolean(SqlExpression expression)
    {
      // optimization: omitting IntToBoolean(BooleanToInt(x)) sequences
      if (expression.NodeType==SqlNodeType.Cast) {
        var operand = ((SqlCast) expression).Operand;
        if (operand.NodeType==SqlNodeType.Case) {
          var _case = (SqlCase) operand;
          if (_case.Count == 1) {
            var firstCaseItem = _case.First();
            var whenTrue = firstCaseItem.Value as SqlLiteral<int>;
            var whenFalse = _case.Else as SqlLiteral<int>;
            if (!ReferenceEquals(whenTrue, null)
             && !ReferenceEquals(whenFalse, null)
             && whenTrue.Value==1
             && whenFalse.Value==0)
              return firstCaseItem.Key;
          }
        }
      }

      return SqlDml.NotEquals(expression, 0);
    }

    public SqlExpression BooleanToInt(SqlExpression expression)
    {
      // optimization: omitting BooleanToInt(IntToBoolean(x)) sequences
      if (expression.NodeType==SqlNodeType.NotEquals) {
        var binary = (SqlBinary) expression;
        var left = binary.Left;
        var right = binary.Right as SqlLiteral<int>;
        if (!ReferenceEquals(right, null) && right.Value==0)
          return left;
      }

      var result = SqlDml.Case();
      result.Add(expression, 1);
      result.Else = 0;
      return SqlDml.Cast(result, booleanType);
    }

    public BooleanExpressionConverter(Driver driver)
    {
      booleanType = driver.BuildValueType(typeof (bool));
    }
  }
}
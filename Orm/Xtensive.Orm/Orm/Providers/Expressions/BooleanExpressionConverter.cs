// Copyright (C) 2009-2021 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Denis Krjuchkov
// Created:    2009.08.17

using System;
using System.Linq;
using Xtensive.Reflection;
using Xtensive.Sql;
using Xtensive.Sql.Dml;

namespace Xtensive.Orm.Providers
{
  internal sealed class BooleanExpressionConverter
  {
    private readonly SqlValueType booleanType;

    private bool enableEqualInt1Optimization = true;

    public readonly struct DisableEqualInt1OptimizationScope : IDisposable
    {
      private readonly BooleanExpressionConverter converter;
      private readonly bool prevState;

      public void Dispose() =>
        converter.enableEqualInt1Optimization = prevState;

      public DisableEqualInt1OptimizationScope(BooleanExpressionConverter converter)
      {
        (this.converter, prevState) = (converter, converter.enableEqualInt1Optimization);
        converter.enableEqualInt1Optimization = false;
      }
    }

    public DisableEqualInt1OptimizationScope DisableEqualInt1Optimization() => new DisableEqualInt1OptimizationScope(this);

    public SqlExpression IntToBoolean(SqlExpression expression)
    {
      // optimization: omitting IntToBoolean(BooleanToInt(x)) sequences
      if (expression.NodeType == SqlNodeType.Cast) {
        var operand = ((SqlCast) expression).Operand;
        if (operand.NodeType == SqlNodeType.Case) {
          var _case = (SqlCase) operand;
          if (_case.Count == 1) {
            var firstCaseItem = _case.First();
            var whenTrue = firstCaseItem.Value as SqlLiteral<int>;
            var whenFalse = _case.Else as SqlLiteral<int>;
            if (!ReferenceEquals(whenTrue, null)
             && !ReferenceEquals(whenFalse, null)
             && whenTrue.Value == 1
             && whenFalse.Value == 0)
              return firstCaseItem.Key;
          }
        }
      }

      return SqlDml.Equals(expression, 1);
    }

    public SqlExpression BooleanToInt(SqlExpression expression)
    {
      // optimization: omitting BooleanToInt(IntToBoolean(x)) sequences
      if (enableEqualInt1Optimization && expression.NodeType == SqlNodeType.Equals) {
        var binary = (SqlBinary) expression;
        var left = binary.Left;
        var right = binary.Right as SqlLiteral<int>;
        if (!ReferenceEquals(right, null) && right.Value == 1)
          return left;
      }

      var result = SqlDml.Case();
      result.Add(expression, 1);
      result.Else = 0;
      return SqlDml.Cast(result, booleanType);
    }

    public BooleanExpressionConverter(StorageDriver driver)
    {
      booleanType = driver.MapValueType(WellKnownTypes.Bool);
    }
  }
}

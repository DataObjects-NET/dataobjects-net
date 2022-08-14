// Copyright (C) 2009-2020 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Denis Krjuchkov
// Created:    2009.08.17

using System.Collections.Generic;
using System.Linq;
using Xtensive.Reflection;
using Xtensive.Sql;
using Xtensive.Sql.Dml;

namespace Xtensive.Orm.Providers
{
  internal sealed class BooleanExpressionConverter
  {
    private static readonly object IntToBooleanTag = new();
    private static readonly object BooleanToIntTag = new();

    private readonly SqlValueType booleanType;

    public SqlExpression IntToBoolean(SqlExpression expression)
    {
      // optimization: omitting IntToBoolean(BooleanToInt(x)) sequences
      if (expression.NodeType == SqlNodeType.Metadata &&
          expression is SqlMetadata metadata &&
          metadata.Value == BooleanToIntTag) {
        return ((SqlCase) ((SqlCast) metadata.Expression).Operand).First().Key;
      }

      return SqlDml.Metadata(SqlDml.Equals(expression, 1), IntToBooleanTag);
    }

    public SqlExpression BooleanToInt(SqlExpression expression)
    {
      // optimization: omitting BooleanToInt(IntToBoolean(x)) sequences
      if (expression.NodeType == SqlNodeType.Metadata &&
          expression is SqlMetadata metadata &&
          metadata.Value == IntToBooleanTag) {
        return ((SqlBinary) metadata.Expression).Left;
      }

      var result = SqlDml.Case();
      result.Add(expression, 1);
      result.Else = 0;
      return SqlDml.Metadata(SqlDml.Cast(result, booleanType), BooleanToIntTag);
    }

    public BooleanExpressionConverter(StorageDriver driver)
    {
      booleanType = driver.MapValueType(WellKnownTypes.Bool);
    }
  }
}
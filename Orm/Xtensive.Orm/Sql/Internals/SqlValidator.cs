// Copyright (C) 2009-2021 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Linq;
using Xtensive.Core;
using Xtensive.Reflection;
using Xtensive.Sql.Dml;

namespace Xtensive.Sql
{
  internal static class SqlValidator
  {
    private static readonly IReadOnlySet<Type> supportedTypes = new HashSet<Type> {
        WellKnownTypes.Bool,
        WellKnownTypes.Char,
        WellKnownTypes.SByte,
        WellKnownTypes.Byte,
        WellKnownTypes.Int16,
        WellKnownTypes.UInt16,
        WellKnownTypes.Int32,
        WellKnownTypes.UInt32,
        WellKnownTypes.Int64,
        WellKnownTypes.UInt64,
        WellKnownTypes.String,
        WellKnownTypes.Single,
        WellKnownTypes.Double,
        WellKnownTypes.Decimal,
        WellKnownTypes.DateTime,
#if NET6_0_OR_GREATER
        WellKnownTypes.DateOnly,
        WellKnownTypes.TimeOnly,
#endif
        WellKnownTypes.DateTimeOffset,
        WellKnownTypes.TimeSpan,
        WellKnownTypes.ByteArray,
        WellKnownTypes.Guid
    };

    public static void EnsureAreSqlRowArguments(IEnumerable<SqlExpression> nodes)
    {
      ArgumentValidator.EnsureArgumentNotNull(nodes, "expressions");
      foreach (SqlExpression expression in nodes)
        ArgumentValidator.EnsureArgumentNotNull(expression, "expression");
    }

    public static void EnsureIsBooleanExpression(SqlExpression node)
    {
      if (!IsBooleanExpression(node))
        throw new ArgumentException(Strings.ExInvalidExpressionType);
    }

    public static void EnsureIsCharacterExpression(SqlExpression node)
    {
      if (!IsCharacterExpression(node))
        throw new ArgumentException(Strings.ExInvalidExpressionType);
    }

    public static void EnsureIsRowValueConstructor(SqlExpression node)
    {
      ArgumentValidator.EnsureArgumentNotNull(node, "expression");
      if (!(IsArithmeticExpression(node) || node is SqlRow || node is SqlSubQuery))
        throw new ArgumentException(Strings.ExInvalidExpressionType);
    }

    public static void EnsureIsArithmeticExpression(SqlExpression node)
    {
      if (!IsArithmeticExpression(node))
        throw new ArgumentException(Strings.ExInvalidExpressionType);
    }

    public static void EnsureIsSubSelect(SqlExpression node)
    {
      ArgumentValidator.EnsureArgumentNotNull(node, "expression");
      if (!(node is SqlSubQuery))
        throw new ArgumentException(Strings.ExInvalidExpressionType);
    }

    public static void EnsureIsLimitOffsetArgument(SqlExpression node)
    {
      ArgumentValidator.EnsureArgumentNotNull(node, "node");
      if (!IsLimitOffsetArgument(node))
        throw new InvalidOperationException(Strings.ExOnlySqlLiteralOrSqlPlaceholderCanBeUsedInLimitOffset);
    }

    public static bool IsBooleanExpression(SqlExpression node)
    {
      if (node==null)
        return true;
      switch (node.NodeType) {
        case SqlNodeType.And:
        case SqlNodeType.Between:
        case SqlNodeType.Case:
        case SqlNodeType.Native:
        case SqlNodeType.Column:
        case SqlNodeType.ColumnRef:
        case SqlNodeType.Equals:
        case SqlNodeType.Exists:
        case SqlNodeType.FunctionCall:
        case SqlNodeType.CustomFunctionCall:
        case SqlNodeType.GreaterThan:
        case SqlNodeType.GreaterThanOrEquals:
        case SqlNodeType.In:
        case SqlNodeType.IsNull:
        case SqlNodeType.IsNotNull:
        case SqlNodeType.LessThan:
        case SqlNodeType.LessThanOrEquals:
        case SqlNodeType.Like:
        case SqlNodeType.Not:
        case SqlNodeType.NotBetween:
        case SqlNodeType.NotIn:
        case SqlNodeType.NotEquals:
        case SqlNodeType.Null:
        case SqlNodeType.Match:
        case SqlNodeType.Or:
        case SqlNodeType.Overlaps:
        case SqlNodeType.Parameter:
        case SqlNodeType.SubSelect:
        case SqlNodeType.Unique:
        case SqlNodeType.Variable:
        case SqlNodeType.Placeholder:
        case SqlNodeType.Metadata:
        case SqlNodeType.DynamicFilter:
          return true;
        case SqlNodeType.Cast:
          return ((SqlCast) node).Type.Type==SqlType.Boolean;
        case SqlNodeType.Literal:
          return (node is SqlLiteral<bool>);
        case SqlNodeType.Variant:
          var variant = (SqlVariant) node;
          return IsBooleanExpression(variant.Main) && IsBooleanExpression(variant.Alternative);
        default:
          return false;
      }
    }

    public static bool IsArithmeticExpression(SqlExpression node)
    {
      if (node==null) {
        return true;
      }

      switch (node.NodeType) {
        case SqlNodeType.Add:
        case SqlNodeType.Avg:
        case SqlNodeType.BitAnd:
        case SqlNodeType.BitNot:
        case SqlNodeType.BitOr:
        case SqlNodeType.BitXor:
        case SqlNodeType.Case:
        case SqlNodeType.Cast:
        case SqlNodeType.Column:
        case SqlNodeType.Concat:
        case SqlNodeType.Native:
        case SqlNodeType.Count:
        case SqlNodeType.Divide:
        case SqlNodeType.FunctionCall:
        case SqlNodeType.CustomFunctionCall:
        case SqlNodeType.Literal:
        case SqlNodeType.Max:
        case SqlNodeType.Min:
        case SqlNodeType.Modulo:
        case SqlNodeType.Multiply:
        case SqlNodeType.Negate:
        case SqlNodeType.Null:
        case SqlNodeType.Parameter:
        case SqlNodeType.Subtract:
        case SqlNodeType.SubSelect:
        case SqlNodeType.Sum:
        case SqlNodeType.Variable:
        case SqlNodeType.Extract:
        case SqlNodeType.Round:
        case SqlNodeType.Placeholder:
        case SqlNodeType.Metadata:
        case SqlNodeType.DateTimeMinusInterval:
        case SqlNodeType.DateTimePlusInterval:
        case SqlNodeType.DateTimeMinusDateTime:
        case SqlNodeType.DateTimeOffsetMinusInterval:
        case SqlNodeType.DateTimeOffsetPlusInterval:
        case SqlNodeType.DateTimeOffsetMinusDateTimeOffset:
#if NET6_0_OR_GREATER
        case SqlNodeType.TimePlusInterval:
        case SqlNodeType.TimeMinusTime:
#endif
          return true;
        case SqlNodeType.Variant:
          var variant = (SqlVariant) node;
          return IsArithmeticExpression(variant.Main) && IsArithmeticExpression(variant.Alternative);
        default:
          return false;
      }
    }

    public static bool IsCharacterExpression(SqlExpression node)
    {
      if (node==null)
        return true;
      if (node is SqlBinary)
        return true; // Allow easy operation for SQLite
      switch (node.NodeType) {
        case SqlNodeType.Add:
        case SqlNodeType.Case:
        case SqlNodeType.Cast:
        case SqlNodeType.Column:
        case SqlNodeType.Concat:
        case SqlNodeType.Native:
        case SqlNodeType.RawConcat:
        case SqlNodeType.FunctionCall:
        case SqlNodeType.CustomFunctionCall:
        case SqlNodeType.Literal:
        case SqlNodeType.Null:
        case SqlNodeType.Parameter:
        case SqlNodeType.SubSelect:
        case SqlNodeType.Trim:
        case SqlNodeType.Variable:
        case SqlNodeType.Placeholder:
        case SqlNodeType.Metadata:
          return true;
        case SqlNodeType.Variant:
          var variant = (SqlVariant)node;
          return IsCharacterExpression(variant.Main) && IsCharacterExpression(variant.Alternative);
        default:
          return false;
      }
    }

    public static bool IsLimitOffsetArgument(SqlExpression node)
    {
      switch (node.NodeType) {
      case SqlNodeType.Literal:
      case SqlNodeType.Placeholder:
        return true;
      case SqlNodeType.Variant:
        var variant = (SqlVariant) node;
        return IsLimitOffsetArgument(variant.Main) && IsLimitOffsetArgument(variant.Alternative);
      default:
        return false;
      }
    }

    public static bool IsLiteralTypeSupported(Type type)
    {
      return supportedTypes.Contains(type);
    }    
  }
}

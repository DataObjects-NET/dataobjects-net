// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.

using System;
using System.Collections.Generic;
using Xtensive.Core;
using Xtensive.Sql.Common;
using Xtensive.Sql.Dom.Dml;
using Xtensive.Sql.Dom.Resources;

namespace Xtensive.Sql.Dom
{
  internal static class SqlValidator
  {
    internal static void EnsureAreSqlRowArguments(IEnumerable<SqlExpression> nodes)
    {
      ArgumentValidator.EnsureArgumentNotNull(nodes, "expressions");
      foreach (SqlExpression se in nodes)
        ArgumentValidator.EnsureArgumentNotNull(se, "expression");
    }

    internal static void EnsureIsBooleanExpression(SqlExpression node)
    {
      if (!IsBooleanExpression(node))
        throw new ArgumentException(Strings.ExInvalidExpressionType);
    }

    internal static void EnsureIsCharacterExpression(SqlExpression node)
    {
      if (!IsCharacterExpression(node))
        throw new ArgumentException(Strings.ExInvalidExpressionType);
    }

    internal static void EnsureIsRowValueConstructor(SqlExpression node)
    {
      ArgumentValidator.EnsureArgumentNotNull(node, "expression");
      if (!(IsArithmeticExpression(node) || node is SqlRow || node is SqlSubQuery))
        throw new ArgumentException(Strings.ExInvalidExpressionType);
    }

    internal static void EnsureIsArithmeticExpression(SqlExpression node)
    {
      if (!IsArithmeticExpression(node))
        throw new ArgumentException(Strings.ExInvalidExpressionType);
    }

    internal static void EnsureIsSubSelect(SqlExpression node)
    {
      ArgumentValidator.EnsureArgumentNotNull(node, "expression");
      if (!(node is SqlSubQuery))
        throw new ArgumentException(Strings.ExInvalidExpressionType);
    }

    internal static bool IsBooleanExpression(SqlExpression node)
    {
      if (node==null)
        return true;
      switch (node.NodeType) {
        case SqlNodeType.And:
        case SqlNodeType.Between:
        case SqlNodeType.Case:
        case SqlNodeType.Constant:
        case SqlNodeType.Column:
        case SqlNodeType.ColumnRef:
        case SqlNodeType.Equals:
        case SqlNodeType.Exists:
        case SqlNodeType.FunctionCall:
        case SqlNodeType.GreaterThan:
        case SqlNodeType.GreaterThanOrEquals:
        case SqlNodeType.In:
        case SqlNodeType.IsNull:
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
          return true;
        case SqlNodeType.Cast:
          return ((SqlCast)node).Type.DataType == SqlDataType.Boolean;
        case SqlNodeType.Literal:
          return (node is SqlLiteral<bool>);
        default:
          return false;
      }
    }

    internal static bool IsArithmeticExpression(SqlExpression node)
    {
      if (node==null)
        return true;
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
        case SqlNodeType.Constant:
        case SqlNodeType.Count:
        case SqlNodeType.Divide:
        case SqlNodeType.FunctionCall:
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
          return true;
        default:
          return false;
      }
    }

    internal static bool IsCharacterExpression(SqlExpression node)
    {
      if (node==null)
        return true;
      switch (node.NodeType) {
        case SqlNodeType.Add:
        case SqlNodeType.Case:
        case SqlNodeType.Cast:
        case SqlNodeType.Column:
        case SqlNodeType.Concat:
        case SqlNodeType.FunctionCall:
        case SqlNodeType.Literal:
        case SqlNodeType.Null:
        case SqlNodeType.Parameter:
        case SqlNodeType.SubSelect:
        case SqlNodeType.Trim:
        case SqlNodeType.Variable:
          return true;
        default:
          return false;
      }
    }
  }
}

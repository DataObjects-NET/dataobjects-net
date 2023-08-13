// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.

using System;
using Xtensive.Core;

namespace Xtensive.Sql.Dml
{
  /// <summary>
  /// Represents binary expression.
  /// </summary>
  [Serializable]
  public class SqlBinary : SqlExpression
  {
    /// <summary>
    /// Gets the left operand of the binary operator.
    /// </summary>
    /// <value>The left operand of the binary operator.</value>
    public SqlExpression Left { get; private set; }

    /// <summary>
    /// Gets the right operand of the binary operator.
    /// </summary>
    /// <value>The right operand of the binary operator.</value>
    public SqlExpression Right { get; private set; }

    public static bool operator true(SqlBinary operand)
    {
      switch (operand.NodeType) {
        case SqlNodeType.Equals:
          if (((object)operand.Right)==((object)operand.Left))
            return true;
          else
            return false;
        case SqlNodeType.NotEquals:
          if (((object)operand.Right)!=((object)operand.Left))
            return true;
          else
            return false;
        case SqlNodeType.And:
          if (((SqlBinary)operand.Left ? true : false) && ((SqlBinary)operand.Right ? true : false))
            return true;
          else
            return false;
        case SqlNodeType.Or:
          if (((SqlBinary)operand.Left ? true : false) || ((SqlBinary)operand.Right ? true : false))
            return true;
          else
            return false;
        default:
          return false;
      }
    }

    public static bool operator false(SqlBinary operand)
    {
      switch (operand.NodeType) {
        case SqlNodeType.Equals:
          if (((object)operand.Right)==((object)operand.Left))
            return false;
          else
            return true;
        case SqlNodeType.NotEquals:
          if (((object)operand.Right)!=((object)operand.Left))
            return false;
          else
            return true;
        case SqlNodeType.And:
          if (((SqlBinary)operand.Left ? true : false) && ((SqlBinary)operand.Right ? true : false))
            return false;
          else
            return true;
        case SqlNodeType.Or:
          if (((SqlBinary)operand.Left ? true : false) || ((SqlBinary)operand.Right ? true : false))
            return false;
          else
            return true;
        default:
          return false;
      }
    }

    public override void ReplaceWith(SqlExpression expression)
    {
      var replacingExpression = ArgumentValidator.EnsureArgumentIs<SqlBinary>(expression);
      NodeType = replacingExpression.NodeType;
      Left = replacingExpression.Left;
      Right = replacingExpression.Right;
    }

    internal override SqlBinary Clone(SqlNodeCloneContext context) =>
      context.GetOrAdd(this, static (t, c) =>
        new SqlBinary(t.NodeType,
            t.Left.Clone(c),
            t.Right.Clone(c)));

    public override void AcceptVisitor(ISqlVisitor visitor)
    {
      visitor.Visit(this);
    }

    // Constructors

    internal SqlBinary(SqlNodeType nodeType, SqlExpression left, SqlExpression right) : base(nodeType)
    {
      Left = left;
      Right = right;
    }
  }
}

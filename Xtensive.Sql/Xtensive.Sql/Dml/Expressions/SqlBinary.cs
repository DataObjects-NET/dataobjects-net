// Copyright (C) 2007 Xtensive LLC.
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
    private SqlExpression left;
    private SqlExpression right;

    /// <summary>
    /// Gets the left operand of the binary operator.
    /// </summary>
    /// <value>The left operand of the binary operator.</value>
    public SqlExpression Left {
      get {
        return left;
      }
    }

    /// <summary>
    /// Gets the right operand of the binary operator.
    /// </summary>
    /// <value>The right operand of the binary operator.</value>
    public SqlExpression Right {
      get {
        return right;
      }
    }
    
    /*public static implicit operator SqlBinary(bool t)
    {
      if (t)
        return new SqlBinary(SqlNodeType.True, null, null);
      else
        return new SqlBinary(SqlNodeType.False, null, null);
    }

    public static SqlBinary operator &(SqlBinary left, SqlBinary right)
    {
      if (SqlValidator.IsBool(left))
        return Sql.And(left, right);
      else
        return Sql.BitAnd(left, right);
    }

    public static SqlBinary operator |(SqlBinary left, SqlBinary right)
    {
      if (SqlValidator.IsBool(left))
        return Sql.Or(left, right);
      else
        return Sql.BitOr(left, right);
    }*/

    public static bool operator true(SqlBinary operand)
    {
      switch (operand.NodeType) {
        //case SqlNodeType.True:
        //  return true;
        //case SqlNodeType.False:
        //  return false;
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
        //case SqlNodeType.True:
        //  return false;
        //case SqlNodeType.False:
        //  return true;
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
      ArgumentValidator.EnsureArgumentNotNull(expression, "expression");
      ArgumentValidator.EnsureArgumentIs<SqlBinary>(expression, "expression");
      SqlBinary replacingExpression = expression as SqlBinary;
      NodeType = replacingExpression.NodeType;
      left = replacingExpression.Left;
      right = replacingExpression.Right;
    }

    internal override object Clone(SqlNodeCloneContext context)
    {
      if (context.NodeMapping.ContainsKey(this))
        return context.NodeMapping[this];
      
      SqlBinary clone = new SqlBinary(NodeType, (SqlExpression)left.Clone(context), (SqlExpression)right.Clone(context));
      context.NodeMapping[this] = clone;
      return clone;
    }

    internal SqlBinary(SqlNodeType nodeType, SqlExpression left, SqlExpression right) : base(nodeType)
    {
      this.left = left;
      this.right = right;
    }

    public override void AcceptVisitor(ISqlVisitor visitor)
    {
      visitor.Visit(this);
    }
  }
}

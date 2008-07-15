// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.

using System;
using Xtensive.Core;

namespace Xtensive.Sql.Dom.Dml
{
  /// <summary>
  /// Defines base class for any sql expression.
  /// </summary>
  [Serializable]
  public abstract class SqlExpression : SqlNode
  {
    public static bool IsNull(SqlExpression expression)
    {
      return (object)expression==null;
    }

    public static bool operator true(SqlExpression operand)
    {
      return false;
    }

    public static bool operator false(SqlExpression operand)
    {
      return false;
    }
    
    #region SqlExpression/SqlExpression

    public static SqlExpression operator &(SqlExpression left, SqlExpression right)
    {
      if(IsNull(left))
        return right;
      if (SqlValidator.IsBooleanExpression(left))
        return Sql.And(left, right);
      return Sql.BitAnd(left, right);
    }

    public static SqlExpression operator |(SqlExpression left, SqlExpression right)
    {
      if (IsNull(left))
        return right;
      if (SqlValidator.IsBooleanExpression(left))
        return Sql.Or(left, right);
      return Sql.BitOr(left, right);
    }

    public static SqlExpression operator ^(SqlExpression left, SqlExpression right)
    {
      return Sql.BitXor(left, right);
    }

    public static SqlBinary operator == (SqlExpression left, SqlExpression right)
    {
      return Sql.Binary(SqlNodeType.Equals, left, right);
    }

    public static SqlBinary operator != (SqlExpression left, SqlExpression right)
    {
      return Sql.Binary(SqlNodeType.NotEquals, left, right);
    }
    
    public static SqlBinary operator > (SqlExpression left, SqlExpression right)
    {
      return Sql.GreaterThan(left, right);
    }

    public static SqlBinary operator < (SqlExpression left, SqlExpression right)
    {
      return Sql.LessThan(left, right);
    }

    public static SqlBinary operator >= (SqlExpression left, SqlExpression right)
    {
      return Sql.GreaterThanOrEquals(left, right);
    }

    public static SqlBinary operator <= (SqlExpression left, SqlExpression right)
    {
      return Sql.LessThanOrEquals(left, right);
    }

    public static SqlBinary operator + (SqlExpression left, SqlExpression right)
    {
      return Sql.Add(left, right);
    }

    public static SqlBinary operator - (SqlExpression left, SqlExpression right)
    {
      return Sql.Subtract(left, right);
    }

    public static SqlUnary operator -(SqlExpression operand)
    {
      return Sql.Negate(operand);
    }

    public static SqlBinary operator * (SqlExpression left, SqlExpression right)
    {
      return Sql.Multiply(left, right);
    }

    public static SqlBinary operator / (SqlExpression left, SqlExpression right)
    {
      return Sql.Divide(left, right);
    }

    public static SqlBinary operator % (SqlExpression left, SqlExpression right)
    {
      return Sql.Modulo(left, right);
    }
    
    # endregion

    public static SqlUnary operator !(SqlExpression operand)
    {
      return Sql.Not(operand);
    }
    
    public static SqlUnary operator ~ (SqlExpression operand)
    {
      return Sql.BitNot(operand);
    }

    public static implicit operator SqlExpression(Guid value)
    {
      return new SqlLiteral<Guid>(value);
    }

    public static implicit operator SqlExpression(bool value)
    {
      return new SqlLiteral<bool>(value);
    }

    public static implicit operator SqlExpression(sbyte value)
    {
      return new SqlLiteral<sbyte>(value);
    }

    public static implicit operator SqlExpression(byte value)
    {
      return new SqlLiteral<byte>(value);
    }

    public static implicit operator SqlExpression(byte[] value)
    {
      return new SqlLiteral<byte[]>(value);
    }

    public static implicit operator SqlExpression(short value)
    {
      return new SqlLiteral<short>(value);
    }

    public static implicit operator SqlExpression(ushort value)
    {
      return new SqlLiteral<ushort>(value);
    }

    public static implicit operator SqlExpression(int value)
    {
      return new SqlLiteral<int>(value);
    }

    public static implicit operator SqlExpression(uint value)
    {
      return new SqlLiteral<uint>(value);
    }

    public static implicit operator SqlExpression(long value)
    {
      return new SqlLiteral<long>(value);
    }

    public static implicit operator SqlExpression(ulong value)
    {
      return new SqlLiteral<ulong>(value);
    }

    public static implicit operator SqlExpression(float value)
    {
      return new SqlLiteral<float>(value);
    }

    public static implicit operator SqlExpression(double value)
    {
      return new SqlLiteral<double>(value);
    }

    public static implicit operator SqlExpression(decimal value)
    {
      return new SqlLiteral<decimal>(value);
    }

    public static implicit operator SqlExpression(string value)
    {
      ArgumentValidator.EnsureArgumentNotNull(value, "value");
      return new SqlLiteral<string>(value);
    }

    public static implicit operator SqlExpression(DateTime value)
    {
      return new SqlLiteral<DateTime>(value);
    }

    public static implicit operator SqlExpression(TimeSpan value)
    {
      return new SqlLiteral<TimeSpan>(value);
    }

    public static implicit operator SqlExpression(char value)
    {
      return new SqlLiteral<char>(value);
    }

    public static implicit operator SqlExpression(SqlSelect select)
    {
      return Sql.SubQuery(select);
    }

    public abstract void ReplaceWith(SqlExpression expression);

    // Constructor

    protected SqlExpression(SqlNodeType nodeType) : base(nodeType)
    {
    }
  }
}

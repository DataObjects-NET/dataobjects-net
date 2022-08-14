// Copyright (C) 2008-2020 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.

using System;
using Xtensive.Core;

namespace Xtensive.Sql.Dml
{
  /// <summary>
  /// Defines base class for any sql expression.
  /// </summary>
  [Serializable]
  public abstract class SqlExpression : SqlNode
  {
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
      if (left is null)
        return right;
      if (right is null)
        return left;
      if (SqlValidator.IsBooleanExpression(left))
        return SqlDml.And(left, right);
      return SqlDml.BitAnd(left, right);
    }

    public static SqlExpression operator |(SqlExpression left, SqlExpression right)
    {
      if (left is null)
        return right;
      if (right is null)
        return left;
      if (SqlValidator.IsBooleanExpression(left))
        return SqlDml.Or(left, right);
      return SqlDml.BitOr(left, right);
    }

    public static SqlExpression operator ^(SqlExpression left, SqlExpression right)
    {
      return SqlDml.BitXor(left, right);
    }

    public static SqlBinary operator == (SqlExpression left, SqlExpression right)
    {
      return SqlDml.Binary(SqlNodeType.Equals, left, right);
    }

    public static SqlBinary operator != (SqlExpression left, SqlExpression right)
    {
      return SqlDml.Binary(SqlNodeType.NotEquals, left, right);
    }
    
    public static SqlBinary operator > (SqlExpression left, SqlExpression right)
    {
      return SqlDml.GreaterThan(left, right);
    }

    public static SqlBinary operator < (SqlExpression left, SqlExpression right)
    {
      return SqlDml.LessThan(left, right);
    }

    public static SqlBinary operator >= (SqlExpression left, SqlExpression right)
    {
      return SqlDml.GreaterThanOrEquals(left, right);
    }

    public static SqlBinary operator <= (SqlExpression left, SqlExpression right)
    {
      return SqlDml.LessThanOrEquals(left, right);
    }

    public static SqlBinary operator + (SqlExpression left, SqlExpression right)
    {
      return SqlDml.Add(left, right);
    }

    public static SqlBinary operator - (SqlExpression left, SqlExpression right)
    {
      return SqlDml.Subtract(left, right);
    }

    public static SqlUnary operator -(SqlExpression operand)
    {
      return SqlDml.Negate(operand);
    }

    public static SqlBinary operator * (SqlExpression left, SqlExpression right)
    {
      return SqlDml.Multiply(left, right);
    }

    public static SqlBinary operator / (SqlExpression left, SqlExpression right)
    {
      return SqlDml.Divide(left, right);
    }

    public static SqlBinary operator % (SqlExpression left, SqlExpression right)
    {
      return SqlDml.Modulo(left, right);
    }
    
    # endregion

    public static SqlUnary operator !(SqlExpression operand)
    {
      return SqlDml.Not(operand);
    }
    
    public static SqlUnary operator ~ (SqlExpression operand)
    {
      return SqlDml.BitNot(operand);
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

    public static implicit operator SqlExpression(DateTimeOffset value)
    {
      return new SqlLiteral<DateTimeOffset>(value);
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
      return SqlDml.SubQuery(select);
    }

    public abstract void ReplaceWith(SqlExpression expression);

    public sealed override int GetHashCode() => base.GetHashCode();

    public sealed override bool Equals(object obj) => ReferenceEquals(this, obj);

    // Constructor

    protected SqlExpression(SqlNodeType nodeType) : base(nodeType)
    {
    }
  }
}

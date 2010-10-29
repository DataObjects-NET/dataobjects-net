// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.02.14

using System;
using Xtensive.Linq;
using Xtensive.Sql;
using Xtensive.Sql.Dml;

namespace Xtensive.Storage.Providers.Sql.Expressions
{
  [CompilerContainer(typeof(SqlExpression))]
  internal static class MathCompilers
  {
    #region Math.Abs mapppings

    [Compiler(typeof(Math), "Abs", TargetKind.Static | TargetKind.Method)]
    public static SqlExpression MathAbsSByte(
      [Type(typeof(sbyte))] SqlExpression d)
    {
      return SqlDml.Abs(d);
    }

    [Compiler(typeof(Math), "Abs", TargetKind.Static | TargetKind.Method)]
    public static SqlExpression MathAbsShort(
      [Type(typeof(short))] SqlExpression d)
    {
      return SqlDml.Abs(d);
    }

    [Compiler(typeof(Math), "Abs", TargetKind.Static | TargetKind.Method)]
    public static SqlExpression MathAbsInt(
      [Type(typeof(int))] SqlExpression d)
    {
      return SqlDml.Abs(d);
    }

    [Compiler(typeof(Math), "Abs", TargetKind.Static | TargetKind.Method)]
    public static SqlExpression MathAbsLong(
      [Type(typeof(long))] SqlExpression d)
    {
      return SqlDml.Abs(d);
    }

    [Compiler(typeof(Math), "Abs", TargetKind.Static | TargetKind.Method)]
    public static SqlExpression MathAbsFloat(
      [Type(typeof(float))] SqlExpression d)
    {
      return SqlDml.Abs(d);
    }

    [Compiler(typeof(Math), "Abs", TargetKind.Static | TargetKind.Method)]
    public static SqlExpression MathAbsDouble(
      [Type(typeof(double))] SqlExpression d)
    {
      return SqlDml.Abs(d);
    }

    [Compiler(typeof(Math), "Abs", TargetKind.Static | TargetKind.Method)]
    public static SqlExpression MathAbsDecimal(
      [Type(typeof(decimal))] SqlExpression d)
    {
      return SqlDml.Abs(d);
    }

    #endregion

    #region Math.Sign mappings

    public static SqlExpression GenericSign(SqlExpression target)
    {
      return ExpressionTranslationHelpers.ToInt(SqlDml.Sign(target));
    }

    [Compiler(typeof(Math), "Sign", TargetKind.Static | TargetKind.Method)]
    public static SqlExpression MathSignSByte(
      [Type(typeof(sbyte))] SqlExpression d)
    {
      return GenericSign(d);
    }

    [Compiler(typeof(Math), "Sign", TargetKind.Static | TargetKind.Method)]
    public static SqlExpression MathSignShort(
      [Type(typeof(short))] SqlExpression d)
    {
      return GenericSign(d);
    }

    [Compiler(typeof(Math), "Sign", TargetKind.Static | TargetKind.Method)]
    public static SqlExpression MathSignInt(
      [Type(typeof(int))] SqlExpression d)
    {
      return GenericSign(d);
    }

    [Compiler(typeof(Math), "Sign", TargetKind.Static | TargetKind.Method)]
    public static SqlExpression MathSignLong(
      [Type(typeof(long))] SqlExpression d)
    {
      return GenericSign(d);
    }

    [Compiler(typeof(Math), "Sign", TargetKind.Static | TargetKind.Method)]
    public static SqlExpression MathSignDouble(
      [Type(typeof(double))] SqlExpression d)
    {
      return GenericSign(d);
    }

    [Compiler(typeof(Math), "Sign", TargetKind.Static | TargetKind.Method)]
    public static SqlExpression MathSignDecimal(
      [Type(typeof(decimal))] SqlExpression d)
    {
      return GenericSign(d);
    }

    [Compiler(typeof(Math), "Sign", TargetKind.Static | TargetKind.Method)]
    public static SqlExpression MathSignFloat(
      [Type(typeof(float))] SqlExpression d)
    {
      return GenericSign(d);
    }

    #endregion

    #region Math.Min mappings

    private static SqlExpression Min(SqlExpression left, SqlExpression right)
    {
      var result = SqlDml.Case();
      result.Add(left < right, left);
      result.Else = right;
      return result;
    }

    [Compiler(typeof(Math), "Min", TargetKind.Static | TargetKind.Method)]
    public static SqlExpression MathMinByte(
      [Type(typeof(byte))] SqlExpression left,
      [Type(typeof(byte))] SqlExpression right)
    {
      return Min(left, right);
    }

    [Compiler(typeof(Math), "Min", TargetKind.Static | TargetKind.Method)]
    public static SqlExpression MathMinSByte(
      [Type(typeof(sbyte))] SqlExpression left,
      [Type(typeof(sbyte))] SqlExpression right)
    {
      return Min(left, right);
    }

    [Compiler(typeof(Math), "Min", TargetKind.Static | TargetKind.Method)]
    public static SqlExpression MathMinShort(
      [Type(typeof(short))] SqlExpression left,
      [Type(typeof(short))] SqlExpression right)
    {
      return Min(left, right);
    }

    [Compiler(typeof(Math), "Min", TargetKind.Static | TargetKind.Method)]
    public static SqlExpression MathMinUShort(
      [Type(typeof(ushort))] SqlExpression left,
      [Type(typeof(ushort))] SqlExpression right)
    {
      return Min(left, right);
    }

    [Compiler(typeof(Math), "Min", TargetKind.Static | TargetKind.Method)]
    public static SqlExpression MathMinInt(
      [Type(typeof(int))] SqlExpression left,
      [Type(typeof(int))] SqlExpression right)
    {
      return Min(left, right);
    }

    [Compiler(typeof(Math), "Min", TargetKind.Static | TargetKind.Method)]
    public static SqlExpression MathMinUInt(
      [Type(typeof(uint))] SqlExpression left,
      [Type(typeof(uint))] SqlExpression right)
    {
      return Min(left, right);
    }

    [Compiler(typeof(Math), "Min", TargetKind.Static | TargetKind.Method)]
    public static SqlExpression MathMinLong(
      [Type(typeof(long))] SqlExpression left,
      [Type(typeof(long))] SqlExpression right)
    {
      return Min(left, right);
    }

    [Compiler(typeof(Math), "Min", TargetKind.Static | TargetKind.Method)]
    public static SqlExpression MathMinULong(
      [Type(typeof(ulong))] SqlExpression left,
      [Type(typeof(ulong))] SqlExpression right)
    {
      return Min(left, right);
    }

    [Compiler(typeof(Math), "Min", TargetKind.Static | TargetKind.Method)]
    public static SqlExpression MathMinFloat(
      [Type(typeof(float))] SqlExpression left,
      [Type(typeof(float))] SqlExpression right)
    {
      return Min(left, right);
    }

    [Compiler(typeof(Math), "Min", TargetKind.Static | TargetKind.Method)]
    public static SqlExpression MathMinDouble(
      [Type(typeof(double))] SqlExpression left,
      [Type(typeof(double))] SqlExpression right)
    {
      return Min(left, right);
    }

    [Compiler(typeof(Math), "Min", TargetKind.Static | TargetKind.Method)]
    public static SqlExpression MathMinDecimal(
      [Type(typeof(decimal))] SqlExpression left,
      [Type(typeof(decimal))] SqlExpression right)
    {
      return Min(left, right);
    }

    #endregion

    #region Math.Max mappings

    private static SqlExpression Max(SqlExpression left, SqlExpression right)
    {
      var result = SqlDml.Case();
      result.Add(left > right, left);
      result.Else = right;
      return result;
    }

    [Compiler(typeof(Math), "Max", TargetKind.Static | TargetKind.Method)]
    public static SqlExpression MathMaxByte(
      [Type(typeof(byte))] SqlExpression left,
      [Type(typeof(byte))] SqlExpression right)
    {
      return Max(left, right);
    }

    [Compiler(typeof(Math), "Max", TargetKind.Static | TargetKind.Method)]
    public static SqlExpression MathMaxSByte(
      [Type(typeof(sbyte))] SqlExpression left,
      [Type(typeof(sbyte))] SqlExpression right)
    {
      return Max(left, right);
    }

    [Compiler(typeof(Math), "Max", TargetKind.Static | TargetKind.Method)]
    public static SqlExpression MathMaxShort(
      [Type(typeof(short))] SqlExpression left,
      [Type(typeof(short))] SqlExpression right)
    {
      return Max(left, right);
    }

    [Compiler(typeof(Math), "Max", TargetKind.Static | TargetKind.Method)]
    public static SqlExpression MathMaxUShort(
      [Type(typeof(ushort))] SqlExpression left,
      [Type(typeof(ushort))] SqlExpression right)
    {
      return Max(left, right);
    }

    [Compiler(typeof(Math), "Max", TargetKind.Static | TargetKind.Method)]
    public static SqlExpression MathMaxInt(
      [Type(typeof(int))] SqlExpression left,
      [Type(typeof(int))] SqlExpression right)
    {
      return Max(left, right);
    }

    [Compiler(typeof(Math), "Max", TargetKind.Static | TargetKind.Method)]
    public static SqlExpression MathMaxUInt(
      [Type(typeof(uint))] SqlExpression left,
      [Type(typeof(uint))] SqlExpression right)
    {
      return Max(left, right);
    }

    [Compiler(typeof(Math), "Max", TargetKind.Static | TargetKind.Method)]
    public static SqlExpression MathMaxLong(
      [Type(typeof(long))] SqlExpression left,
      [Type(typeof(long))] SqlExpression right)
    {
      return Max(left, right);
    }

    [Compiler(typeof(Math), "Max", TargetKind.Static | TargetKind.Method)]
    public static SqlExpression MathMaxULong(
      [Type(typeof(ulong))] SqlExpression left,
      [Type(typeof(ulong))] SqlExpression right)
    {
      return Max(left, right);
    }

    [Compiler(typeof(Math), "Max", TargetKind.Static | TargetKind.Method)]
    public static SqlExpression MathMaxFloat(
      [Type(typeof(float))] SqlExpression left,
      [Type(typeof(float))] SqlExpression right)
    {
      return Max(left, right);
    }

    [Compiler(typeof(Math), "Max", TargetKind.Static | TargetKind.Method)]
    public static SqlExpression MathMaxDouble(
      [Type(typeof(double))] SqlExpression left,
      [Type(typeof(double))] SqlExpression right)
    {
      return Max(left, right);
    }

    [Compiler(typeof(Math), "Max", TargetKind.Static | TargetKind.Method)]
    public static SqlExpression MathMaxDecimal(
      [Type(typeof(decimal))] SqlExpression left,
      [Type(typeof(decimal))] SqlExpression right)
    {
      return Max(left, right);
    }

    #endregion

    [Compiler(typeof(Math), "Acos", TargetKind.Static | TargetKind.Method)]
    public static SqlExpression MathAcos(
      [Type(typeof(double))] SqlExpression d)
    {
      return SqlDml.Acos(d);
    }

    [Compiler(typeof(Math), "Asin", TargetKind.Static | TargetKind.Method)]
    public static SqlExpression MathAsin(
      [Type(typeof(double))] SqlExpression d)
    {
      return SqlDml.Asin(d);
    }

    [Compiler(typeof(Math), "Atan", TargetKind.Static | TargetKind.Method)]
    public static SqlExpression MathAtan(
      [Type(typeof(double))] SqlExpression d)
    {
      return SqlDml.Atan(d);
    }

    [Compiler(typeof(Math), "Atan2", TargetKind.Static | TargetKind.Method)]
    public static SqlExpression MathAtan2(
      [Type(typeof(double))] SqlExpression y,
      [Type(typeof(double))] SqlExpression x)
    {
      return SqlDml.Atan2(y, x);
    }

    [Compiler(typeof(Math), "BigMul", TargetKind.Static | TargetKind.Method)]
    public static SqlExpression MathBigMul(
      [Type(typeof(int))] SqlExpression a,
      [Type(typeof(int))] SqlExpression b)
    {
      return ExpressionTranslationHelpers.ToLong(a) * b;
    }

    [Compiler(typeof(Math), "Ceiling", TargetKind.Static | TargetKind.Method)]
    public static SqlExpression MathCeilingDouble(
      [Type(typeof(double))] SqlExpression d)
    {
      return SqlDml.Ceiling(d);
    }

    [Compiler(typeof(Math), "Ceiling", TargetKind.Static | TargetKind.Method)]
    public static SqlExpression MathCeilingDecimal(
      [Type(typeof(decimal))] SqlExpression d)
    {
      return SqlDml.Ceiling(d);
    }

    [Compiler(typeof(Math), "Cos", TargetKind.Static | TargetKind.Method)]
    public static SqlExpression MathCos(
      [Type(typeof(double))] SqlExpression d)
    {
      return SqlDml.Cos(d);
    }

    [Compiler(typeof(Math), "Cosh", TargetKind.Static | TargetKind.Method)]
    public static SqlExpression MathCosh(
      [Type(typeof(double))] SqlExpression d)
    {
      return (SqlDml.Exp(d) + SqlDml.Exp(-d)) / SqlDml.Literal(2);
    }

    [Compiler(typeof(Math), "Exp", TargetKind.Static | TargetKind.Method)]
    public static SqlExpression MathExp(
      [Type(typeof(double))] SqlExpression d)
    {
      return SqlDml.Exp(d);
    }

    [Compiler(typeof(Math), "Floor", TargetKind.Static | TargetKind.Method)]
    public static SqlExpression MathFloorDouble(
      [Type(typeof(double))] SqlExpression d)
    {
      return SqlDml.Floor(d);
    }

    [Compiler(typeof(Math), "Floor", TargetKind.Static | TargetKind.Method)]
    public static SqlExpression MathFloorDecimal(
      [Type(typeof(decimal))] SqlExpression d)
    {
      return SqlDml.Floor(d);
    }

    [Compiler(typeof(Math), "Log", TargetKind.Static | TargetKind.Method)]
    public static SqlExpression MathLog(
      [Type(typeof(double))] SqlExpression d)
    {
      return SqlDml.Log(d);
    }

    [Compiler(typeof(Math), "Log", TargetKind.Static | TargetKind.Method)]
    public static SqlExpression MathLog(
      [Type(typeof(double))] SqlExpression d,
      [Type(typeof(double))] SqlExpression newBase)
    {
      return SqlDml.Log(d) / SqlDml.Log(newBase);
    }

    [Compiler(typeof(Math), "Log10", TargetKind.Static | TargetKind.Method)]
    public static SqlExpression MathLog10(
      [Type(typeof(double))] SqlExpression d)
    {
      return SqlDml.Log10(d);
    }

    [Compiler(typeof(Math), "PI", TargetKind.Static | TargetKind.Field)]
    public static SqlExpression MathPI()
    {
      return SqlDml.Pi();
    }

    [Compiler(typeof(Math), "Pow", TargetKind.Static | TargetKind.Method)]
    public static SqlExpression MathPow(
      [Type(typeof(double))] SqlExpression x,
      [Type(typeof(double))] SqlExpression y)
    {
      return SqlDml.Power(x, y);
    }

    [Compiler(typeof(Math), "Round", TargetKind.Static | TargetKind.Method)]
    public static SqlExpression MathRoundDouble(
      [Type(typeof(double))] SqlExpression d)
    {
      return BankersRound(d, null, false);
    }

    [Compiler(typeof(Math), "Round", TargetKind.Static | TargetKind.Method)]
    public static SqlExpression MathRoundDouble(
      [Type(typeof(double))] SqlExpression d,
      [Type(typeof(int))] SqlExpression digits)
    {
      return BankersRound(d, digits, false);
    }

    [Compiler(typeof(Math), "Round", TargetKind.Static | TargetKind.Method)]
    public static SqlExpression MathRoundDoubleWithMode(
      [Type(typeof(double))] SqlExpression d,
      [Type(typeof(MidpointRounding))] SqlExpression mode)
    {
      return GenericRound(d, null, false, mode);
    }

    [Compiler(typeof(Math), "Round", TargetKind.Static | TargetKind.Method)]
    public static SqlExpression MathRoundDoubleWithMode(
      [Type(typeof(double))] SqlExpression d,
      [Type(typeof(int))] SqlExpression digits,
      [Type(typeof(MidpointRounding))] SqlExpression mode)
    {
      return GenericRound(d, digits, false, mode);
    }

    [Compiler(typeof(Math), "Round", TargetKind.Static | TargetKind.Method)]
    public static SqlExpression MathRoundDecimal(
      [Type(typeof(decimal))] SqlExpression d)
    {
      return BankersRound(d, null, true);
    }

    [Compiler(typeof(Math), "Round", TargetKind.Static | TargetKind.Method)]
    public static SqlExpression MathRoundDecimal(
      [Type(typeof(decimal))] SqlExpression d,
      [Type(typeof(int))] SqlExpression decimals)
    {
      return BankersRound(d, decimals, true);
    }

    [Compiler(typeof(Math), "Round", TargetKind.Static | TargetKind.Method)]
    public static SqlExpression MathRoundDecimalWithMode(
      [Type(typeof(decimal))] SqlExpression d,
      [Type(typeof(MidpointRounding))] SqlExpression mode)
    {
      return GenericRound(d, null, true, mode);
    }

    [Compiler(typeof(Math), "Round", TargetKind.Static | TargetKind.Method)]
    public static SqlExpression MathRoundDecimalWithMode(
      [Type(typeof(decimal))] SqlExpression d,
      [Type(typeof(int))] SqlExpression decimals,
      [Type(typeof(MidpointRounding))] SqlExpression mode)
    {
      return GenericRound(d, decimals, true, mode);
    }

    [Compiler(typeof(Math), "Sin", TargetKind.Static | TargetKind.Method)]
    public static SqlExpression MathSin(
      [Type(typeof(double))] SqlExpression d)
    {
      return SqlDml.Sin(d);
    }

    [Compiler(typeof(Math), "Sinh", TargetKind.Static | TargetKind.Method)]
    public static SqlExpression MathSinh(
      [Type(typeof(double))] SqlExpression d)
    {
      return (SqlDml.Exp(d) - SqlDml.Exp(-d)) / SqlDml.Literal(2);
    }

    [Compiler(typeof(Math), "Sqrt", TargetKind.Static | TargetKind.Method)]
    public static SqlExpression MathSqrt(
      [Type(typeof(double))] SqlExpression d)
    {
      return SqlDml.Sqrt(d);
    }

    [Compiler(typeof(Math), "Tan", TargetKind.Static | TargetKind.Method)]
    public static SqlExpression MathTan(
      [Type(typeof(double))] SqlExpression d)
    {
      return SqlDml.Tan(d);
    }

    [Compiler(typeof(Math), "Tanh", TargetKind.Static | TargetKind.Method)]
    public static SqlExpression MathTanh(
      [Type(typeof(double))] SqlExpression d)
    {
      var exp2d = SqlDml.Exp(SqlDml.Literal(2) * d);
      var one = SqlDml.Literal(1);

      return (exp2d - one) / (exp2d + one);
    }

    [Compiler(typeof(Math), "Truncate", TargetKind.Static | TargetKind.Method)]
    public static SqlExpression MathTruncateDouble(
      [Type(typeof(double))] SqlExpression d)
    {
      return SqlDml.Truncate(d);
    }

    [Compiler(typeof(Math), "Truncate", TargetKind.Static | TargetKind.Method)]
    public static SqlExpression MathTruncateDecimal(
      [Type(typeof(decimal))] SqlExpression d)
    {
      return SqlDml.Truncate(d);
    }

    #region Round helpers

    private static SqlExpression GenericRound(SqlExpression value, SqlExpression digits, bool isDecimal, SqlExpression mode)
    {
      if (mode.NodeType!=SqlNodeType.Container)
        throw new NotSupportedException();
      var container = (SqlContainer) mode;
      if (!(container.Value is MidpointRounding))
        throw new NotSupportedException();
      return SqlDml.Round(value, digits,
        isDecimal ? TypeCode.Decimal : TypeCode.Double,
        (MidpointRounding) container.Value);
    }

    private static SqlExpression BankersRound(SqlExpression value, SqlExpression digits, bool isDecimal)
    {
      return SqlDml.Round(value, digits, isDecimal ? TypeCode.Decimal : TypeCode.Double, MidpointRounding.ToEven);
    }

    #endregion
  }
}
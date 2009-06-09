// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.02.14

using System;
using Xtensive.Core.Linq;
using Xtensive.Sql.Common;
using Xtensive.Sql.Dom;
using Xtensive.Sql.Dom.Dml;
using SqlFactory = Xtensive.Sql.Dom.Sql;

namespace Xtensive.Storage.Providers.Sql.Mappings.FunctionMappings
{
  [CompilerContainer(typeof(SqlExpression))]
  internal static class MathMappings
  {
    #region Math.Abs mapppings

    [Compiler(typeof(Math), "Abs", TargetKind.Static | TargetKind.Method)]
    public static SqlExpression MathAbsSByte(
      [Type(typeof(sbyte))] SqlExpression d)
    {
      return SqlFactory.Abs(d);
    }

    [Compiler(typeof(Math), "Abs", TargetKind.Static | TargetKind.Method)]
    public static SqlExpression MathAbsShort(
      [Type(typeof(short))] SqlExpression d)
    {
      return SqlFactory.Abs(d);
    }

    [Compiler(typeof(Math), "Abs", TargetKind.Static | TargetKind.Method)]
    public static SqlExpression MathAbsInt(
      [Type(typeof(int))] SqlExpression d)
    {
      return SqlFactory.Abs(d);
    }

    [Compiler(typeof(Math), "Abs", TargetKind.Static | TargetKind.Method)]
    public static SqlExpression MathAbsLong(
      [Type(typeof(long))] SqlExpression d)
    {
      return SqlFactory.Abs(d);
    }

    [Compiler(typeof(Math), "Abs", TargetKind.Static | TargetKind.Method)]
    public static SqlExpression MathAbsFloat(
      [Type(typeof(float))] SqlExpression d)
    {
      return SqlFactory.Abs(d);
    }

    [Compiler(typeof(Math), "Abs", TargetKind.Static | TargetKind.Method)]
    public static SqlExpression MathAbsDouble(
      [Type(typeof(double))] SqlExpression d)
    {
      return SqlFactory.Abs(d);
    }

    [Compiler(typeof(Math), "Abs", TargetKind.Static | TargetKind.Method)]
    public static SqlExpression MathAbsDecimal(
      [Type(typeof(decimal))] SqlExpression d)
    {
      return SqlFactory.Abs(d);
    }

    #endregion

    #region Math.Sign mappings

    private static SqlExpression Sign(SqlExpression target)
    {
      return SqlFactory.Cast(SqlFactory.Sign(target), SqlDataType.Int32);
    }

    [Compiler(typeof(Math), "Sign", TargetKind.Static | TargetKind.Method)]
    public static SqlExpression MathSignSByte(
      [Type(typeof(sbyte))] SqlExpression d)
    {
      return Sign(d);
    }

    [Compiler(typeof(Math), "Sign", TargetKind.Static | TargetKind.Method)]
    public static SqlExpression MathSignShort(
      [Type(typeof(short))] SqlExpression d)
    {
      return Sign(d);
    }

    [Compiler(typeof(Math), "Sign", TargetKind.Static | TargetKind.Method)]
    public static SqlExpression MathSignInt(
      [Type(typeof(int))] SqlExpression d)
    {
      return Sign(d);
    }

    [Compiler(typeof(Math), "Sign", TargetKind.Static | TargetKind.Method)]
    public static SqlExpression MathSignLong(
      [Type(typeof(long))] SqlExpression d)
    {
      return Sign(d);
    }

    [Compiler(typeof(Math), "Sign", TargetKind.Static | TargetKind.Method)]
    public static SqlExpression MathSignDouble(
      [Type(typeof(double))] SqlExpression d)
    {
      return Sign(d);
    }

    [Compiler(typeof(Math), "Sign", TargetKind.Static | TargetKind.Method)]
    public static SqlExpression MathSignDecimal(
      [Type(typeof(decimal))] SqlExpression d)
    {
      return Sign(d);
    }

    [Compiler(typeof(Math), "Sign", TargetKind.Static | TargetKind.Method)]
    public static SqlExpression MathSignFloat(
      [Type(typeof(float))] SqlExpression d)
    {
      return Sign(d);
    }

    #endregion

    #region Math.Min mappings

    private static SqlExpression Min(SqlExpression left, SqlExpression right)
    {
      var result = SqlFactory.Case();
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
      var result = SqlFactory.Case();
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
      return SqlFactory.Acos(d);
    }

    [Compiler(typeof(Math), "Asin", TargetKind.Static | TargetKind.Method)]
    public static SqlExpression MathAsin(
      [Type(typeof(double))] SqlExpression d)
    {
      return SqlFactory.Asin(d);
    }

    [Compiler(typeof(Math), "Atan", TargetKind.Static | TargetKind.Method)]
    public static SqlExpression MathAtan(
      [Type(typeof(double))] SqlExpression d)
    {
      return SqlFactory.Atan(d);
    }

    [Compiler(typeof(Math), "Atan2", TargetKind.Static | TargetKind.Method)]
    public static SqlExpression MathAtan2(
      [Type(typeof(double))] SqlExpression y,
      [Type(typeof(double))] SqlExpression x)
    {
      return SqlFactory.Atan2(y, x);
    }

    [Compiler(typeof(Math), "BigMul", TargetKind.Static | TargetKind.Method)]
    public static SqlExpression MathBigMul(
      [Type(typeof(int))] SqlExpression a,
      [Type(typeof(int))] SqlExpression b)
    {
      return SqlFactory.Cast(a, SqlDataType.Int64) * b;
    }

    [Compiler(typeof(Math), "Ceiling", TargetKind.Static | TargetKind.Method)]
    public static SqlExpression MathCeilingDouble(
      [Type(typeof(double))] SqlExpression d)
    {
      return SqlFactory.Ceiling(d);
    }

    [Compiler(typeof(Math), "Ceiling", TargetKind.Static | TargetKind.Method)]
    public static SqlExpression MathCeilingDecimal(
      [Type(typeof(decimal))] SqlExpression d)
    {
      return SqlFactory.Ceiling(d);
    }

    [Compiler(typeof(Math), "Cos", TargetKind.Static | TargetKind.Method)]
    public static SqlExpression MathCos(
      [Type(typeof(double))] SqlExpression d)
    {
      return SqlFactory.Cos(d);
    }

    [Compiler(typeof(Math), "Cosh", TargetKind.Static | TargetKind.Method)]
    public static SqlExpression MathCosh(
      [Type(typeof(double))] SqlExpression d)
    {
      return (SqlFactory.Exp(d) + SqlFactory.Exp(-d)) / SqlFactory.Literal(2);
    }

    [Compiler(typeof(Math), "Exp", TargetKind.Static | TargetKind.Method)]
    public static SqlExpression MathExp(
      [Type(typeof(double))] SqlExpression d)
    {
      return SqlFactory.Exp(d);
    }

    [Compiler(typeof(Math), "Floor", TargetKind.Static | TargetKind.Method)]
    public static SqlExpression MathFloorDouble(
      [Type(typeof(double))] SqlExpression d)
    {
      return SqlFactory.Floor(d);
    }

    [Compiler(typeof(Math), "Floor", TargetKind.Static | TargetKind.Method)]
    public static SqlExpression MathFloorDecimal(
      [Type(typeof(decimal))] SqlExpression d)
    {
      return SqlFactory.Floor(d);
    }

    [Compiler(typeof(Math), "Log", TargetKind.Static | TargetKind.Method)]
    public static SqlExpression MathLog(
      [Type(typeof(double))] SqlExpression d)
    {
      return SqlFactory.Log(d);
    }

    [Compiler(typeof(Math), "Log", TargetKind.Static | TargetKind.Method)]
    public static SqlExpression MathLog(
      [Type(typeof(double))] SqlExpression d,
      [Type(typeof(double))] SqlExpression newBase)
    {
      return SqlFactory.Log(d) / SqlFactory.Log(newBase);
    }

    [Compiler(typeof(Math), "Log10", TargetKind.Static | TargetKind.Method)]
    public static SqlExpression MathLog10(
      [Type(typeof(double))] SqlExpression d)
    {
      return SqlFactory.Log10(d);
    }

    [Compiler(typeof(Math), "PI", TargetKind.Static | TargetKind.Field)]
    public static SqlExpression MathPI()
    {
      return SqlFactory.Pi();
    }

    [Compiler(typeof(Math), "Pow", TargetKind.Static | TargetKind.Method)]
    public static SqlExpression MathPow(
      [Type(typeof(double))] SqlExpression x,
      [Type(typeof(double))] SqlExpression y)
    {
      return SqlFactory.Power(x, y);
    }

    [Compiler(typeof(Math), "Round", TargetKind.Static | TargetKind.Method)]
    public static SqlExpression MathRoundDouble(
      [Type(typeof(double))] SqlExpression d)
    {
      return SqlFactory.Round(d);
    }

    [Compiler(typeof(Math), "Round", TargetKind.Static | TargetKind.Method)]
    public static SqlExpression MathRoundDouble(
      [Type(typeof(double))] SqlExpression d,
      [Type(typeof(int))] SqlExpression digits)
    {
      return SqlFactory.Round(d, digits);
    }

    [Compiler(typeof(Math), "Round", TargetKind.Static | TargetKind.Method)]
    public static SqlExpression MathRoundDecimal(
      [Type(typeof(decimal))] SqlExpression d)
    {
      return SqlFactory.Round(d);
    }

    [Compiler(typeof(Math), "Round", TargetKind.Static | TargetKind.Method)]
    public static SqlExpression MathRoundDecimal(
      [Type(typeof(decimal))] SqlExpression d,
      [Type(typeof(int))] SqlExpression decimals)
    {
      return SqlFactory.Round(d, decimals);
    }

    [Compiler(typeof(Math), "Sin", TargetKind.Static | TargetKind.Method)]
    public static SqlExpression MathSin(
      [Type(typeof(double))] SqlExpression d)
    {
      return SqlFactory.Sin(d);
    }

    [Compiler(typeof(Math), "Sinh", TargetKind.Static | TargetKind.Method)]
    public static SqlExpression MathSinh(
      [Type(typeof(double))] SqlExpression d)
    {
      return (SqlFactory.Exp(d) - SqlFactory.Exp(-d)) / SqlFactory.Literal(2);
    }

    [Compiler(typeof(Math), "Sqrt", TargetKind.Static | TargetKind.Method)]
    public static SqlExpression MathSqrt(
      [Type(typeof(double))] SqlExpression d)
    {
      return SqlFactory.Sqrt(d);
    }

    [Compiler(typeof(Math), "Tan", TargetKind.Static | TargetKind.Method)]
    public static SqlExpression MathTan(
      [Type(typeof(double))] SqlExpression d)
    {
      return SqlFactory.Tan(d);
    }

    [Compiler(typeof(Math), "Tanh", TargetKind.Static | TargetKind.Method)]
    public static SqlExpression MathTanh(
      [Type(typeof(double))] SqlExpression d)
    {
      var exp2d = SqlFactory.Exp(SqlFactory.Literal(2) * d);
      var one = SqlFactory.Literal(1);

      return (exp2d - one) / (exp2d + one);
    }

    [Compiler(typeof(Math), "Truncate", TargetKind.Static | TargetKind.Method)]
    public static SqlExpression MathTruncateDouble(
      [Type(typeof(double))] SqlExpression d)
    {
      return SqlFactory.Truncate(d);
    }

    [Compiler(typeof(Math), "Truncate", TargetKind.Static | TargetKind.Method)]
    public static SqlExpression MathTruncateDecimal(
      [Type(typeof(decimal))] SqlExpression d)
    {
      return SqlFactory.Truncate(d);
    }

  }
}

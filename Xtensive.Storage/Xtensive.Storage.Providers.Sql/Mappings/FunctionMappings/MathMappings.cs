// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.02.14

using System;
using Xtensive.Core.Linq;
using Xtensive.Sql.Dom.Dml;
using SqlFactory = Xtensive.Sql.Dom.Sql;

namespace Xtensive.Storage.Providers.Sql.Mappings.FunctionMappings
{
  internal static class MathMappings
  {
    #region Math.Abs mapppings

    [Compiler(typeof(Math), "Abs", TargetKind.Static | TargetKind.Method)]
    public static SqlExpression MathAbsSByte(
      [ParamType(typeof(sbyte))] SqlExpression d)
    {
      return SqlFactory.Abs(d);
    }

    [Compiler(typeof(Math), "Abs", TargetKind.Static | TargetKind.Method)]
    public static SqlExpression MathAbsShort(
      [ParamType(typeof(short))] SqlExpression d)
    {
      return SqlFactory.Abs(d);
    }

    [Compiler(typeof(Math), "Abs", TargetKind.Static | TargetKind.Method)]
    public static SqlExpression MathAbsInt(
      [ParamType(typeof(int))] SqlExpression d)
    {
      return SqlFactory.Abs(d);
    }

    [Compiler(typeof(Math), "Abs", TargetKind.Static | TargetKind.Method)]
    public static SqlExpression MathAbsLong(
      [ParamType(typeof(long))] SqlExpression d)
    {
      return SqlFactory.Abs(d);
    }

    [Compiler(typeof(Math), "Abs", TargetKind.Static | TargetKind.Method)]
    public static SqlExpression MathAbsFloat(
      [ParamType(typeof(float))] SqlExpression d)
    {
      return SqlFactory.Abs(d);
    }

    [Compiler(typeof(Math), "Abs", TargetKind.Static | TargetKind.Method)]
    public static SqlExpression MathAbsDouble(
      [ParamType(typeof(double))] SqlExpression d)
    {
      return SqlFactory.Abs(d);
    }

    [Compiler(typeof(Math), "Abs", TargetKind.Static | TargetKind.Method)]
    public static SqlExpression MathAbsDecimal(
      [ParamType(typeof(decimal))] SqlExpression d)
    {
      return SqlFactory.Abs(d);
    }

    #endregion

    #region Math.Sign mappings

    [Compiler(typeof(Math), "Sign", TargetKind.Static | TargetKind.Method)]
    public static SqlExpression MathSignSByte(
      [ParamType(typeof(sbyte))] SqlExpression d)
    {
      return SqlFactory.Sign(d);
    }

    [Compiler(typeof(Math), "Sign", TargetKind.Static | TargetKind.Method)]
    public static SqlExpression MathSignShort(
      [ParamType(typeof(short))] SqlExpression d)
    {
      return SqlFactory.Sign(d);
    }

    [Compiler(typeof(Math), "Sign", TargetKind.Static | TargetKind.Method)]
    public static SqlExpression MathSignInt(
      [ParamType(typeof(int))] SqlExpression d)
    {
      return SqlFactory.Sign(d);
    }

    [Compiler(typeof(Math), "Sign", TargetKind.Static | TargetKind.Method)]
    public static SqlExpression MathSignLong(
      [ParamType(typeof(long))] SqlExpression d)
    {
      return SqlFactory.Sign(d);
    }

    [Compiler(typeof(Math), "Sign", TargetKind.Static | TargetKind.Method)]
    public static SqlExpression MathSignDouble(
      [ParamType(typeof(double))] SqlExpression d)
    {
      return SqlFactory.Sign(d);
    }

    [Compiler(typeof(Math), "Sign", TargetKind.Static | TargetKind.Method)]
    public static SqlExpression MathSignDecimal(
      [ParamType(typeof(decimal))] SqlExpression d)
    {
      return SqlFactory.Sign(d);
    }

    #endregion

    #region Math.Min mappings

    private static SqlExpression MinHelper(SqlExpression left, SqlExpression right)
    {
      var result = SqlFactory.Case();
      result.Add(left < right, left);
      result.Else = right;
      return result;
    }

    [Compiler(typeof(Math), "Min", TargetKind.Static | TargetKind.Method)]
    public static SqlExpression MathMinByte(
      [ParamType(typeof(byte))] SqlExpression left,
      [ParamType(typeof(byte))] SqlExpression right)
    {
      return MinHelper(left, right);
    }

    [Compiler(typeof(Math), "Min", TargetKind.Static | TargetKind.Method)]
    public static SqlExpression MathMinSByte(
      [ParamType(typeof(sbyte))] SqlExpression left,
      [ParamType(typeof(sbyte))] SqlExpression right)
    {
      return MinHelper(left, right);
    }

    [Compiler(typeof(Math), "Min", TargetKind.Static | TargetKind.Method)]
    public static SqlExpression MathMinShort(
      [ParamType(typeof(short))] SqlExpression left,
      [ParamType(typeof(short))] SqlExpression right)
    {
      return MinHelper(left, right);
    }

    [Compiler(typeof(Math), "Min", TargetKind.Static | TargetKind.Method)]
    public static SqlExpression MathMinUShort(
      [ParamType(typeof(ushort))] SqlExpression left,
      [ParamType(typeof(ushort))] SqlExpression right)
    {
      return MinHelper(left, right);
    }

    [Compiler(typeof(Math), "Min", TargetKind.Static | TargetKind.Method)]
    public static SqlExpression MathMinInt(
      [ParamType(typeof(int))] SqlExpression left,
      [ParamType(typeof(int))] SqlExpression right)
    {
      return MinHelper(left, right);
    }

    [Compiler(typeof(Math), "Min", TargetKind.Static | TargetKind.Method)]
    public static SqlExpression MathMinUInt(
      [ParamType(typeof(uint))] SqlExpression left,
      [ParamType(typeof(uint))] SqlExpression right)
    {
      return MinHelper(left, right);
    }

    [Compiler(typeof(Math), "Min", TargetKind.Static | TargetKind.Method)]
    public static SqlExpression MathMinLong(
      [ParamType(typeof(long))] SqlExpression left,
      [ParamType(typeof(long))] SqlExpression right)
    {
      return MinHelper(left, right);
    }

    [Compiler(typeof(Math), "Min", TargetKind.Static | TargetKind.Method)]
    public static SqlExpression MathMinULong(
      [ParamType(typeof(ulong))] SqlExpression left,
      [ParamType(typeof(ulong))] SqlExpression right)
    {
      return MinHelper(left, right);
    }

    [Compiler(typeof(Math), "Min", TargetKind.Static | TargetKind.Method)]
    public static SqlExpression MathMinFloat(
      [ParamType(typeof(float))] SqlExpression left,
      [ParamType(typeof(float))] SqlExpression right)
    {
      return MinHelper(left, right);
    }

    [Compiler(typeof(Math), "Min", TargetKind.Static | TargetKind.Method)]
    public static SqlExpression MathMinDouble(
      [ParamType(typeof(double))] SqlExpression left,
      [ParamType(typeof(double))] SqlExpression right)
    {
      return MinHelper(left, right);
    }

    [Compiler(typeof(Math), "Min", TargetKind.Static | TargetKind.Method)]
    public static SqlExpression MathMinDecimal(
      [ParamType(typeof(decimal))] SqlExpression left,
      [ParamType(typeof(decimal))] SqlExpression right)
    {
      return MinHelper(left, right);
    }

    #endregion

    #region Math.Max mappings

    private static SqlExpression MaxHelper(SqlExpression left, SqlExpression right)
    {
      var result = SqlFactory.Case();
      result.Add(left > right, left);
      result.Else = right;
      return result;
    }

    [Compiler(typeof(Math), "Max", TargetKind.Static | TargetKind.Method)]
    public static SqlExpression MathMaxByte(
      [ParamType(typeof(byte))] SqlExpression left,
      [ParamType(typeof(byte))] SqlExpression right)
    {
      return MaxHelper(left, right);
    }

    [Compiler(typeof(Math), "Max", TargetKind.Static | TargetKind.Method)]
    public static SqlExpression MathMaxSByte(
      [ParamType(typeof(sbyte))] SqlExpression left,
      [ParamType(typeof(sbyte))] SqlExpression right)
    {
      return MaxHelper(left, right);
    }

    [Compiler(typeof(Math), "Max", TargetKind.Static | TargetKind.Method)]
    public static SqlExpression MathMaxShort(
      [ParamType(typeof(short))] SqlExpression left,
      [ParamType(typeof(short))] SqlExpression right)
    {
      return MaxHelper(left, right);
    }

    [Compiler(typeof(Math), "Max", TargetKind.Static | TargetKind.Method)]
    public static SqlExpression MathMaxUShort(
      [ParamType(typeof(ushort))] SqlExpression left,
      [ParamType(typeof(ushort))] SqlExpression right)
    {
      return MaxHelper(left, right);
    }

    [Compiler(typeof(Math), "Max", TargetKind.Static | TargetKind.Method)]
    public static SqlExpression MathMaxInt(
      [ParamType(typeof(int))] SqlExpression left,
      [ParamType(typeof(int))] SqlExpression right)
    {
      return MaxHelper(left, right);
    }

    [Compiler(typeof(Math), "Max", TargetKind.Static | TargetKind.Method)]
    public static SqlExpression MathMaxUInt(
      [ParamType(typeof(uint))] SqlExpression left,
      [ParamType(typeof(uint))] SqlExpression right)
    {
      return MaxHelper(left, right);
    }

    [Compiler(typeof(Math), "Max", TargetKind.Static | TargetKind.Method)]
    public static SqlExpression MathMaxLong(
      [ParamType(typeof(long))] SqlExpression left,
      [ParamType(typeof(long))] SqlExpression right)
    {
      return MaxHelper(left, right);
    }

    [Compiler(typeof(Math), "Max", TargetKind.Static | TargetKind.Method)]
    public static SqlExpression MathMaxULong(
      [ParamType(typeof(ulong))] SqlExpression left,
      [ParamType(typeof(ulong))] SqlExpression right)
    {
      return MaxHelper(left, right);
    }

    [Compiler(typeof(Math), "Max", TargetKind.Static | TargetKind.Method)]
    public static SqlExpression MathMaxFloat(
      [ParamType(typeof(float))] SqlExpression left,
      [ParamType(typeof(float))] SqlExpression right)
    {
      return MaxHelper(left, right);
    }

    [Compiler(typeof(Math), "Max", TargetKind.Static | TargetKind.Method)]
    public static SqlExpression MathMaxDouble(
      [ParamType(typeof(double))] SqlExpression left,
      [ParamType(typeof(double))] SqlExpression right)
    {
      return MaxHelper(left, right);
    }

    [Compiler(typeof(Math), "Max", TargetKind.Static | TargetKind.Method)]
    public static SqlExpression MathMaxDecimal(
      [ParamType(typeof(decimal))] SqlExpression left,
      [ParamType(typeof(decimal))] SqlExpression right)
    {
      return MaxHelper(left, right);
    }

    #endregion

    [Compiler(typeof(Math), "Acos", TargetKind.Static | TargetKind.Method)]
    public static SqlExpression MathAcos(
      [ParamType(typeof(double))] SqlExpression d)
    {
      return SqlFactory.Acos(d);
    }

    [Compiler(typeof(Math), "Asin", TargetKind.Static | TargetKind.Method)]
    public static SqlExpression MathAsin(
      [ParamType(typeof(double))] SqlExpression d)
    {
      return SqlFactory.Asin(d);
    }

    [Compiler(typeof(Math), "Atan", TargetKind.Static | TargetKind.Method)]
    public static SqlExpression MathAtan(
      [ParamType(typeof(double))] SqlExpression d)
    {
      return SqlFactory.Atan(d);
    }

    [Compiler(typeof(Math), "Atan2", TargetKind.Static | TargetKind.Method)]
    public static SqlExpression MathAtan2(
      [ParamType(typeof(double))] SqlExpression y,
      [ParamType(typeof(double))] SqlExpression x)
    {
      // todo: remember matan (-:
      throw new NotImplementedException();
    }

    [Compiler(typeof(Math), "BigMul", TargetKind.Static | TargetKind.Method)]
    public static SqlExpression MathBigMul(
      [ParamType(typeof(int))] SqlExpression a,
      [ParamType(typeof(int))] SqlExpression b)
    {
      return a * b;
    }

    [Compiler(typeof(Math), "Ceiling", TargetKind.Static | TargetKind.Method)]
    public static SqlExpression MathCeilingDouble(
      [ParamType(typeof(double))] SqlExpression d)
    {
      return SqlFactory.Ceiling(d);
    }

    [Compiler(typeof(Math), "Ceiling", TargetKind.Static | TargetKind.Method)]
    public static SqlExpression MathCeilingDecimal(
      [ParamType(typeof(decimal))] SqlExpression d)
    {
      return SqlFactory.Ceiling(d);
    }

    [Compiler(typeof(Math), "Cos", TargetKind.Static | TargetKind.Method)]
    public static SqlExpression MathCos(
      [ParamType(typeof(double))] SqlExpression d)
    {
      return SqlFactory.Cos(d);
    }

    [Compiler(typeof(Math), "Cosh", TargetKind.Static | TargetKind.Method)]
    public static SqlExpression MathCosh(
      [ParamType(typeof(double))] SqlExpression d)
    {
      return (SqlFactory.Exp(d) + SqlFactory.Exp(-d)) / SqlFactory.Literal(2);
    }

    [Compiler(typeof(Math), "Exp", TargetKind.Static | TargetKind.Method)]
    public static SqlExpression MathExp(
      [ParamType(typeof(double))] SqlExpression d)
    {
      return SqlFactory.Exp(d);
    }

    [Compiler(typeof(Math), "Floor", TargetKind.Static | TargetKind.Method)]
    public static SqlExpression MathFloorDouble(
      [ParamType(typeof(double))] SqlExpression d)
    {
      return SqlFactory.Floor(d);
    }

    [Compiler(typeof(Math), "Floor", TargetKind.Static | TargetKind.Method)]
    public static SqlExpression MathFloorDecimal(
      [ParamType(typeof(decimal))] SqlExpression d)
    {
      return SqlFactory.Floor(d);
    }

    [Compiler(typeof(Math), "Log", TargetKind.Static | TargetKind.Method)]
    public static SqlExpression MathLog(
      [ParamType(typeof(double))] SqlExpression d)
    {
      return SqlFactory.Log(d);
    }

    [Compiler(typeof(Math), "Log", TargetKind.Static | TargetKind.Method)]
    public static SqlExpression MathLog(
      [ParamType(typeof(double))] SqlExpression d,
      [ParamType(typeof(double))] SqlExpression newBase)
    {
      return SqlFactory.Log(d) / SqlFactory.Log(newBase);
    }

    [Compiler(typeof(Math), "Log10", TargetKind.Static | TargetKind.Method)]
    public static SqlExpression MathLog10(
      [ParamType(typeof(double))] SqlExpression d)
    {
      return SqlFactory.Log10(d);
    }

    [Compiler(typeof(Math), "Pow", TargetKind.Static | TargetKind.Method)]
    public static SqlExpression MathPow(
      [ParamType(typeof(double))] SqlExpression x,
      [ParamType(typeof(double))] SqlExpression y)
    {
      return SqlFactory.Power(x, y);
    }

    [Compiler(typeof(Math), "Round", TargetKind.Static | TargetKind.Method)]
    public static SqlExpression MathRoundDouble(
      [ParamType(typeof(double))] SqlExpression d)
    {
      return SqlFactory.Round(d, SqlFactory.Literal(0));
    }

    [Compiler(typeof(Math), "Round", TargetKind.Static | TargetKind.Method)]
    public static SqlExpression MathRoundDouble(
      [ParamType(typeof(double))] SqlExpression d,
      [ParamType(typeof(int))] SqlExpression digits)
    {
      return SqlFactory.Round(d, digits);
    }

    [Compiler(typeof(Math), "Round", TargetKind.Static | TargetKind.Method)]
    public static SqlExpression MathRoundDecimal(
      [ParamType(typeof(decimal))] SqlExpression d)
    {
      return SqlFactory.Round(d, SqlFactory.Literal(0));
    }

    [Compiler(typeof(Math), "Round", TargetKind.Static | TargetKind.Method)]
    public static SqlExpression MathRoundDecimal(
      [ParamType(typeof(decimal))] SqlExpression d,
      [ParamType(typeof(int))] SqlExpression decimals)
    {
      return SqlFactory.Round(d, decimals);
    }

    [Compiler(typeof(Math), "Sin", TargetKind.Static | TargetKind.Method)]
    public static SqlExpression MathSin(
      [ParamType(typeof(double))] SqlExpression d)
    {
      return SqlFactory.Sin(d);
    }

    [Compiler(typeof(Math), "Sinh", TargetKind.Static | TargetKind.Method)]
    public static SqlExpression MathSinh(
      [ParamType(typeof(double))] SqlExpression d)
    {
      return (SqlFactory.Exp(d) - SqlFactory.Exp(-d)) / SqlFactory.Literal(2);
    }

    [Compiler(typeof(Math), "Sqrt", TargetKind.Static | TargetKind.Method)]
    public static SqlExpression MathSqrt(
      [ParamType(typeof(double))] SqlExpression d)
    {
      return SqlFactory.Sqrt(d);
    }

    [Compiler(typeof(Math), "Tan", TargetKind.Static | TargetKind.Method)]
    public static SqlExpression MathTan(
      [ParamType(typeof(double))] SqlExpression d)
    {
      return SqlFactory.Tan(d);
    }

    [Compiler(typeof(Math), "Tanh", TargetKind.Static | TargetKind.Method)]
    public static SqlExpression MathTanh(
      [ParamType(typeof(double))] SqlExpression d)
    {
      var exp2d = SqlFactory.Exp(SqlFactory.Literal(2) * d);
      var one = SqlFactory.Literal(1);

      return (exp2d - one) / (exp2d + one);
    }

    // made internal for using in DecimalMappings
    internal static SqlExpression TruncateHelper(SqlExpression d)
    {
      var result = SqlFactory.Case();
      result.Add(d > 0, SqlFactory.Floor(d));
      result.Else = SqlFactory.Ceiling(d);
      return result;
    }

    [Compiler(typeof(Math), "Truncate", TargetKind.Static | TargetKind.Method)]
    public static SqlExpression MathTruncateDouble(
      [ParamType(typeof(double))] SqlExpression d)
    {
      return TruncateHelper(d);
    }

    [Compiler(typeof(Math), "Truncate", TargetKind.Static | TargetKind.Method)]
    public static SqlExpression MathTruncateDecimal(
      [ParamType(typeof(decimal))] SqlExpression d)
    {
      return TruncateHelper(d);
    }

  }
}

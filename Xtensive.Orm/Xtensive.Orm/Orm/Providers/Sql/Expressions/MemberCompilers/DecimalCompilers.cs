// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.02.18

using System;
using Xtensive.Linq;
using Xtensive.Sql;
using Xtensive.Sql.Dml;
using Operator = Xtensive.Reflection.WellKnown.Operator;

namespace Xtensive.Orm.Providers.Sql.Expressions
{
  [CompilerContainer(typeof(SqlExpression))]
  internal static class DecimalCompilers
  {
    #region Static methods

    [Compiler(typeof(decimal), "Add", TargetKind.Static | TargetKind.Method)]
    public static SqlExpression DecimalAdd(
      [Type(typeof(decimal))] SqlExpression d1,
      [Type(typeof(decimal))] SqlExpression d2)
    {
      return d1 + d2;
    }

    [Compiler(typeof(decimal), "Ceiling", TargetKind.Static | TargetKind.Method)]
    public static SqlExpression DecimalCeiling(
      [Type(typeof(decimal))] SqlExpression d)
    {
      return MathCompilers.MathCeilingDecimal(d);
    }

    [Compiler(typeof(decimal), "Compare", TargetKind.Static | TargetKind.Method)]
    public static SqlExpression DecimalCompare(
      [Type(typeof(decimal))] SqlExpression d1,
      [Type(typeof(decimal))] SqlExpression d2)
    {
      return ExpressionTranslationHelpers.ToInt(SqlDml.Sign(d1 - d2));
    }

    [Compiler(typeof(decimal), "Divide", TargetKind.Static | TargetKind.Method)]
    public static SqlExpression DecimalDivide(
      [Type(typeof(decimal))] SqlExpression d1,
      [Type(typeof(decimal))] SqlExpression d2)
    {
      return d1 / d2;
    }

    [Compiler(typeof(decimal), "Equals", TargetKind.Static | TargetKind.Method)]
    public static SqlExpression DecimalEquals(
      [Type(typeof(decimal))] SqlExpression d1,
      [Type(typeof(decimal))] SqlExpression d2)
    {
      return d1==d2;
    }

    [Compiler(typeof(decimal), "Floor", TargetKind.Static | TargetKind.Method)]
    public static SqlExpression DecimalFloor(
      [Type(typeof(decimal))] SqlExpression d)
    {
      return MathCompilers.MathFloorDecimal(d);
    }

    [Compiler(typeof(decimal), "Multiply", TargetKind.Static | TargetKind.Method)]
    public static SqlExpression DecimalMultiply(
      [Type(typeof(decimal))] SqlExpression d1,
      [Type(typeof(decimal))] SqlExpression d2)
    {
      return d1 * d2;
    }

    [Compiler(typeof(decimal), "Negate", TargetKind.Static | TargetKind.Method)]
    public static SqlExpression DecimalMultiply(
      [Type(typeof(decimal))] SqlExpression d)
    {
      return -d;
    }

    [Compiler(typeof(decimal), "Parse", TargetKind.Static | TargetKind.Method)]
    public static SqlExpression DecimalParse(
      [Type(typeof(string))] SqlExpression str)
    {
      return ExpressionTranslationHelpers.ToDecimal(str);
    }

    [Compiler(typeof(decimal), "Remainder", TargetKind.Static | TargetKind.Method)]
    public static SqlExpression DecimalRemainder(
      [Type(typeof(decimal))] SqlExpression d1,
      [Type(typeof(decimal))] SqlExpression d2)
    {
      return d1 % d2;
    }

    [Compiler(typeof(decimal), "Round", TargetKind.Static | TargetKind.Method)]
    public static SqlExpression DecimalRound(
      [Type(typeof(decimal))] SqlExpression d)
    {
      return MathCompilers.MathRoundDecimal(d);
    }

    [Compiler(typeof(decimal), "Round", TargetKind.Static | TargetKind.Method)]
    public static SqlExpression DecimalRound(
      [Type(typeof(decimal))] SqlExpression d,
      [Type(typeof(int))] SqlExpression decimals)
    {
      return MathCompilers.MathRoundDecimal(d, decimals);
    }

    [Compiler(typeof(decimal), "Round", TargetKind.Static | TargetKind.Method)]
    public static SqlExpression DecimalRoundWithMode(
      [Type(typeof(decimal))] SqlExpression d,
      [Type(typeof(MidpointRounding))] SqlExpression mode)
    {
      return MathCompilers.MathRoundDecimalWithMode(d, mode);
    }

    [Compiler(typeof(decimal), "Round", TargetKind.Static | TargetKind.Method)]
    public static SqlExpression DecimalRoundWithMode(
      [Type(typeof(decimal))] SqlExpression d,
      [Type(typeof(int))] SqlExpression decimals,
      [Type(typeof(MidpointRounding))] SqlExpression mode)
    {
      return MathCompilers.MathRoundDecimalWithMode(d, decimals, mode);
    }

    [Compiler(typeof(decimal), "Subtract", TargetKind.Static | TargetKind.Method)]
    public static SqlExpression DecimalSubtract(
      [Type(typeof(decimal))] SqlExpression d1,
      [Type(typeof(decimal))] SqlExpression d2)
    {
      return d1 - d2;
    }

    [Compiler(typeof(decimal), "Truncate", TargetKind.Static | TargetKind.Method)]
    public static SqlExpression DecimalTruncate(
      [Type(typeof(decimal))] SqlExpression d)
    {
      return MathCompilers.MathTruncateDecimal(d);
    }

    #endregion

    #region Operators

    [Compiler(typeof(decimal), Operator.Equality, TargetKind.Operator)]
    public static SqlExpression DecimalOperatorEquality(
      [Type(typeof(decimal))] SqlExpression left,
      [Type(typeof(decimal))] SqlExpression right)
    {
      return left == right;
    }

    [Compiler(typeof(decimal), Operator.Inequality, TargetKind.Operator)]
    public static SqlExpression DecimalOperatorInequality(
      [Type(typeof(decimal))] SqlExpression left,
      [Type(typeof(decimal))] SqlExpression right)
    {
      return left != right;
    }

    [Compiler(typeof(decimal), Operator.GreaterThan, TargetKind.Operator)]
    public static SqlExpression DecimalOperatorGreaterThan(
      [Type(typeof(decimal))] SqlExpression left,
      [Type(typeof(decimal))] SqlExpression right)
    {
      return left > right;
    }

    [Compiler(typeof(decimal), Operator.GreaterThanOrEqual, TargetKind.Operator)]
    public static SqlExpression DecimalOperatorGreaterThanOrEqual(
      [Type(typeof(decimal))] SqlExpression left,
      [Type(typeof(decimal))] SqlExpression right)
    {
      return left >= right;
    }

    [Compiler(typeof(decimal), Operator.LessThan, TargetKind.Operator)]
    public static SqlExpression DecimalOperatorLessThan(
      [Type(typeof(decimal))] SqlExpression left,
      [Type(typeof(decimal))] SqlExpression right)
    {
      return left < right;
    }

    [Compiler(typeof(decimal), Operator.LessThanOrEqual, TargetKind.Operator)]
    public static SqlExpression DecimalOperatorLessThanOrEqual(
      [Type(typeof(decimal))] SqlExpression left,
      [Type(typeof(decimal))] SqlExpression right)
    {
      return left <= right;
    }


    [Compiler(typeof(decimal), Operator.Addition, TargetKind.Operator)]
    public static SqlExpression DecimalOperatorAddition(
      [Type(typeof(decimal))] SqlExpression left,
      [Type(typeof(decimal))] SqlExpression right)
    {
      return left + right;
    }

    [Compiler(typeof(decimal), Operator.Subtraction, TargetKind.Operator)]
    public static SqlExpression DecimalOperatorSubtractionTimeSpan(
      [Type(typeof(decimal))] SqlExpression left,
      [Type(typeof(decimal))] SqlExpression right)
    {
      return left - right;
    }

    [Compiler(typeof(decimal), Operator.Multiply, TargetKind.Operator)]
    public static SqlExpression DecimalOperatorMultiply(
      [Type(typeof(decimal))] SqlExpression left,
      [Type(typeof(decimal))] SqlExpression right)
    {
      return left * right;
    }

    [Compiler(typeof(decimal), Operator.Division, TargetKind.Operator)]
    public static SqlExpression DecimalOperatorDivision(
      [Type(typeof(decimal))] SqlExpression left,
      [Type(typeof(decimal))] SqlExpression right)
    {
      return left / right;
    }

    [Compiler(typeof(decimal), Operator.Modulus, TargetKind.Operator)]
    public static SqlExpression DecimalOperatorModulo(
      [Type(typeof(decimal))] SqlExpression left,
      [Type(typeof(decimal))] SqlExpression right)
    {
      return left % right;
    }

    [Compiler(typeof(decimal), Operator.UnaryNegation, TargetKind.Operator)]
    public static SqlExpression DecimalOperatorUnaryNegation(
      [Type(typeof(decimal))] SqlExpression d)
    {
      return -d;
    }

    [Compiler(typeof(decimal), Operator.UnaryPlus, TargetKind.Operator)]
    public static SqlExpression DecimalOperatorUnaryPlus(
      [Type(typeof(decimal))] SqlExpression d)
    {
      return d;
    }

    #endregion

    #region Implicit type casts

    [Compiler(typeof(decimal), Operator.Implicit, TargetKind.Operator)]
    public static SqlExpression DecimalOperatorImplicitCastFromShort(
      [Type(typeof(short))] SqlExpression d)
    {
      return d;
    }

    [Compiler(typeof(decimal), Operator.Implicit, TargetKind.Operator)]
    public static SqlExpression DecimalOperatorImplicitCastFromUShort(
      [Type(typeof(ushort))] SqlExpression d)
    {
      return d;
    }

    [Compiler(typeof(decimal), Operator.Implicit, TargetKind.Operator)]
    public static SqlExpression DecimalOperatorImplicitCastFromInt(
      [Type(typeof(int))] SqlExpression d)
    {
      return d;
    }

    [Compiler(typeof(decimal), Operator.Implicit, TargetKind.Operator)]
    public static SqlExpression DecimalOperatorImplicitCastFromUInt(
      [Type(typeof(uint))] SqlExpression d)
    {
      return d;
    }

    [Compiler(typeof(decimal), Operator.Implicit, TargetKind.Operator)]
    public static SqlExpression DecimalOperatorImplicitCastFromLong(
      [Type(typeof(long))] SqlExpression d)
    {
      return d;
    }

    [Compiler(typeof(decimal), Operator.Implicit, TargetKind.Operator)]
    public static SqlExpression DecimalOperatorImplicitCastFromULong(
      [Type(typeof(ulong))] SqlExpression d)
    {
      return d;
    }

    [Compiler(typeof(decimal), Operator.Implicit, TargetKind.Operator)]
    public static SqlExpression DecimalOperatorImplicitCastFromByte(
      [Type(typeof(byte))] SqlExpression d)
    {
      return d;
    }

    [Compiler(typeof(decimal), Operator.Implicit, TargetKind.Operator)]
    public static SqlExpression DecimalOperatorImplicitCastFromSByte(
      [Type(typeof(sbyte))] SqlExpression d)
    {
      return d;
    }

    #endregion

    #region Explicit type casts

    [Compiler(typeof(decimal), Operator.Explicit, TargetKind.Operator)]
    [return: Type(typeof(short))]
    public static SqlExpression DecimalOperatorExplicitCastToShort(
      [Type(typeof(decimal))] SqlExpression d)
    {
      return d;
    }

    [Compiler(typeof(decimal), Operator.Explicit, TargetKind.Operator)]
    [return: Type(typeof(ushort))]
    public static SqlExpression DecimalOperatorExplicitCastToUShort(
      [Type(typeof(decimal))] SqlExpression d)
    {
      return d;
    }

    [Compiler(typeof(decimal), Operator.Explicit, TargetKind.Operator)]
    [return: Type(typeof(int))]
    public static SqlExpression DecimalOperatorExplicitCastToInt(
      [Type(typeof(decimal))] SqlExpression d)
    {
      return d;
    }

    [Compiler(typeof(decimal), Operator.Explicit, TargetKind.Operator)]
    [return: Type(typeof(uint))]
    public static SqlExpression DecimalOperatorExplicitCastToUInt(
      [Type(typeof(decimal))] SqlExpression d)
    {
      return d;
    }

    [Compiler(typeof(decimal), Operator.Explicit, TargetKind.Operator)]
    [return: Type(typeof(long))]
    public static SqlExpression DecimalOperatorExplicitCastToLong(
      [Type(typeof(decimal))] SqlExpression d)
    {
      return d;
    }

    [Compiler(typeof(decimal), Operator.Explicit, TargetKind.Operator)]
    [return: Type(typeof(ulong))]
    public static SqlExpression DecimalOperatorExplicitCastToULong(
      [Type(typeof(decimal))] SqlExpression d)
    {
      return d;
    }

    [Compiler(typeof(decimal), Operator.Explicit, TargetKind.Operator)]
    [return: Type(typeof(byte))]
    public static SqlExpression DecimalOperatorExplicitCastToByte(
      [Type(typeof(decimal))] SqlExpression d)
    {
      return d;
    }

    [Compiler(typeof(decimal), Operator.Explicit, TargetKind.Operator)]
    [return: Type(typeof(sbyte))]
    public static SqlExpression DecimalOperatorExplicitCastToSByte(
      [Type(typeof(decimal))] SqlExpression d)
    {
      return d;
    }

    [Compiler(typeof(decimal), Operator.Explicit, TargetKind.Operator)]
    [return: Type(typeof(float))]
    public static SqlExpression DecimalOperatorExplicitCastToFloat(
      [Type(typeof(decimal))] SqlExpression d)
    {
      return d;
    }

    [Compiler(typeof(decimal), Operator.Explicit, TargetKind.Operator)]
    [return: Type(typeof(double))]
    public static SqlExpression DecimalOperatorExplicitCastToDouble(
      [Type(typeof(decimal))] SqlExpression d)
    {
      return d;
    }

    [Compiler(typeof(decimal), Operator.Explicit, TargetKind.Operator)]
    public static SqlExpression DecimalOperatorExplicitCastFromFloat(
      [Type(typeof(float))] SqlExpression d)
    {
      return d;
    }

    [Compiler(typeof(decimal), Operator.Explicit, TargetKind.Operator)]
    public static SqlExpression DecimalOperatorExplicitCastFromDouble(
      [Type(typeof(double))] SqlExpression d)
    {
      return d;
    }
    
    #endregion
  }
}
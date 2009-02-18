// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.02.18

using System;
using Xtensive.Core.Linq;
using Xtensive.Sql.Common;
using Xtensive.Sql.Dom.Dml;
using SqlFactory = Xtensive.Sql.Dom.Sql;

namespace Xtensive.Storage.Providers.Sql.Mappings.FunctionMappings
{
  internal class DecimalMappings
  {
    [Compiler(typeof(decimal), "Add", TargetKind.Static | TargetKind.Method)]
    public static SqlExpression DecimalAdd(
      [ParamType(typeof(decimal))] SqlExpression d1,
      [ParamType(typeof(decimal))] SqlExpression d2)
    {
      return d1 + d2;
    }

    [Compiler(typeof(decimal), "Ceiling", TargetKind.Static | TargetKind.Method)]
    public static SqlExpression DecimalCeiling(
      [ParamType(typeof(decimal))] SqlExpression d)
    {
      return SqlFactory.Ceiling(d);
    }

    [Compiler(typeof(decimal), "Compare", TargetKind.Static | TargetKind.Method)]
    public static SqlExpression DecimalCompare(
      [ParamType(typeof(decimal))] SqlExpression d1,
      [ParamType(typeof(decimal))] SqlExpression d2)
    {
      return SqlFactory.Sign(d1 - d2);
    }

    [Compiler(typeof(decimal), "Divide", TargetKind.Static | TargetKind.Method)]
    public static SqlExpression DecimalDivide(
      [ParamType(typeof(decimal))] SqlExpression d1,
      [ParamType(typeof(decimal))] SqlExpression d2)
    {
      return d1 / d2;
    }

    [Compiler(typeof(decimal), "Equals", TargetKind.Static | TargetKind.Method)]
    public static SqlExpression DecimalEquals(
      [ParamType(typeof(decimal))] SqlExpression d1,
      [ParamType(typeof(decimal))] SqlExpression d2)
    {
      return d1==d2;
    }

    [Compiler(typeof(decimal), "Floor", TargetKind.Static | TargetKind.Method)]
    public static SqlExpression DecimalFloor(
      [ParamType(typeof(decimal))] SqlExpression d)
    {
      return SqlFactory.Floor(d);
    }

    [Compiler(typeof(decimal), "Multiply", TargetKind.Static | TargetKind.Method)]
    public static SqlExpression DecimalMultiply(
      [ParamType(typeof(decimal))] SqlExpression d1,
      [ParamType(typeof(decimal))] SqlExpression d2)
    {
      return d1 * d2;
    }

    [Compiler(typeof(decimal), "Negate", TargetKind.Static | TargetKind.Method)]
    public static SqlExpression DecimalMultiply(
      [ParamType(typeof(decimal))] SqlExpression d)
    {
      return -d;
    }

    [Compiler(typeof(decimal), "Parse", TargetKind.Static | TargetKind.Method)]
    public static SqlExpression DecimalParse(
      [ParamType(typeof(string))] SqlExpression str)
    {
      return SqlFactory.Cast(str, SqlDataType.Decimal);
    }

    [Compiler(typeof(decimal), "Remainder", TargetKind.Static | TargetKind.Method)]
    public static SqlExpression DecimalRemainder(
      [ParamType(typeof(decimal))] SqlExpression d1,
      [ParamType(typeof(decimal))] SqlExpression d2)
    {
      return d1 % d2;
    }

    [Compiler(typeof(decimal), "Round", TargetKind.Static | TargetKind.Method)]
    public static SqlExpression DecimalRound(
      [ParamType(typeof(decimal))] SqlExpression d)
    {
      return SqlFactory.Round(d, SqlFactory.Literal(0));
    }

    [Compiler(typeof(decimal), "Round", TargetKind.Static | TargetKind.Method)]
    public static SqlExpression DecimalRound(
      [ParamType(typeof(decimal))] SqlExpression d,
      [ParamType(typeof(int))] SqlExpression decimals)
    {
      return SqlFactory.Round(d, decimals);
    }

    [Compiler(typeof(decimal), "Subtract", TargetKind.Static | TargetKind.Method)]
    public static SqlExpression DecimalSubtract(
      [ParamType(typeof(decimal))] SqlExpression d1,
      [ParamType(typeof(decimal))] SqlExpression d2)
    {
      return d1 - d2;
    }

    [Compiler(typeof(decimal), "Truncate", TargetKind.Static | TargetKind.Method)]
    public static SqlExpression DecimalTruncate(
      [ParamType(typeof(decimal))] SqlExpression d)
    {
      return MathMappings.TruncateHelper(d);
    }
  }
}

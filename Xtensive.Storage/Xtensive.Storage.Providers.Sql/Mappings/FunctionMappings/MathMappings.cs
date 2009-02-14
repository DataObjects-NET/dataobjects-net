// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.02.14

using System;
using System.Linq;
using Xtensive.Core.Linq;
using Xtensive.Sql.Dom.Dml;
using SqlFactory = Xtensive.Sql.Dom.Sql;

namespace Xtensive.Storage.Providers.Sql.Mappings.FunctionMappings
{
  internal class MathMappings
  {
    [Compiler(typeof(Math), "Pow", TargetKind.Static | TargetKind.Method)]
    static SqlExpression MathPow(
      [ParamType(typeof(double))] SqlExpression x,
      [ParamType(typeof(double))] SqlExpression y)
    {
      return SqlFactory.Power(x, y);
    }
    
    [Compiler(typeof(Math), "Floor", TargetKind.Static | TargetKind.Method)]
    static SqlExpression MathFloorDouble(
      [ParamType(typeof(double))] SqlExpression d)
    {
      return SqlFactory.Floor(d);
    }

    [Compiler(typeof(Math), "Floor", TargetKind.Static | TargetKind.Method)]
    static SqlExpression MathFloorDecimal(
      [ParamType(typeof(decimal))] SqlExpression d)
    {
      return SqlFactory.Floor(d);
    }
  }
}

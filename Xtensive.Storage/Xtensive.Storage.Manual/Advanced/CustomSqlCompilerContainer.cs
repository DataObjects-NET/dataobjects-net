// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2009.11.11

using System;
using System.Diagnostics;
using System.Reflection;
using Xtensive.Core.Linq;
using Xtensive.Sql;
using Xtensive.Sql.Dml;

namespace Xtensive.Storage.Manual.Advanced.CustomSqlCompiler
{
  [CompilerContainer(typeof(Xtensive.Sql.Dml.SqlExpression))]
  public static class CustomSqlCompilerContainer
  {
    [Compiler(typeof(CustomSqlCompilerStringExtensions), "GetThirdChar", TargetKind.Method | TargetKind.Static)]
    public static SqlExpression GetThirdChar(SqlExpression _this)
    {
      return SqlDml.Substring(_this, 2, 1);
    }

    [Compiler(typeof(CustomSqlCompilerStringExtensions), "BuildAddressString", TargetKind.Method | TargetKind.Static)]
    public static SqlExpression BuildAddressString(SqlExpression countryExpression, SqlExpression streetExpression, SqlExpression buildingExpression)
    {
      return SqlDml.Concat(countryExpression, SqlDml.Literal(", "), streetExpression, SqlDml.Literal("-"), buildingExpression);
    }

    [Compiler(typeof(string), "GetHashCode", TargetKind.Method)]
    public static SqlExpression GetHashCode(SqlExpression _this)
    {
      // return string length as hashcode.
      return SqlDml.CharLength(_this);
    }
  }
}
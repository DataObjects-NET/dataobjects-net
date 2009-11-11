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

namespace Xtensive.Storage.Manual.Advanced
{
  [CompilerContainer(typeof(SqlExpression))]
  public static class CustomStringCompilerContainer
  {
    [Compiler(typeof(CustomCompilerStringExtensions), "GetThirdChar", TargetKind.Method | TargetKind.Static)]
    public static SqlExpression GetThirdChar(SqlExpression _this)
    {
      return SqlDml.Substring(_this, 2, 1);;
    }
  }
}
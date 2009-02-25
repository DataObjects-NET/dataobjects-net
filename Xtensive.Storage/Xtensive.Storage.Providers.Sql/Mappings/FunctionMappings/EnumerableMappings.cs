// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.02.24

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Xtensive.Core.Linq;
using Xtensive.Sql.Dom.Dml;
using SqlFactory = Xtensive.Sql.Dom.Sql;

namespace Xtensive.Storage.Providers.Sql.Mappings.FunctionMappings
{
  internal static class EnumerableMappings
  {
    [Compiler(typeof(Enumerable), "Contains", TargetKind.Static | TargetKind.Method)]
    public static SqlExpression EnumerableContains(MethodInfo methodInfo,
      SqlExpression source, SqlExpression value)
    {
      throw new NotImplementedException();
    }
  }
}

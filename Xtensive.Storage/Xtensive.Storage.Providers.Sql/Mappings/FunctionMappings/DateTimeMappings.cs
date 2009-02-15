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
  internal static class DateTimeMappings
  {
    [Compiler(typeof(DateTime), "Now", TargetKind.Static | TargetKind.PropertyGet)]
    public static SqlExpression DateTimeNow()
    {
      return SqlFactory.CurrentTimeStamp();
    }
  }
}

// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.02.20

using System;
using System.Reflection;
using Xtensive.Core.Linq;
using Xtensive.Sql.Dom.Dml;
using SqlFactory = Xtensive.Sql.Dom.Sql;

namespace Xtensive.Storage.Providers.Sql.Mappings.FunctionMappings
{
  internal static class NullableMappings
  {
    [Compiler(typeof(Nullable<>), "Value", TargetKind.PropertyGet)]
    public static SqlExpression NullableValue(MemberInfo memberInfo, SqlExpression this_)
    {
      return this_;
    }

    [Compiler(typeof(Nullable<>), "HasValue", TargetKind.PropertyGet)]
    public static SqlExpression NullableHasValue(MemberInfo memberInfo, SqlExpression this_)
    {
      return SqlFactory.IsNotNull(this_);
    }
  }
}

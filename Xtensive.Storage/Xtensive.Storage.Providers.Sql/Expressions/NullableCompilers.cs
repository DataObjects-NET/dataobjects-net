// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.02.20

using System;
using System.Reflection;
using Xtensive.Core.Linq;
using Xtensive.Sql;
using Xtensive.Sql.Dml;

namespace Xtensive.Storage.Providers.Sql.Expressions
{
  [CompilerContainer(typeof(SqlExpression))]
  internal static class NullableCompilers
  {
    [Compiler(typeof(Nullable<>), "Value", TargetKind.PropertyGet)]
    public static SqlExpression NullableValue(MemberInfo memberInfo, SqlExpression _this)
    {
      return _this;
    }

    [Compiler(typeof(Nullable<>), "HasValue", TargetKind.PropertyGet)]
    public static SqlExpression NullableHasValue(MemberInfo memberInfo, SqlExpression _this)
    {
      return SqlDml.IsNotNull(_this);
    }
  }
}
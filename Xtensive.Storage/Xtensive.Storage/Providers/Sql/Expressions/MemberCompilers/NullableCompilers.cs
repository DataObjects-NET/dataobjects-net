// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.02.20

using System;
using System.Reflection;
using Xtensive.Linq;
using Xtensive.Reflection;
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

    [Compiler(typeof(Nullable<>), "GetValueOrDefault")]
    public static SqlExpression NullableGetValueOrDefault(MemberInfo memberInfo, SqlExpression _this)
    {
      var valueType = memberInfo.DeclaringType.StripNullable();
      return SqlDml.Coalesce(_this, SqlDml.Literal(Activator.CreateInstance(valueType)));
    }

    [Compiler(typeof(Nullable<>), "GetValueOrDefault")]
    public static SqlExpression NullableGetValueOrDefault(MemberInfo memberInfo, SqlExpression _this, SqlExpression _default)
    {
      return SqlDml.Coalesce(_this, _default);
    }
  }
}
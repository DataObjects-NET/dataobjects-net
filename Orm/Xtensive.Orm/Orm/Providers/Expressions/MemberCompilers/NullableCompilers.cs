// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.02.20

using System;
using System.Linq.Expressions;
using System.Reflection;
using Xtensive.Linq;
using Xtensive.Reflection;
using Xtensive.Sql;
using Xtensive.Sql.Dml;

namespace Xtensive.Orm.Providers
{
  [CompilerContainer(typeof (SqlExpression))]
  internal static class NullableCompilers
  {
    [Compiler(typeof (Nullable<>), "Value", TargetKind.PropertyGet)]
    public static SqlExpression NullableValue(MemberInfo memberInfo, SqlExpression _this)
    {
      return _this;
    }

    [Compiler(typeof (Nullable<>), "HasValue", TargetKind.PropertyGet)]
    public static SqlExpression NullableHasValue(MemberInfo memberInfo, SqlExpression _this)
    {
      var context = ExpressionTranslationContext.Current;
      if (!IsBooleanSpecialCase(context, memberInfo))
        return SqlDml.IsNotNull(_this);
      return SqlDml.IsNotNull(context.BooleanExpressionConverter.BooleanToInt(_this));
    }

    [Compiler(typeof (Nullable<>), "GetValueOrDefault")]
    public static SqlExpression NullableGetValueOrDefault(MemberInfo memberInfo, SqlExpression _this)
    {
      var context = ExpressionTranslationContext.Current;
      var defaultValue = SqlDml.Literal(Activator.CreateInstance(memberInfo.DeclaringType.StripNullable()));
      if (!IsBooleanSpecialCase(context, memberInfo))
        return SqlDml.Coalesce(_this, defaultValue);
      return context.BooleanExpressionConverter.IntToBoolean(
        SqlDml.Coalesce(context.BooleanExpressionConverter.BooleanToInt(_this), defaultValue));
    }

    [Compiler(typeof (Nullable<>), "GetValueOrDefault")]
    public static SqlExpression NullableGetValueOrDefault(MemberInfo memberInfo, SqlExpression _this, SqlExpression _default)
    {
      var context = ExpressionTranslationContext.Current;
      var @this = _this;
      var @default = _default;
      SqlContainer container = @this as SqlContainer;
      if (container != null)
        if (container.Value.GetType().IsEnum)
          @this = SqlDml.Literal(Convert.ChangeType(container.Value, Enum.GetUnderlyingType(container.Value.GetType())));
      container = @default as SqlContainer;
      if (container != null)
        if (container.Value.GetType().IsEnum)
          @default = SqlDml.Literal(Convert.ChangeType(container.Value, Enum.GetUnderlyingType(container.Value.GetType())));
      if (!IsBooleanSpecialCase(context, memberInfo))
        return SqlDml.Coalesce(@this, @default);
      return context.BooleanExpressionConverter.IntToBoolean(SqlDml.Coalesce(
        context.BooleanExpressionConverter.BooleanToInt(@this),
        context.BooleanExpressionConverter.BooleanToInt(@default)));
    }

    private static bool IsBooleanSpecialCase(ExpressionTranslationContext context, MemberInfo member)
    {
      return member.DeclaringType==typeof (bool?) && !context.ProviderInfo.Supports(ProviderFeatures.FullFeaturedBooleanExpressions);
    }
  }
}
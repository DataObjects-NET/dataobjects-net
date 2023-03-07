// Copyright (C) 2009-2022 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Denis Krjuchkov
// Created:    2009.09.18

using System;
using Xtensive.Reflection;
using Xtensive.Sql;
using Xtensive.Sql.Dml;

namespace Xtensive.Orm.Providers
{
  internal static class ExpressionTranslationHelpers
  {
    public static SqlExpression ToBool(SqlExpression target) =>
      Cast(target, WellKnownTypes.Bool);

    public static SqlExpression ToInt(SqlExpression target) =>
      Cast(target, WellKnownTypes.Int32);

    public static SqlExpression ToDouble(SqlExpression target) =>
      Cast(target, WellKnownTypes.Double);

    public static SqlExpression ToLong(SqlExpression target) =>
      Cast(target, WellKnownTypes.Int64);

    public static SqlExpression ToSbyte(SqlExpression target) =>
      Cast(target, WellKnownTypes.SByte);

    public static SqlExpression ToShort(SqlExpression target) =>
      Cast(target, WellKnownTypes.Int16);

    public static SqlExpression ToFloat(SqlExpression target) =>
      Cast(target, WellKnownTypes.Single);
    
    public static SqlExpression ToDecimal(SqlExpression target) =>
      Cast(target, WellKnownTypes.Decimal);
    
    public static SqlExpression ToByte(SqlExpression target) =>
      Cast(target, WellKnownTypes.Byte);

    public static SqlExpression ToChar(SqlExpression target) =>
      Cast(target, WellKnownTypes.String);

    public static SqlExpression ToDateTime(SqlExpression target) =>
      Cast(target, WellKnownTypes.DateTime);
#if NET6_0_OR_GREATER

    public static SqlExpression ToDate(SqlExpression target) =>
      Cast(target, WellKnownTypes.DateOnly);

    public static SqlExpression ToTime(SqlExpression target) =>
      Cast(target, WellKnownTypes.TimeOnly);
#endif

    public static SqlExpression ToUint(SqlExpression target) =>
      Cast(target, WellKnownTypes.UInt32);

    public static SqlExpression ToUlong(SqlExpression target) =>
      Cast(target, WellKnownTypes.UInt64);

    public static SqlExpression ToUshort(SqlExpression target) =>
      Cast(target, WellKnownTypes.UInt16);

    public static SqlExpression ToString(SqlExpression target) =>
      Cast(target, WellKnownTypes.String);

    private static SqlExpression Cast(SqlExpression target, Type type)
    {
      var destinationType = ExpressionTranslationContext.Current.Driver.MapValueType(type);
      return SqlDml.Cast(target, destinationType);
    }
  }
}

// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
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
    public static SqlExpression ToBool(SqlExpression target)
    {
      return Cast(target, WellKnownTypes.Bool);
    }

    public static SqlExpression ToInt(SqlExpression target)
    {
      return Cast(target, WellKnownTypes.Int32);
    }

    public static SqlExpression ToDouble(SqlExpression target)
    {
      return Cast(target, WellKnownTypes.Double);
    }

    public static SqlExpression ToLong(SqlExpression target)
    {
      return Cast(target, WellKnownTypes.Int64);
    }

    public static SqlExpression ToSbyte(SqlExpression target)
    {
      return Cast(target, WellKnownTypes.SByte);
    }

    public static SqlExpression ToShort(SqlExpression target)
    {
      return Cast(target, WellKnownTypes.Int16);
    }

    public static SqlExpression ToFloat(SqlExpression target)
    {
      return Cast(target, WellKnownTypes.Single);
    }
    
    public static SqlExpression ToDecimal(SqlExpression target)
    {
      return Cast(target, WellKnownTypes.Decimal);
    }
    
    public static SqlExpression ToByte(SqlExpression target)
    {
      return Cast(target, WellKnownTypes.Byte);
    }

    public static SqlExpression ToChar(SqlExpression target)
    {
      return Cast(target, WellKnownTypes.String);
    }

    public static SqlExpression ToDateTime(SqlExpression target)
    {
      return Cast(target, typeof (DateTime));
    }

    public static SqlExpression ToUint(SqlExpression target)
    {
      return Cast(target, WellKnownTypes.UInt32);
    }

    public static SqlExpression ToUlong(SqlExpression target)
    {
      return Cast(target, WellKnownTypes.UInt64);
    }

    public static SqlExpression ToUshort(SqlExpression target)
    {
      return Cast(target, WellKnownTypes.UInt16);
    }

    public static SqlExpression ToString(SqlExpression target)
    {
      return Cast(target, WellKnownTypes.String);
    }

    private static SqlExpression Cast(SqlExpression target, Type type)
    {
      var destinationType = ExpressionTranslationContext.Current.Driver.MapValueType(type);
      return SqlDml.Cast(target, destinationType);
    }
  }
}
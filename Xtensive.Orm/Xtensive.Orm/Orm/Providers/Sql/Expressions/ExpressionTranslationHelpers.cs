// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.09.18

using System;
using Xtensive.Sql;
using Xtensive.Sql.Dml;

namespace Xtensive.Orm.Providers.Sql.Expressions
{
  internal static class ExpressionTranslationHelpers
  {
    public static SqlExpression ToBool(SqlExpression target)
    {
      return Cast(target, typeof (bool));
    }

    public static SqlExpression ToInt(SqlExpression target)
    {
      return Cast(target, typeof (int));
    }

    public static SqlExpression ToDouble(SqlExpression target)
    {
      return Cast(target, typeof (double));
    }

    public static SqlExpression ToLong(SqlExpression target)
    {
      return Cast(target, typeof (long));
    }

    public static SqlExpression ToSbyte(SqlExpression target)
    {
      return Cast(target, typeof (sbyte));
    }

    public static SqlExpression ToShort(SqlExpression target)
    {
      return Cast(target, typeof (short));
    }

    public static SqlExpression ToFloat(SqlExpression target)
    {
      return Cast(target, typeof (float));
    }
    
    public static SqlExpression ToDecimal(SqlExpression target)
    {
      return Cast(target, typeof (decimal));
    }
    
    public static SqlExpression ToByte(SqlExpression target)
    {
      return Cast(target, typeof (byte));
    }

    public static SqlExpression ToChar(SqlExpression target)
    {
      return Cast(target, typeof (string));
    }

    public static SqlExpression ToDateTime(SqlExpression target)
    {
      return Cast(target, typeof (DateTime));
    }

    public static SqlExpression ToUint(SqlExpression target)
    {
      return Cast(target, typeof (uint));
    }

    public static SqlExpression ToUlong(SqlExpression target)
    {
      return Cast(target, typeof (ulong));
    }

    public static SqlExpression ToUshort(SqlExpression target)
    {
      return Cast(target, typeof (ushort));
    }

    public static SqlExpression ToString(SqlExpression target)
    {
      return Cast(target, typeof (string));
    }

    private static SqlExpression Cast(SqlExpression target, Type type)
    {
      var destinationType = ExpressionTranslationContext.Current.Driver.BuildValueType(type);
      return SqlDml.Cast(target, destinationType);
    }
  }
}
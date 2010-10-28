// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.09.18

using System;
using Xtensive.Sql;
using Xtensive.Sql.Dml;

namespace Xtensive.Storage.Providers.Sql.Expressions
{
  internal static class ExpressionTranslationHelpers
  {
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
    
    public static SqlExpression ToDecimal(SqlExpression target)
    {
      return Cast(target, typeof (decimal));
    }

    public static SqlExpression ToChar(SqlExpression target)
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
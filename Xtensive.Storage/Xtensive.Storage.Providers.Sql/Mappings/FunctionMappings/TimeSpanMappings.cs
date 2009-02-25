// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.02.24

using System;
using Xtensive.Core.Linq;
using Xtensive.Sql.Dom.Dml;
using Operator = Xtensive.Core.Reflection.WellKnown.Operator;
using SqlFactory = Xtensive.Sql.Dom.Sql;

namespace Xtensive.Storage.Providers.Sql.Mappings.FunctionMappings
{
  internal static class TimeSpanMappings
  {
    #region Extractors

    [Compiler(typeof(TimeSpan), "Milliseconds", TargetKind.PropertyGet)]
    public static SqlExpression TimeSpanMilliseconds(SqlExpression this_)
    {
      throw new NotImplementedException();
    }

    [Compiler(typeof(TimeSpan), "Seconds", TargetKind.PropertyGet)]
    public static SqlExpression TimeSpanSeconds(SqlExpression this_)
    {
      throw new NotImplementedException();
    }

    [Compiler(typeof(TimeSpan), "Minutes", TargetKind.PropertyGet)]
    public static SqlExpression TimeSpanMinutes(SqlExpression this_)
    {
      throw new NotImplementedException();
    }

    [Compiler(typeof(TimeSpan), "Hours", TargetKind.PropertyGet)]
    public static SqlExpression TimeSpanHours(SqlExpression this_)
    {
      throw new NotImplementedException();
    }
    
    [Compiler(typeof(TimeSpan), "Days", TargetKind.PropertyGet)]
    public static SqlExpression TimeSpanDays(SqlExpression this_)
    {
      throw new NotImplementedException();
    }

    #endregion

    #region Converters

    [Compiler(typeof(TimeSpan), "Ticks", TargetKind.PropertyGet)]
    public static SqlExpression TimeSpanTicks(SqlExpression this_)
    {
      throw new NotImplementedException();
    }

    [Compiler(typeof(TimeSpan), "TotalMilliseconds", TargetKind.PropertyGet)]
    public static SqlExpression TimeSpanTotalMilliseconds(SqlExpression this_)
    {
      throw new NotImplementedException();
    }

    [Compiler(typeof(TimeSpan), "TotalSeconds", TargetKind.PropertyGet)]
    public static SqlExpression TimeSpanTotalSeconds(SqlExpression this_)
    {
      throw new NotImplementedException();
    }

    [Compiler(typeof(TimeSpan), "TotalMinutes", TargetKind.PropertyGet)]
    public static SqlExpression TimeSpanTotalMinutes(SqlExpression this_)
    {
      throw new NotImplementedException();
    }

    [Compiler(typeof(TimeSpan), "TotalHours", TargetKind.PropertyGet)]
    public static SqlExpression TimeSpanTotalHours(SqlExpression this_)
    {
      throw new NotImplementedException();
    }

    [Compiler(typeof(TimeSpan), "TotalDays", TargetKind.PropertyGet)]
    public static SqlExpression TimeSpanTotalDays(SqlExpression this_)
    {
      throw new NotImplementedException();
    }

    #endregion

    #region Operators

    [Compiler(typeof(TimeSpan), Operator.Addition, TargetKind.Operator)]
    public static SqlExpression TimeSpanOperatorAddition(
      [Type(typeof(TimeSpan))] SqlExpression t1,
      [Type(typeof(TimeSpan))] SqlExpression t2)
    {
      throw new NotImplementedException();
    }

    [Compiler(typeof(TimeSpan), Operator.Subtraction, TargetKind.Operator)]
    public static SqlExpression TimeSpanOperatorSubtraction(
      [Type(typeof(TimeSpan))] SqlExpression t1,
      [Type(typeof(TimeSpan))] SqlExpression t2)
    {
      throw new NotImplementedException();
    }

    [Compiler(typeof(TimeSpan), Operator.UnaryPlus, TargetKind.Operator)]
    public static SqlExpression TimeSpanOperatorUnaryPlus(
      [Type(typeof(TimeSpan))] SqlExpression t)
    {
      return t;
    }

    [Compiler(typeof(TimeSpan), Operator.UnaryNegation, TargetKind.Operator)]
    public static SqlExpression TimeSpanOperatorUnaryNegation(
      [Type(typeof(TimeSpan))] SqlExpression t)
    {
      throw new NotImplementedException();
    }

    #endregion
  }
}

// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Aleksey Gamzov
// Created:    2010.11.02

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xtensive.Linq;
using Xtensive.Sql;
using Xtensive.Sql.Dml;

namespace Xtensive.Orm.Providers.Sql.Expressions
{
  [CompilerContainer(typeof(SqlExpression))]
  internal static class VbDateAndTimeCompilers
  {
    #if NET40
      private const string VbDateAndTime = "Microsoft.VisualBasic.DateAndTime, Microsoft.VisualBasic, Version=10.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a";
    #else
      private const string VbDateAndTime = "Microsoft.VisualBasic.DateAndTime, Microsoft.VisualBasic, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a";
    #endif

    [Compiler(VbDateAndTime, "Year", TargetKind.Static)]
    public static SqlExpression Year(SqlExpression dateExpression)
    {
      return ExpressionTranslationHelpers.ToInt(SqlDml.Extract(SqlDateTimePart.Year, dateExpression));
    }

    [Compiler(VbDateAndTime, "Month", TargetKind.Static)]
    public static SqlExpression Month(SqlExpression dateExpression)
    {
      return ExpressionTranslationHelpers.ToInt(SqlDml.Extract(SqlDateTimePart.Month, dateExpression));
    }

    [Compiler(VbDateAndTime, "Day", TargetKind.Static)]
    public static SqlExpression Day(SqlExpression dateExpression)
    {
      return ExpressionTranslationHelpers.ToInt(SqlDml.Extract(SqlDateTimePart.Day, dateExpression));
    }

    [Compiler(VbDateAndTime, "Hour", TargetKind.Static)]
    public static SqlExpression Hour(SqlExpression dateExpression)
    {
      return ExpressionTranslationHelpers.ToInt(SqlDml.Extract(SqlDateTimePart.Hour, dateExpression));
    }

    [Compiler(VbDateAndTime, "Minute", TargetKind.Static)]
    public static SqlExpression Minute(SqlExpression dateExpression)
    {
      return ExpressionTranslationHelpers.ToInt(SqlDml.Extract(SqlDateTimePart.Minute, dateExpression));
    }

    [Compiler(VbDateAndTime, "Second", TargetKind.Static)]
    public static SqlExpression Second(SqlExpression dateExpression)
    {
      return ExpressionTranslationHelpers.ToInt(SqlDml.Extract(SqlDateTimePart.Second, dateExpression));
    }
  }
}

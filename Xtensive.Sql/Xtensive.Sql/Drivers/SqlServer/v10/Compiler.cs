// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.07.07

using System;
using Xtensive.Sql.Dml;

namespace Xtensive.Sql.SqlServer.v10
{
  internal class Compiler : v09.Compiler
  {
    protected static SqlUserFunctionCall DateAddNanosecond(SqlExpression date, SqlExpression nanoseconds)
    {
      return SqlDml.FunctionCall("DATEADD", SqlDml.Native("NS"), nanoseconds, date);
    }

    protected static SqlUserFunctionCall DateDiffNanosecond(SqlExpression date1, SqlExpression date2)
    {
      return SqlDml.FunctionCall("DATEDIFF", SqlDml.Native("NS"), date1, date2);
    }

    protected override SqlExpression DateTimeTruncate(SqlExpression date)
    {
      return SqlDml.Cast(date, new SqlValueType("Date"));
    }

    protected override SqlExpression  DateTimeSubtractDateTime(SqlExpression date1, SqlExpression date2)
    {
      return base.DateTimeSubtractDateTime(date1, date2)
        + DateDiffNanosecond(
          DateAddMillisecond(
            DateAddDay(date2, DateDiffDay(date2, date1)),
            DateDiffMillisecond(DateAddDay(date2, DateDiffDay(date2, date1)), date1)),
          date1);
    }

    protected override SqlExpression DateTimeAddInterval(SqlExpression date, SqlExpression interval)
    {
      return DateAddNanosecond(
        DateAddMillisecond(
          DateAddDay(date, interval / NanosecondsPerDay),
          (interval/NanosecondsPerMillisecond) % (MillisecondsPerDay)),
        (interval/NanosecondsPerSecond) % NanosecondsPerDay/NanosecondsPerSecond);
    }

    // Constructors

    public Compiler(SqlDriver driver)
      : base(driver)
    {
    }
  }
}
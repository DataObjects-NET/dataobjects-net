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
    protected static readonly long NanosecondsPerDay = TimeSpan.FromDays(1).Ticks*100;
    protected static readonly long NanosecondsPerSecond = 1000000000;
    protected static readonly long NanosecondsPerMillisecond = 1000000;

    protected static SqlUserFunctionCall DateAddNanosecond(SqlExpression date, SqlExpression nanoseconds)
    {
      return SqlDml.FunctionCall("DATEADD", SqlDml.Native("NS"), nanoseconds, date);
    }

    protected static SqlUserFunctionCall DateDiffNanosecond(SqlExpression date1, SqlExpression date2)
    {
      return SqlDml.FunctionCall("DATEDIFF", SqlDml.Native("NS"), date1, date2);
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
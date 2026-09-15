// Copyright (C) 2010-2026 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Aleksey Gamzov
// Created:    2010.11.02

using Xtensive.Sql;
using Xtensive.Sql.Dml;

namespace Xtensive.Orm.Providers
{
  [CompilerContainer(typeof(SqlExpression))]
  internal static class VbDateAndTimeCompilers
  {
#if NET10_0_OR_GREATER
    private const string VbDateAndTime = "Microsoft.VisualBasic.DateAndTime, Microsoft.VisualBasic.Core, Version=15.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a";
#else
    private const string VbDateAndTime = "Microsoft.VisualBasic.DateAndTime, Microsoft.VisualBasic.Core, Version=13.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a";
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

// Copyright (C) 2003-2013 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kulakov
// Created:    2013.07.23

using System;
using Xtensive.Sql;
using Xtensive.Sql.Dml;

namespace Xtensive.Orm.Providers
{
  [CompilerContainer(typeof(SqlExpression))]
  internal static class EnumCompilers
  {
    [Compiler(typeof(Enum), "HasFlag")]
    public static SqlExpression EnumHasFlag(SqlExpression _this, 
      [Type(typeof (Enum))] SqlExpression value)
    {
      var right = GetSqlLiterExpression(value);
      var a = right is SqlLiteral<bool>;
      return SqlDml.Equals(SqlDml.BitAnd(GetSqlLiterExpression(_this),right), right);
    }
    
    private static SqlExpression GetSqlLiterExpression(SqlExpression expression)
    {
      var container = expression as SqlContainer;
      if (container != null) {
        var containeredValue = (container).Value as Enum;
        if (containeredValue!=null)
          return SqlDml.Literal(Convert.ChangeType(containeredValue,Enum.GetUnderlyingType(containeredValue.GetType())));
        throw new NotSupportedException(Strings.ExNonEnumParametersForEnumHasFlagAreNotSupported);
      }
      return expression;
    }
  }
}

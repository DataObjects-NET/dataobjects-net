// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Aleksey Gamzov
// Created:    2010.11.02

using Xtensive.Core.Linq;
using Xtensive.Sql;
using Xtensive.Sql.Dml;

namespace Xtensive.Storage.Providers.Sql.Expressions
{
  [CompilerContainer(typeof(SqlExpression))]
  internal static class VbStringsCompilers
  {
    #if NET40
      private const string VbStrings = "Microsoft.VisualBasic.Strings, Microsoft.VisualBasic, Version=10.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a";
    #else
      private const string VbStrings = "Microsoft.VisualBasic.Strings, Microsoft.VisualBasic, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a";
    #endif

    [Compiler(VbStrings, "Trim", TargetKind.Static)]
    public static SqlExpression Trim(SqlExpression stringExpression)
    {
      return SqlDml.Trim(stringExpression);
    }

  }
}

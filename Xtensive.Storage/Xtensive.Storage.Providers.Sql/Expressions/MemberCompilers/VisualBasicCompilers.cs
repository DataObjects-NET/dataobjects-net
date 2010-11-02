﻿// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Aleksey Gamzov
// Created:    2010.11.02

using System;
using System.Linq;
using System.Reflection;
using System.Text;
using Xtensive.Core;
using Xtensive.Core.Helpers;
using Xtensive.Core.Linq;
using Xtensive.Sql;
using Xtensive.Sql.Dml;
using Xtensive.Storage.Providers.Sql.Resources;
using Operator = Xtensive.Core.Reflection.WellKnown.Operator;

namespace Xtensive.Storage.Providers.Sql.Expressions
{
  [CompilerContainer(typeof(SqlExpression))]
  internal static class VisualBasicCompilers
  {
    #if NET40
      private const string VbStrings = "Microsoft.VisualBasic.Strings, Microsoft.VisualBasic, Version=10.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a";
      private const string VbOperators = "Microsoft.VisualBasic.CompilerServices.Operators, Microsoft.VisualBasic, Version=10.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a";
    #else
      private const string VbStrings = "";
      private const string VbOperators = "";
    #endif

    [Compiler(VbStrings, "Trim", TargetKind.Static)]
    public static SqlExpression StringTrimVb(SqlExpression stringExpression)
    {
      return SqlDml.Trim(stringExpression);
    }

    // [Compiler(VbOperators, "CompareString", TargetKind.Static)]
    public static SqlExpression CompareStringVb(SqlExpression leftStringExpression, SqlExpression rightStringExpression)
    {
      return SqlDml.Equals(leftStringExpression, rightStringExpression);
    }
  }
}

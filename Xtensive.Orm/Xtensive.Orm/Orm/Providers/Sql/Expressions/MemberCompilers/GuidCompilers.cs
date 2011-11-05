// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.07.10

using System;
using Xtensive.Linq;
using Xtensive.Sql;
using Xtensive.Sql.Dml;
using Operator = Xtensive.Reflection.WellKnown.Operator;

namespace Xtensive.Orm.Providers.Sql.Expressions
{
  [CompilerContainer(typeof(SqlExpression))]
  internal class GuidCompilers
  {
    [Compiler(typeof(Guid), Operator.Equality, TargetKind.Operator)]
    public static SqlExpression StringOperatorEquality(SqlExpression left, SqlExpression right)
    {
      return SqlDml.Equals(left, right);
    }

    [Compiler(typeof(Guid), Operator.Inequality, TargetKind.Operator)]
    public static SqlExpression StringOperatorInequality(SqlExpression left, SqlExpression right)
    {
      return SqlDml.NotEquals(left, right);
    }
  }
}
// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.07.24

using System;
using System.Diagnostics;
using Xtensive.Core;

namespace Xtensive.Sql.Dml
{
  public class SqlExtract : SqlExpression
  {
    public SqlDateTimePart DateTimePart { get; private set; }
    public SqlDateTimeOffsetPart DateTimeOffsetPart { get; private set; }
    public SqlIntervalPart IntervalPart { get; private set; }

    public SqlExpression Operand { get; private set; }

    public override void ReplaceWith(SqlExpression expression)
    {
      var replacingExpression = ArgumentValidator.EnsureArgumentIs<SqlExtract>(expression);
      DateTimePart = replacingExpression.DateTimePart;
      DateTimeOffsetPart = replacingExpression.DateTimeOffsetPart;
      IntervalPart = replacingExpression.IntervalPart;
      Operand = replacingExpression.Operand;
    }

    internal override SqlExtract Clone(SqlNodeCloneContext context) =>
      context.GetOrAdd(this, static (t, c) =>
        t.DateTimePart != SqlDateTimePart.Nothing
          ? new SqlExtract(t.DateTimePart, t.Operand.Clone(c))
          : t.IntervalPart != SqlIntervalPart.Nothing
            ? new SqlExtract(t.IntervalPart, t.Operand.Clone(c))
            : new SqlExtract(t.DateTimeOffsetPart, t.Operand.Clone(c)));

    public override void AcceptVisitor(ISqlVisitor visitor)
    {
      visitor.Visit(this);
    }

    // Constructors

    internal SqlExtract(SqlDateTimePart dateTimePart, SqlExpression operand)
      : base(SqlNodeType.Extract)
    {
      DateTimePart = dateTimePart;
      DateTimeOffsetPart = SqlDateTimeOffsetPart.Nothing;
      IntervalPart = SqlIntervalPart.Nothing;
      Operand = operand;
    }

    internal SqlExtract(SqlIntervalPart intervalPart, SqlExpression operand)
      : base(SqlNodeType.Extract)
    {
      DateTimePart = SqlDateTimePart.Nothing;
      DateTimeOffsetPart = SqlDateTimeOffsetPart.Nothing;
      IntervalPart = intervalPart;
      Operand = operand;
    }

    public SqlExtract(SqlDateTimeOffsetPart dateTimeOffsetPart, SqlExpression operand)
      : base(SqlNodeType.Extract)
    {
      DateTimePart = SqlDateTimePart.Nothing;
      IntervalPart = SqlIntervalPart.Nothing;
      DateTimeOffsetPart = dateTimeOffsetPart;
      Operand = operand;
    }
  }
}
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

    internal override object Clone(SqlNodeCloneContext context) =>
      context.NodeMapping.TryGetValue(this, out var clone)
        ? clone
        : context.NodeMapping[this] = DateTimePart!=SqlDateTimePart.Nothing
          ? new SqlExtract(DateTimePart, (SqlExpression) Operand.Clone(context))
          : IntervalPart!=SqlIntervalPart.Nothing
            ? new SqlExtract(IntervalPart, (SqlExpression) Operand.Clone(context))
            : new SqlExtract(DateTimeOffsetPart, (SqlExpression) Operand.Clone(context));

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
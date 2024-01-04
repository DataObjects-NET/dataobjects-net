// Copyright (C) 2009-2023 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Denis Krjuchkov
// Created:    2009.07.24

using System;
using System.Diagnostics;
using System.Reflection;
using Xtensive.Core;

namespace Xtensive.Sql.Dml
{
  public class SqlExtract : SqlExpression
  {
    private const int TimeTypeId = 1;
    private const int DateTypeId = 2;
    private const int DateTimeTypeId = 3;
    private const int DateTimeOffsetTypeId = 4;
    private const int IntervalTypeId = 5;

    private SqlDateTimeOffsetPart internalValue;
    private int typeMarker;
    private bool typeHasTime;

    public SqlDateTimePart DateTimePart =>
      typeMarker == DateTimeTypeId ? internalValue.ToDateTimePartFast() : SqlDateTimePart.Nothing;

    public SqlDatePart DatePart =>
      typeMarker == DateTypeId ? internalValue.ToDatePartFast() : SqlDatePart.Nothing;

    public SqlTimePart TimePart =>
      typeMarker == TimeTypeId ? internalValue.ToTimePartFast() : SqlTimePart.Nothing;

    public SqlDateTimeOffsetPart DateTimeOffsetPart =>
      typeMarker == DateTimeOffsetTypeId ? internalValue : SqlDateTimeOffsetPart.Nothing;

    public SqlIntervalPart IntervalPart =>
      typeMarker == IntervalTypeId ? internalValue.ToIntervalPartFast(): SqlIntervalPart.Nothing;

    public SqlExpression Operand { get; private set; }

    public bool IsSecondExtraction =>
      typeHasTime && internalValue == SqlDateTimeOffsetPart.Second;
    public bool IsMillisecondExtraction =>
      typeHasTime && internalValue == SqlDateTimeOffsetPart.Millisecond;

    public bool IsDateTimeOffsetPart => typeMarker == DateTimeOffsetTypeId;

    public bool IsDateTimePart => typeMarker == DateTimeTypeId;

    public bool IsDatePart => typeMarker == DateTypeId;

    public bool IsTimePart => typeMarker == TimeTypeId;

    public bool IsIntervalPart => typeMarker == IntervalTypeId;

    public override void ReplaceWith(SqlExpression expression)
    {
      var replacingExpression = ArgumentValidator.EnsureArgumentIs<SqlExtract>(expression);
      internalValue = replacingExpression.internalValue;
      typeMarker = replacingExpression.typeMarker;
      typeHasTime = replacingExpression.typeHasTime;
      //DateTimePart = replacingExpression.DateTimePart;
      //DateTimeOffsetPart = replacingExpression.DateTimeOffsetPart;
      //IntervalPart = replacingExpression.IntervalPart;
      Operand = replacingExpression.Operand;
    }

    internal override SqlExtract Clone(SqlNodeCloneContext context) =>
      context.GetOrAdd(this, static (t, c) => new SqlExtract(t.internalValue, t.typeMarker, t.Operand.Clone(c)));
        //t.DateTimePart != SqlDateTimePart.Nothing
        //  ? new SqlExtract(t.DateTimePart, t.Operand.Clone(c))
        //  : t.IntervalPart != SqlIntervalPart.Nothing
        //    ? new SqlExtract(t.IntervalPart, t.Operand.Clone(c))
        //    : new SqlExtract(t.DateTimeOffsetPart, t.Operand.Clone(c)));

    public override void AcceptVisitor(ISqlVisitor visitor)
    {
      visitor.Visit(this);
    }

    // Constructors

    internal SqlExtract(SqlDateTimePart dateTimePart, SqlExpression operand)
      : base(SqlNodeType.Extract)
    {
      internalValue = dateTimePart.ToDtoPartFast();
      typeMarker = DateTimeTypeId;
      typeHasTime = true;

      //DateTimePart = dateTimePart;
      //DateTimeOffsetPart = SqlDateTimeOffsetPart.Nothing;
      //IntervalPart = SqlIntervalPart.Nothing;
      Operand = operand;
    }

    internal SqlExtract(SqlIntervalPart intervalPart, SqlExpression operand)
      : base(SqlNodeType.Extract)
    {
      internalValue = intervalPart.ToDtoPartFast();
      typeMarker = IntervalTypeId;
      typeHasTime = true;

      //DateTimePart = SqlDateTimePart.Nothing;
      //DateTimeOffsetPart = SqlDateTimeOffsetPart.Nothing;
      //IntervalPart = intervalPart;
      Operand = operand;
    }

    internal SqlExtract(SqlDateTimeOffsetPart dateTimeOffsetPart, SqlExpression operand)
      : base(SqlNodeType.Extract)
    {
      internalValue = dateTimeOffsetPart;
      typeMarker = DateTimeOffsetTypeId;
      typeHasTime = true;
      Operand = operand;
    }

    internal SqlExtract(SqlDatePart datePart, SqlExpression operand)
      : base(SqlNodeType.Extract)
    {
      internalValue = datePart.ToDtoPartFast();
      typeMarker = DateTypeId;
      typeHasTime = false;
      Operand = operand;
    }

    internal SqlExtract(SqlTimePart timePart, SqlExpression operand)
      : base(SqlNodeType.Extract)
    {
      internalValue = timePart.ToDtoPartFast();
      typeMarker = TimeTypeId;
      typeHasTime = true;
      Operand = operand;
    }

    private SqlExtract(SqlDateTimeOffsetPart internalValue, int typeMarker, SqlExpression operand)
      :base(SqlNodeType.Extract)
    {
      this.internalValue = internalValue;
      this.typeMarker = typeMarker;
      typeHasTime = typeMarker == DateTimeTypeId || typeMarker == DateTimeOffsetTypeId || typeMarker == TimeTypeId || typeMarker == IntervalTypeId;
      Operand = operand;
    }
  }
}
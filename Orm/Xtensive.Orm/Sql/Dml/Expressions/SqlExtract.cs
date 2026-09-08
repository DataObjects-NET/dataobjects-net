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
    private const int DateTimeTypeId = 3;
    private const int DateTimeOffsetTypeId = 4;
    private const int IntervalTypeId = 5;

    private SqlDateTimeOffsetPart internalValue;
    private int typeMarker;

    public SqlDateTimePart DateTimePart =>
      typeMarker == DateTimeTypeId ? internalValue.ToDateTimePartFast() : SqlDateTimePart.Nothing;

    public SqlDateTimeOffsetPart DateTimeOffsetPart =>
      typeMarker == DateTimeOffsetTypeId ? internalValue : SqlDateTimeOffsetPart.Nothing;

    public SqlIntervalPart IntervalPart =>
      typeMarker == IntervalTypeId ? internalValue.ToIntervalPartFast() : SqlIntervalPart.Nothing;

    public SqlExpression Operand { get; private set; }

    public bool IsSecondExtraction =>
      internalValue == SqlDateTimeOffsetPart.Second;
    public bool IsMillisecondExtraction =>
      internalValue == SqlDateTimeOffsetPart.Millisecond;

    public bool IsDateTimeOffsetPart => typeMarker == DateTimeOffsetTypeId;

    public bool IsDateTimePart => typeMarker == DateTimeTypeId;

    public bool IsIntervalPart => typeMarker == IntervalTypeId;

    public override void ReplaceWith(SqlExpression expression)
    {
      ArgumentValidator.EnsureArgumentNotNull(expression, "expression");
      ArgumentValidator.EnsureArgumentIs<SqlExtract>(expression, "expression");
      var replacingExpression = (SqlExtract) expression;
      internalValue = replacingExpression.internalValue;
      typeMarker = replacingExpression.typeMarker;
      Operand = replacingExpression.Operand;
    }

    internal override object Clone(SqlNodeCloneContext context)
    {
      if (context.NodeMapping.ContainsKey(this))
        return context.NodeMapping[this];

      var clone = new SqlExtract(this.internalValue, this.typeMarker, (SqlExpression)this.Operand.Clone(context));
      context.NodeMapping[this] = clone;
      return clone;
    }

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
      Operand = operand;
    }

    internal SqlExtract(SqlIntervalPart intervalPart, SqlExpression operand)
      : base(SqlNodeType.Extract)
    {
      internalValue = intervalPart.ToDtoPartFast();
      typeMarker = IntervalTypeId;
      Operand = operand;
    }

    public SqlExtract(SqlDateTimeOffsetPart dateTimeOffsetPart, SqlExpression operand)
      : base(SqlNodeType.Extract)
    {
      internalValue = dateTimeOffsetPart;
      typeMarker = DateTimeOffsetTypeId;
      Operand = operand;
    }

    private SqlExtract(SqlDateTimeOffsetPart internalValue, int typeMarker, SqlExpression operand)
      : base(SqlNodeType.Extract)
    {
      this.internalValue = internalValue;
      this.typeMarker = typeMarker;
      Operand = operand;
    }
  }
}
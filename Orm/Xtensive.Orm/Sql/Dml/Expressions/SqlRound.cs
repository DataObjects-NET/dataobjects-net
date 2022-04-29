// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.08.03

using System;
using Xtensive.Core;

namespace Xtensive.Sql.Dml
{
  [Serializable]
  public sealed class SqlRound : SqlExpression
  {
    public SqlExpression Argument { get; private set; }
    public SqlExpression Length { get; private set; }
    public TypeCode Type { get; private set; }
    public MidpointRounding Mode { get; private set; }
    
    public override void ReplaceWith(SqlExpression expression)
    {
      var replacingExpression = ArgumentValidator.EnsureArgumentIs<SqlRound>(expression);
      Argument = replacingExpression.Argument;
      Length = replacingExpression.Length;
      Type = replacingExpression.Type;
      Mode = replacingExpression.Mode;
    }

    internal override object Clone(SqlNodeCloneContext context) =>
      context.NodeMapping.TryGetValue(this, out var clone)
        ? clone
        : context.NodeMapping[this] = new SqlRound(
            (SqlExpression) Argument.Clone(context),
            Length.IsNullReference() ? null : (SqlExpression) Length.Clone(context),
            Type, Mode);

    public override void AcceptVisitor(ISqlVisitor visitor)
    {
      visitor.Visit(this);
    }

    // Constructors

    internal SqlRound(SqlExpression argument, SqlExpression length, TypeCode type, MidpointRounding mode)
      : base(SqlNodeType.Round)
    {
      Argument = argument;
      Length = length;
      Type = type;
      Mode = mode;
    }
  }
}
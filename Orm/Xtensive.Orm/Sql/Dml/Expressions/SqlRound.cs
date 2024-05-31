// Copyright (C) 2009-2024 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
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
      ArgumentValidator.EnsureArgumentIs<SqlRound>(expression, "expression");
      var replacingExpression = (SqlRound) expression;
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
            Length is null ? null : (SqlExpression) Length.Clone(context),
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
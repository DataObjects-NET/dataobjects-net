// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.04.22

using Xtensive.Core;

namespace Xtensive.Sql.Dom.Dml
{
  public class SqlVariant : SqlExpression
  {
    private SqlExpression main;
    private SqlExpression alternative;
    private object key;

    public SqlExpression Main { get { return main; } }
    public SqlExpression Alternative { get { return alternative; } }
    public object Key { get { return key; } }

    public override void ReplaceWith(SqlExpression expression)
    {
      ArgumentValidator.EnsureArgumentNotNull(expression, "expression");
      ArgumentValidator.EnsureArgumentIs<SqlVariant>(expression, "expression");

      var replacingExpression = (SqlVariant) expression;
      main = replacingExpression.main;
      alternative = replacingExpression.alternative;
      key = replacingExpression.key;
    }

    internal override object Clone(SqlNodeCloneContext context)
    {
      if (context.NodeMapping.ContainsKey(this))
        return context.NodeMapping[this];

      var clone = new SqlVariant((SqlExpression) main.Clone(context), (SqlExpression) alternative.Clone(context), key);
      context.NodeMapping[this] = clone;
      return clone;
    }

    public override void AcceptVisitor(ISqlVisitor visitor)
    {
      visitor.Visit(this);
    }

    internal SqlVariant(SqlExpression main, SqlExpression alternative, object key)
      : base(SqlNodeType.Variant)
    {
      this.main = main;
      this.alternative = alternative;
      this.key = key;
    }
  }
}
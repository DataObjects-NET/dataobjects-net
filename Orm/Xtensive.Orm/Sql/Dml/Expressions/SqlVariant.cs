// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.04.22

using Xtensive.Core;

namespace Xtensive.Sql.Dml
{
  public class SqlVariant : SqlExpression
  {
    public object Id { get; private set; }
    public SqlExpression Main { get; private set; }
    public SqlExpression Alternative { get; private set; }
    
    public override void ReplaceWith(SqlExpression expression)
    {
      var replacingExpression = ArgumentValidator.EnsureArgumentIs<SqlVariant>(expression);
      Main = replacingExpression.Main;
      Alternative = replacingExpression.Alternative;
      Id = replacingExpression.Id;
    }

    internal override object Clone(SqlNodeCloneContext context) =>
      context.NodeMapping.TryGetValue(this, out var clone)
        ? clone
        : context.NodeMapping[this] = new SqlVariant(Id, (SqlExpression) Main.Clone(context), (SqlExpression) Alternative.Clone(context));

    public override void AcceptVisitor(ISqlVisitor visitor)
    {
      visitor.Visit(this);
    }

    internal SqlVariant(object id, SqlExpression main, SqlExpression alternative)
      : base(SqlNodeType.Variant)
    {
      Main = main;
      Alternative = alternative;
      Id = id;
    }
  }
}
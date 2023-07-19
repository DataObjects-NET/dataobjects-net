// Copyright (C) 2011 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2011.10.10

namespace Xtensive.Sql.Dml
{
  public class SqlFragment : SqlNode, ISqlCompileUnit
  {
    public SqlExpression Expression { get; private set; }

    internal override SqlFragment Clone(SqlNodeCloneContext context) =>
      context.GetOrAdd(this, static (t, c) =>
        new SqlFragment(t.Expression.Clone(c)));

    public override void AcceptVisitor(ISqlVisitor visitor)
    {
      visitor.Visit(this);
    }


    // Constructors

    internal SqlFragment(SqlExpression expression)
      : base(SqlNodeType.Fragment)
    {
      Expression = expression;
    }
  }
}
// Copyright (C) 2011-2024 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Denis Krjuchkov
// Created:    2011.10.10

namespace Xtensive.Sql.Dml
{
  public class SqlFragment : SqlNode, ISqlCompileUnit
  {
    public SqlExpression Expression { get; private set; }

    internal override object Clone(SqlNodeCloneContext context) =>
      context.NodeMapping.TryGetValue(this, out var clone)
        ? clone
        : context.NodeMapping[this] = new SqlFragment((SqlExpression) Expression.Clone(context));

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
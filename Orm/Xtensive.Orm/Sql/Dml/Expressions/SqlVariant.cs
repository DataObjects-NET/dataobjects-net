// Copyright (C) 2009-2024 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
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
      ArgumentValidator.EnsureArgumentNotNull(expression, "expression");
      ArgumentValidator.EnsureArgumentIs<SqlVariant>(expression, "expression");

      var replacingExpression = (SqlVariant) expression;
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
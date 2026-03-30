// Copyright (C) 2009-2024 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Denis Krjuchkov
// Created:    2009.10.10

using Xtensive.Core;

namespace Xtensive.Sql.Dml
{
  public class SqlPlaceholder : SqlExpression
  {
    public object Id { get; private set; }

    internal override SqlPlaceholder Clone(SqlNodeCloneContext context) =>
      context.GetOrAdd(this, static (t, c) =>
        new SqlPlaceholder(t.Id));

    public override void AcceptVisitor(ISqlVisitor visitor)
    {
      visitor.Visit(this);
    }

    public override void ReplaceWith(SqlExpression expression)
    {
      var replacingExpression = ArgumentValidator.EnsureArgumentIs<SqlPlaceholder>(expression);
      Id = replacingExpression.Id;
    }

    // Constructors

    internal SqlPlaceholder(object id)
      : base(SqlNodeType.Placeholder)
    {
      Id = id;
    }
  }
}
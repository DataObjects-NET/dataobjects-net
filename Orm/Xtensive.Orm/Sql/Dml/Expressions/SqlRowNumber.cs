// Copyright (C) 2009-2024 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Denis Krjuchkov
// Created:    2009.08.21

using Xtensive.Core;

namespace Xtensive.Sql.Dml
{
  public class SqlRowNumber : SqlExpression
  {
    public SqlOrderCollection OrderBy { get; private set; }

    internal override SqlRowNumber Clone(SqlNodeCloneContext context) =>
      context.GetOrAdd(this, static (t, c) => {
        var clone = new SqlRowNumber();
        foreach (SqlOrder so in t.OrderBy)
          clone.OrderBy.Add(so.Clone(c));
        return clone;
      });

    public override void AcceptVisitor(ISqlVisitor visitor)
    {
      visitor.Visit(this);
    }

    public override void ReplaceWith(SqlExpression expression)
    {
      var replacingExpression = ArgumentValidator.EnsureArgumentIs<SqlRowNumber>(expression);
      OrderBy.Clear();
      foreach (var item in replacingExpression.OrderBy)
        OrderBy.Add(item);
    }
    

    // Constructors
    
    internal SqlRowNumber()
      : base(SqlNodeType.RowNumber)
    {
      OrderBy = new SqlOrderCollection();
    }
  }
}
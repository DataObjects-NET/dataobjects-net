// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.08.21

using Xtensive.Core;

namespace Xtensive.Sql.Dml
{
  public class SqlRowNumber : SqlExpression
  {
    public SqlOrderCollection OrderBy { get; private set; }

    internal override object Clone(SqlNodeCloneContext context)
    {
      if (context.NodeMapping.TryGetValue(this, out var value)) {
        return value;
      }
      var clone = new SqlRowNumber();
      foreach (SqlOrder so in OrderBy)
        clone.OrderBy.Add((SqlOrder) so.Clone(context));
      context.NodeMapping[this] = clone;
      return clone;
    }

    public override void AcceptVisitor(ISqlVisitor visitor)
    {
      visitor.Visit(this);
    }

    public override void ReplaceWith(SqlExpression expression)
    {
      ArgumentValidator.EnsureArgumentNotNull(expression, "expression");
      ArgumentValidator.EnsureArgumentIs<SqlRowNumber>(expression, "expression");
      var replacingExpression = (SqlRowNumber) expression;
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
// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.

using System;

namespace Xtensive.Sql.Dml
{
  /// <summary>
  /// Represents order specification.
  /// </summary>
  [Serializable]
  public class SqlOrder : SqlNode
  {
    private readonly SqlExpression expression;
    private readonly int position;
    private readonly bool ascending;

    /// <summary>
    /// Gets the expression to sort by.
    /// </summary>
    /// <value>The expression.</value>
    public SqlExpression Expression
    {
      get { return expression; }
    }

    /// <summary>
    /// Gets the position of column to sort by.
    /// </summary>
    /// <value>The position.</value>
    public int Position
    {
      get { return position; }
    }

    /// <summary>
    /// Gets a value indicating whether this <see cref="SqlOrder"/> is ascending.
    /// </summary>
    /// <value><see langword="true"/> if ascending; otherwise, <see langword="false"/>.</value>
    public bool Ascending
    {
      get { return ascending; }
    }

    internal override object Clone(SqlNodeCloneContext context)
    {
      if (context.NodeMapping.ContainsKey(this))
        return context.NodeMapping[this];

      SqlOrder clone;
      if (!SqlExpression.IsNull(expression))
        clone = new SqlOrder((SqlColumn)expression.Clone(context), ascending);
      else
        clone = new SqlOrder(position, ascending);
      context.NodeMapping[this] = clone;
      return clone;
    }

    // Constructor

    internal SqlOrder(SqlExpression expression, bool ascending)
      : base(SqlNodeType.Order)
    {
      this.expression = expression;
      this.ascending = ascending;
    }

    internal SqlOrder(int position, bool ascending) : base(SqlNodeType.Order)
    {
      this.position = position;
      this.ascending = ascending;
    }

    public override void AcceptVisitor(ISqlVisitor visitor)
    {
      visitor.Visit(this);
    }
  }
}

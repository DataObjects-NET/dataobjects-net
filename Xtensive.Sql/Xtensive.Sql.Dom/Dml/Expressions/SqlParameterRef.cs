// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.

using System;
using Xtensive.Core;

namespace Xtensive.Sql.Dom.Dml
{
  /// <summary>
  /// Represents a Sql parameter.
  /// </summary>
  [Serializable]
  public class SqlParameterRef : SqlExpression, ISqlCursorFetchTarget
  {
    private SqlParameter parameter;

    /// <summary>
    /// Gets the <see cref="SqlParameter"/> this instance references.
    /// </summary>
    public SqlParameter Parameter
    {
      get { return parameter; }
    }

    public override void ReplaceWith(SqlExpression expression)
    {
      ArgumentValidator.EnsureArgumentNotNull(expression, "expression");
      ArgumentValidator.EnsureArgumentIs<SqlParameterRef>(expression, "expression");
      SqlParameterRef replacingExpression = expression as SqlParameterRef;
      parameter = replacingExpression.Parameter;
    }

    internal override object Clone(SqlNodeCloneContext context)
    {
      if (context.NodeMapping.ContainsKey(this))
        return context.NodeMapping[this];

      SqlParameterRef clone = new SqlParameterRef(parameter);
      context.NodeMapping[this] = clone;
      return clone;
    }

    public override void AcceptVisitor(ISqlVisitor visitor)
    {
      visitor.Visit(this);
    }

    // Constructor

    internal SqlParameterRef(SqlParameter parameter) : base(SqlNodeType.Parameter)
    {
      this.parameter = parameter;
    }
  }
}

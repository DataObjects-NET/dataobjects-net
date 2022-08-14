// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.06.11

using System;
using Xtensive.Core;

namespace Xtensive.Sql.Dml
{
  /// <summary>
  /// Represents a container that can hold any value.
  /// An attempt to its translation leads to an error.
  /// This class can be used to store temporary values inside <see cref="SqlExpression"/>.
  /// </summary>
  public class SqlContainer : SqlExpression
  {
    /// <summary>
    /// Gets the value contained in this instance.
    /// </summary>
    public object Value { get; private set; }

    public override void ReplaceWith(SqlExpression expression)
    {
      var replacingExpression = ArgumentValidator.EnsureArgumentIs<SqlContainer>(expression);
      Value = replacingExpression.Value;
    }

    internal override object Clone(SqlNodeCloneContext context) =>
      context.NodeMapping.TryGetValue(this, out var clone)
        ? clone
        : context.NodeMapping[this] = new SqlContainer(Value);

    public override void AcceptVisitor(ISqlVisitor visitor)
    {
      throw new NotSupportedException();
    }

    internal SqlContainer(object value)
      : base(SqlNodeType.Container)
    {
      Value = value;
    }
  }
}
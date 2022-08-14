// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2009.09.01

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Xtensive.Core;

namespace Xtensive.Sql.Dml
{
  [Serializable]
  public class SqlConcat : SqlExpressionList
  {
    internal override object Clone(SqlNodeCloneContext context)
    {
      if (context.NodeMapping.TryGetValue(this, out var value)) {
        return value;
      }

      var expressionsClone = new List<SqlExpression>();
      foreach (var e in expressions)
        expressionsClone.Add((SqlExpression) e.Clone(context));

      var clone = new SqlConcat(expressionsClone);
      return clone;
    }

    public override void ReplaceWith(SqlExpression expression)
    {
      var replacingExpression = ArgumentValidator.EnsureArgumentIs<SqlConcat>(expression);
      expressions.Clear();
      foreach (var e in replacingExpression)
        expressions.Add(e);
    }
    
    public override void AcceptVisitor(ISqlVisitor visitor)
    {
      visitor.Visit(this);
    }


    // Constructors

    internal SqlConcat(IList<SqlExpression> expressions)
      : base(SqlNodeType.Concat, expressions)
    {
    }
  }
}
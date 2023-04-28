// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2009.05.06

using System;
using System.Linq.Expressions;

namespace Xtensive.Orm.Linq.Expressions
{
  internal abstract class ExtendedExpression : Expression
  {
    private Type type;

    public ExtendedExpressionType ExtendedType { get; private set; }

    public sealed override ExpressionType NodeType => (ExpressionType) ExtendedType;

    public override Type Type => type;

    // Constructors

    protected ExtendedExpression(ExtendedExpressionType expressionType, Type type)
      : base()
    {
      this.type = type;
      ExtendedType = expressionType;
    }
  }
}
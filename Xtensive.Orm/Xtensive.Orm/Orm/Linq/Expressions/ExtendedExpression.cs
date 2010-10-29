// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2009.05.06

using System;
using System.Diagnostics;
using System.Linq.Expressions;

namespace Xtensive.Orm.Linq.Expressions
{
  internal abstract class ExtendedExpression : Expression
  {
    public ExtendedExpressionType ExtendedType { get; private set; }


    // Constructors

    protected ExtendedExpression(ExtendedExpressionType expressionType, Type type)
#pragma warning disable 612,618
      : base((ExpressionType) expressionType, type)
#pragma warning restore 612,618
    {
      ExtendedType = expressionType;
    }
  }
}
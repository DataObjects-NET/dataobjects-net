// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2008.12.02

using System;
using System.Linq.Expressions;

namespace Xtensive.Storage.Rse.Compilation.Expressions
{
  public abstract class RseExpression : Expression
  {
    public new RseExpressionType NodeType
    {
      get { return (RseExpressionType) base.NodeType; }
    }

    protected RseExpression(RseExpressionType nodeType, Type type)
      : base((ExpressionType)nodeType, type)
    {
    }
  }
}
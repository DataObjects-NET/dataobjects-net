// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2008.12.02

using System;
using System.Linq.Expressions;

namespace Xtensive.Storage.Linq.Expressions
{
  [Serializable]
  public abstract class ExtendedExpression : Expression
  {
    public new ExtendedExpressionType NodeType
    {
      get { return (ExtendedExpressionType) base.NodeType; }
    }


    // Constructors

    protected ExtendedExpression(ExtendedExpressionType nodeType, Type type)
      : base((ExpressionType)nodeType, type)
    {
    }
  }
}
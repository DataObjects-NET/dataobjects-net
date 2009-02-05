// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2009.02.05

using System;
using System.Linq.Expressions;

namespace Xtensive.Storage.Linq
{
  [Serializable]
  internal sealed class MemberPathExpression : Expression 
  {
    public MemberPath Path { get; private set; }
    public Expression Expression { get; private set; }

    public MemberPathExpression(MemberPath path, Expression expression)
      : base((ExpressionType)ExtendedExpressionType.MemberPath, expression.Type)
    {
      Path = path;
      Expression = expression;
    }
  }
}
// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2008.11.11

using System;
using System.Linq.Expressions;

namespace Xtensive.Storage.Rse.Compilation.Expressions
{
  public sealed class FieldAccessExpression : Expression
  {
    public FieldAccessExpression(Type type)
      : base((ExpressionType)ExtendedExpressionType.FieldAccess, type)
    {}
  }
}
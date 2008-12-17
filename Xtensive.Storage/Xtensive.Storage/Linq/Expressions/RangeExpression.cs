// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2008.12.02

using System;

namespace Xtensive.Storage.Linq.Expressions
{
  [Serializable]
  public sealed class RangeExpression : ExtendedExpression
  {
    // Constructors

    public RangeExpression(Type type)
      : base(ExtendedExpressionType.Range, type)
    {
    }
  }
}
// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2008.12.02

using System;
using Xtensive.Storage.Linq.Expressions;

namespace Xtensive.Storage.Rse.Compilation.Expressions
{
  [Serializable]
  public sealed class SeekExpression : ExtendedExpression
  {
    public SeekExpression(Type type)
      : base(ExtendedExpressionType.Seek, type)
    {
    }
  }
}
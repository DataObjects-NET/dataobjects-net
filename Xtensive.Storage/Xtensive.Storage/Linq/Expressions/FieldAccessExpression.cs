// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2008.11.11

using System;
using System.Linq.Expressions;
using Xtensive.Storage.Model;

namespace Xtensive.Storage.Linq.Expressions
{
  public sealed class FieldAccessExpression : ExtendedExpression
  {
    public FieldInfo Field { get; private set; }

    public override string ToString()
    {
      return string.Format("Field[{0}]", Field.Name);
    }


    public FieldAccessExpression(Type type, FieldInfo field)
      : base(ExtendedExpressionType.FieldAccess, type)
    {
      Field = field;
    }
  }
}
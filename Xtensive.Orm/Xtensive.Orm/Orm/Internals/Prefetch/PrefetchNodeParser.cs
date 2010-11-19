// Copyright (C) 2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2010.11.19

using System;
using System.Diagnostics;
using System.Linq.Expressions;

namespace Xtensive.Orm.Internals.Prefetch
{
  [Serializable]
  internal class PrefetchNodeParser : ExpressionVisitor
  {
    public static PrefetchNode Parse<T,TValue>(Expression<Func<T, TValue>> expression)
    {
      throw new NotImplementedException();
    }
  }
}
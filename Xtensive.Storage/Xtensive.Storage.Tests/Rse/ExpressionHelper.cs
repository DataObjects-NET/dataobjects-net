// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexander Nikolaev
// Created:    2009.03.26

using System;
using System.Linq.Expressions;
using Xtensive.Core.Linq.Normalization;
using Xtensive.Core.Tuples;

namespace Xtensive.Storage.Tests.Rse
{
  public static class ExpressionHelper
  {
    public static Conjunction<Expression> AddBoolean(this Conjunction<Expression> exp,
      Expression<Func<Tuple, bool>> boolean)
    {
      exp.Operands.Add(boolean.Body);
      return exp;
    }

    public static DisjunctiveNormalized AddCnf(this DisjunctiveNormalized root,
     Conjunction<Expression> exp)
    {
      root.Operands.Add(exp);
      return root;
    }
  }
}
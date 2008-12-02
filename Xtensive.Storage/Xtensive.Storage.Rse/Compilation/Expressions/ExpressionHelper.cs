// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2008.12.02

using System.Linq;
using System.Linq.Expressions;
using Xtensive.Core.Reflection;

namespace Xtensive.Storage.Rse.Compilation.Expressions
{
  public static class ExpressionHelper
  {
    public static Expression StripQuotes(this Expression expression)
    {
      while (expression.NodeType == ExpressionType.Quote)
        expression = ((UnaryExpression)expression).Operand;
      return expression;
    }

    public static bool IsQuery(this Expression expression)
    {
      return expression.Type.IsOfGenericType(typeof(IQueryable<>));
    }

  }
}
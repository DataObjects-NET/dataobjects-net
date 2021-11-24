// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2009.05.06

using System.Collections.Generic;
using System.Linq.Expressions;
using Xtensive.Core;

namespace Xtensive.Orm.Linq.Expressions
{
  internal interface IMappedExpression
  {
    Expression BindParameter(ParameterExpression parameter, Dictionary<Expression, Expression> processedExpressions);
    Expression RemoveOuterParameter(Dictionary<Expression, Expression> processedExpressions);
    Expression Remap(int offset, Dictionary<Expression, Expression> processedExpressions);
    Expression Remap(IReadOnlyList<int> map, Dictionary<Expression, Expression> processedExpressions);
  }
}
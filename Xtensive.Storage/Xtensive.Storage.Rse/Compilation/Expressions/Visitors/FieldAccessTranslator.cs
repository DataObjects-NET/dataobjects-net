// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2008.11.11

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Xtensive.Core.Tuples;

namespace Xtensive.Storage.Rse.Compilation.Expressions.Visitors
{
  public class FieldAccessTranslator : ExpressionVisitor
  {
    private readonly HashSet<Expression> candidates;

    public static Expression Translate(Expression expression, HashSet<Expression> candidates)
    {
      var fat = new FieldAccessTranslator(candidates);
      return fat.Visit(expression);
    }

    protected override Expression VisitMethodCall(MethodCallExpression expression)
    {
      if (expression.Object.Type == typeof(Tuple)) {
        if (expression.Method.Name == "GetValue" || expression.Method.Name == "GetValueOrDefault") {
          var type = expression.Method.ReturnType;
          var columnArgument = expression.Arguments[0];
          int columnIndex;
          if (columnArgument.NodeType == ExpressionType.Constant)
            columnIndex = (int)((ConstantExpression)columnArgument).Value;
          else {
            var columnFunc = Expression.Lambda<Func<int>>(columnArgument).Compile();
            columnIndex = columnFunc();
          }
          return new FieldAccessExpression(type, columnIndex);
        }
      }
      return base.VisitMethodCall(expression);
    }

    protected override Expression VisitUnknown(Expression expression)
    {
      return expression;
    }
    

    // Constructor

    private FieldAccessTranslator(HashSet<Expression> candidates)
    {
      this.candidates = candidates;
    }
  }
}
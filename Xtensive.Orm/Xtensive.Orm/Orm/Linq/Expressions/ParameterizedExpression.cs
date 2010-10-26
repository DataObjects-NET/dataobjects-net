// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2009.05.18

using System;
using System.Linq.Expressions;

namespace Xtensive.Orm.Linq.Expressions
{
  internal abstract class ParameterizedExpression : ExtendedExpression
  {
    public ParameterExpression OuterParameter { get; private set; }
    public bool DefaultIfEmpty { get; set; }

    public override string ToString()
    {
      return ExtendedType.ToString();
    }

    /// <summary>
    /// Check if <see cref="ParameterizedExpression"/> can be remapped 
    /// according to current <see cref="RemapContext"/>.
    /// </summary>
    protected bool CanRemap
    {
      get
      {
        var context = RemapScope.CurrentContext;
        return context.SubqueryParameterExpression==null 
          ? OuterParameter==null 
          : OuterParameter==context.SubqueryParameterExpression;
      }
    }

    // Constructors

    protected ParameterizedExpression(ExtendedExpressionType expressionType, Type type, ParameterExpression parameterExpression, bool defaultIfEmpty)
      : base(expressionType, type)
    {
      OuterParameter = parameterExpression;
      DefaultIfEmpty = defaultIfEmpty;
    }
  }
}
// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2009.02.25

using System;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using Xtensive.Core.Internals.DocTemplates;
using Xtensive.Core.Reflection;

namespace Xtensive.Storage.Linq
{
  [Serializable]
  internal sealed class QueryableMethodCall : Expression
  {
    public QueryableMethodKind MethodKind { get; private set; }
    public MethodCallExpression Expression { get; private set; }


    // Constructors
    
    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    public QueryableMethodCall(QueryableMethodKind methodKind, MethodCallExpression expression)
      : base((ExpressionType)ExtendedExpressionType.QueryableMethod, expression.Type)
    {
      MethodKind = methodKind;
      Expression = expression;
    }
  }
}
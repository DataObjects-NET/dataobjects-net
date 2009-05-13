// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.05.13

using System;
using System.Linq;
using System.Linq.Expressions;
using ExpressionFactory = System.Linq.Expressions.Expression;

namespace Xtensive.Core.Linq.SerializableExpressions
{
  /// <summary>
  /// A serializable representation of <see cref="InvocationExpression"/>.
  /// </summary>
  [Serializable]
  public sealed class SerializableInvocationExpression : SerializableExpression
  {
    /// <summary>
    /// <see cref="InvocationExpression.Expression"/>
    /// </summary>
    public SerializableExpression Expression;
    /// <summary>
    /// <see cref="InvocationExpression.Arguments"/>
    /// </summary>
    public SerializableExpression[] Arguments;
  }
}
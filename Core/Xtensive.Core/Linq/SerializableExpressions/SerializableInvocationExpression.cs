// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.05.13

using System;
using System.Linq.Expressions;

namespace Xtensive.Linq.SerializableExpressions
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
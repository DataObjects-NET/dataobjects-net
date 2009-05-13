// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.05.12

using System;
using System.Linq;
using System.Linq.Expressions;

namespace Xtensive.Core.Linq.SerializableExpressions
{
  /// <summary>
  /// A serializable version of <see cref="LambdaExpression"/>.
  /// </summary>
  [Serializable]
  public sealed class SerializableLambdaExpression : SerializableExpression
  {
    /// <summary>
    /// A type of delegate that will of this expression.
    /// </summary>
    public Type DelegateType;

    /// <summary>
    /// <see cref="LambdaExpression.Body"/>.
    /// </summary>
    public SerializableExpression Body;

    /// <summary>
    /// <see cref="LambdaExpression.Parameters"/>.
    /// </summary>
    public SerializableParameterExpression[] Parameters;
  }
}
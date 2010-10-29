// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.05.12

using System;
using System.Linq.Expressions;

namespace Xtensive.Linq.SerializableExpressions
{
  /// <summary>
  /// A serializable representation of <see cref="LambdaExpression"/>.
  /// </summary>
  [Serializable]
  public sealed class SerializableLambdaExpression : SerializableExpression
  {
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
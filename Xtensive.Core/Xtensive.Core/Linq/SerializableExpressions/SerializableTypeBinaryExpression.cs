// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.05.12

using System;
using System.Linq.Expressions;
using ExpressionFactory = System.Linq.Expressions.Expression;

namespace Xtensive.Core.Linq.SerializableExpressions
{
  /// <summary>
  /// A serializable representation of <see cref="TypeBinaryExpression"/>.
  /// </summary>
  [Serializable]
  public sealed class SerializableTypeBinaryExpression : SerializableExpression
  {
    /// <summary>
    /// <see cref="TypeBinaryExpression.Expression"/>
    /// </summary>
    public SerializableExpression Expression;
    /// <summary>
    /// <see cref="TypeBinaryExpression.TypeOperand"/>
    /// </summary>
    public Type TypeOperand;
  }
}
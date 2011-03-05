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
  /// A serializable representation of <see cref="ConditionalExpression"/>.
  /// </summary>
  [Serializable]
  public sealed class SerializableConditionalExpression : SerializableExpression
  {
    /// <summary>
    /// <see cref="ConditionalExpression.Test"/>
    /// </summary>
    public SerializableExpression Test;
    /// <summary>
    /// <see cref="ConditionalExpression.IfTrue"/>
    /// </summary>
    public SerializableExpression IfTrue;
    /// <summary>
    /// <see cref="ConditionalExpression.IfFalse"/>
    /// </summary>
    public SerializableExpression IfFalse;
  }
}
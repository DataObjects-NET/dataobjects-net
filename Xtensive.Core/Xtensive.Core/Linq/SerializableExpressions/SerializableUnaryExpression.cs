// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.05.12

using System;
using System.Linq.Expressions;
using System.Reflection;

namespace Xtensive.Core.Linq.SerializableExpressions
{
  /// <summary>
  /// A serializable representation of <see cref="UnaryExpression"/>.
  /// </summary>
  [Serializable]
  public sealed class SerializableUnaryExpression : SerializableExpression
  {
    /// <summary>
    /// <see cref="UnaryExpression.Operand"/>
    /// </summary>
    public SerializableExpression Operand;
    /// <summary>
    /// <see cref="UnaryExpression.Method"/>
    /// </summary>
    public MethodInfo Method;
  }
}
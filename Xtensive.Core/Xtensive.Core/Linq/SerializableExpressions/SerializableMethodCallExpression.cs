// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.05.12

using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Xtensive.Core.Linq.SerializableExpressions
{
  /// <summary>
  /// A serializable representation of <see cref="MethodCallExpression"/>.
  /// </summary>
  [Serializable]
  public sealed class SerializableMethodCallExpression : SerializableExpression
  {
    /// <summary>
    /// <see cref="MethodCallExpression.Arguments"/>
    /// </summary>
    public SerializableExpression[] Arguments;
    /// <summary>
    /// <see cref="MethodCallExpression.Method"/>
    /// </summary>
    public MethodInfo Method;
    /// <summary>
    /// <see cref="MethodCallExpression.Object"/>
    /// </summary>
    public SerializableExpression Object;
  }
}
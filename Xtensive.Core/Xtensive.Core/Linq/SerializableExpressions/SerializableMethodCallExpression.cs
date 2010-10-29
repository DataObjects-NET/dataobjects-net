// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.05.12

using System;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.Serialization;
using Xtensive.Linq.SerializableExpressions.Internals;

namespace Xtensive.Linq.SerializableExpressions
{
  /// <summary>
  /// A serializable representation of <see cref="MethodCallExpression"/>.
  /// </summary>
  [Serializable]
  public sealed class SerializableMethodCallExpression : SerializableExpression
  {
    private string methodName;

    /// <summary>
    /// <see cref="MethodCallExpression.Arguments"/>
    /// </summary>
    public SerializableExpression[] Arguments;
    /// <summary>
    /// <see cref="MethodCallExpression.Method"/>
    /// </summary>
    [NonSerialized]
    public MethodInfo Method;

    /// <summary>
    /// <see cref="MethodCallExpression.Object"/>
    /// </summary>
    public SerializableExpression Object;

    [OnSerializing]
    private void OnSerializing(StreamingContext context)
    {
      methodName = Method.ToSerializableForm();
    }

    [OnDeserialized]
    private void OnDeserialized(StreamingContext context)
    {
      Method = methodName.GetMethodFromSerializableForm();
    }
  }
}
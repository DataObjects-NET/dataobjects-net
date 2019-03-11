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

    public override void GetObjectData(SerializationInfo info, StreamingContext context)
    {
      base.GetObjectData(info, context);
      info.AddArray("Arguments", Arguments);
      info.AddValue("Method", Method.ToSerializableForm());
      info.AddValue("Object", Object);
    }

    public SerializableMethodCallExpression()
    {
    }

    public SerializableMethodCallExpression(SerializationInfo info, StreamingContext context)
      : base(info, context)
    {
      Arguments = info.GetArrayFromSerializableForm<SerializableExpression>("Arguments");
      Method = info.GetString("Method").GetMethodFromSerializableForm();
      Object = (SerializableExpression) info.GetValue("Object", typeof (SerializableExpression));
    }
  }
}
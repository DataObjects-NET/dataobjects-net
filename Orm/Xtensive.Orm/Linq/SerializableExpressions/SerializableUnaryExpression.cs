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
  /// A serializable representation of <see cref="UnaryExpression"/>.
  /// </summary>
  [Serializable]
  public sealed class SerializableUnaryExpression : SerializableExpression
  {
    //private string methodName;

    /// <summary>
    /// <see cref="UnaryExpression.Operand"/>
    /// </summary>
    public SerializableExpression Operand;
    /// <summary>
    /// <see cref="UnaryExpression.Method"/>
    /// </summary>
    [NonSerialized]
    public MethodInfo Method;

    public override void GetObjectData(SerializationInfo info, StreamingContext context)
    {
      base.GetObjectData(info, context);
      info.AddValue("Operand", Operand);
      info.AddValue("Method", Method.ToSerializableForm());
    }

    public SerializableUnaryExpression()
    {
    }

    public SerializableUnaryExpression(SerializationInfo info, StreamingContext context)
      : base(info, context)
    {
      Operand = (SerializableExpression) info.GetValue("Operand", typeof (SerializableExpression));
      Method = info.GetString("Method").GetMethodFromSerializableForm();
    }
  }
}
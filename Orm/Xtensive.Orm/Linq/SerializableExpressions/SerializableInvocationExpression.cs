// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.05.13

using System;
using System.Linq.Expressions;
using System.Runtime.Serialization;
using Xtensive.Linq.SerializableExpressions.Internals;

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

    public override void GetObjectData(SerializationInfo info, StreamingContext context)
    {
      base.GetObjectData(info, context);
      info.AddValue("Expression", Expression);
      info.AddArray("Arguments", Arguments);
    }

    public SerializableInvocationExpression()
    {
    }

    public SerializableInvocationExpression(SerializationInfo info, StreamingContext context)
      : base(info, context)
    {
      Expression = (SerializableExpression) info.GetValue("Expression", typeof (SerializableExpression));
      Arguments = info.GetArrayFromSerializableForm<SerializableExpression>("Arguments");
    }
  }
}
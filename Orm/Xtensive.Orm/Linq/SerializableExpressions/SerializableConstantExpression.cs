// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.05.12

using System;
using System.Linq.Expressions;
using System.Runtime.Serialization;

namespace Xtensive.Linq.SerializableExpressions
{
  /// <summary>
  /// A serializable representation of <see cref="ConstantExpression"/>.
  /// </summary>
  [Serializable]
  public sealed class SerializableConstantExpression : SerializableExpression
  {
    /// <summary>
    /// <see cref="ConstantExpression.Value"/>
    /// </summary>
    public object Value;

    public override void GetObjectData(SerializationInfo info, StreamingContext context)
    {
      base.GetObjectData(info, context);
      info.AddValue("Value", Value);
    }


    public SerializableConstantExpression()
    {
    }

    public SerializableConstantExpression(SerializationInfo info, StreamingContext context)
      : base(info, context)
    {
      Value = info.GetValue("Value", typeof(object));
    }
  }
}
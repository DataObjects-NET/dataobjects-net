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
  /// A serializable representation of <see cref="NewArrayExpression"/>
  /// </summary>
  [Serializable]
  public sealed class SerializableNewArrayExpression : SerializableExpression
  {
    /// <summary>
    /// <see cref="NewArrayExpression.Expressions"/>
    /// </summary>
    public SerializableExpression[] Expressions;

    public override void GetObjectData(SerializationInfo info, StreamingContext context)
    {
      base.GetObjectData(info, context);
      info.AddArray("Expressions", Expressions);
    }

    public SerializableNewArrayExpression()
    {
    }

    public SerializableNewArrayExpression(SerializationInfo info, StreamingContext context)
      : base(info, context)
    {
      Expressions = info.GetArrayFromSerializableForm<SerializableExpression>("Expressions");
    }
  }
}
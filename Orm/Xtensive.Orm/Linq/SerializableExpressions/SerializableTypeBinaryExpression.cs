// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.05.12

using System;
using System.Linq.Expressions;
using System.Runtime.Serialization;
using Xtensive.Linq.SerializableExpressions.Internals;
using ExpressionFactory = System.Linq.Expressions.Expression;

namespace Xtensive.Linq.SerializableExpressions
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
    [NonSerialized]
    public Type TypeOperand;

    public override void GetObjectData(SerializationInfo info, StreamingContext context)
    {
      base.GetObjectData(info, context);
      info.AddValue("Expression", Expression);
      info.AddValue("TypeOperand", TypeOperand.ToSerializableForm());
    }

    public SerializableTypeBinaryExpression()
    {
    }

    public SerializableTypeBinaryExpression(SerializationInfo info, StreamingContext context)
      : base(info, context)
    {
      Expression = (SerializableExpression) info.GetValue("Expression", typeof (SerializableExpression));
      TypeOperand = info.GetString("TypeOperand").GetTypeFromSerializableForm();
    }
  }
}
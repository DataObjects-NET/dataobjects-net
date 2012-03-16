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
    private string typeOperandName;

    /// <summary>
    /// <see cref="TypeBinaryExpression.Expression"/>
    /// </summary>
    public SerializableExpression Expression;
    /// <summary>
    /// <see cref="TypeBinaryExpression.TypeOperand"/>
    /// </summary>
    [NonSerialized]
    public Type TypeOperand;

    [OnSerializing]
    private void OnSerializing(StreamingContext context)
    {
      typeOperandName = TypeOperand.ToSerializableForm();
    }

    [OnDeserialized]
    private void OnDeserialized(StreamingContext context)
    {
      TypeOperand = typeOperandName.GetTypeFromSerializableForm();
    }
  }
}
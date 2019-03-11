// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.05.12

using System;
using System.Linq.Expressions;
using System.Runtime.Serialization;
using Xtensive.Linq.SerializableExpressions.Internals;

namespace Xtensive.Linq.SerializableExpressions
{
  /// <summary>
  /// A serializable representation of <see cref="Expression"/>.
  /// </summary>
  [Serializable]
  public abstract class SerializableExpression : ISerializable
  {
    /// <summary>
    /// <see cref="Expression.NodeType"/>.
    /// </summary>
    public ExpressionType NodeType;
    /// <summary>
    /// <see cref="Expression.Type"/>.
    /// </summary>
    [NonSerialized]
    public Type Type;

    public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
    {
      info.AddValue("NodeType", NodeType.ToString());
      info.AddValue("Type", Type.ToSerializableForm());
    }

    protected SerializableExpression()
    {
    }

    protected SerializableExpression(SerializationInfo info, StreamingContext context)
    {
      NodeType = (ExpressionType) Enum.Parse(typeof (ExpressionType), info.GetString("NodeType"));
      Type = info.GetString("Type").GetTypeFromSerializableForm();
    }
  }
}
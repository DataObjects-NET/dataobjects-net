// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.05.15

using System;
using System.Linq.Expressions;
using System.Runtime.Serialization;

namespace Xtensive.Linq.SerializableExpressions
{
  /// <summary>
  /// A serializable representation of <see cref="MemberAssignment"/>
  /// </summary>
  [Serializable]
  public class SerializableMemberAssignment : SerializableMemberBinding
  {
    /// <summary>
    /// <see cref="MemberAssignment.Expression"/>
    /// </summary>
    public SerializableExpression Expression;

    public override void GetObjectData(SerializationInfo info, StreamingContext context)
    {
      base.GetObjectData(info, context);
      info.AddValue("Expression", Expression);
    }

    public SerializableMemberAssignment()
    {
    }

    public SerializableMemberAssignment(SerializationInfo info, StreamingContext context)
      : base(info, context)
    {
      Expression = (SerializableExpression) info.GetValue("Expression", typeof (SerializableExpression));
    }
  }
}
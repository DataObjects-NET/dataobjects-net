// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.05.12

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.Serialization;
using Xtensive.Linq.SerializableExpressions.Internals;

namespace Xtensive.Linq.SerializableExpressions
{
  /// <summary>
  /// A serializable representation of <see cref="MemberExpression"/>.
  /// </summary>
  [Serializable]
  public sealed class SerializableMemberExpression : SerializableExpression
  {
    /// <summary>
    /// <see cref="MemberExpression.Expression"/>
    /// </summary>
    public SerializableExpression Expression;
    /// <summary>
    /// <see cref="MemberExpression.Member"/>
    /// </summary>
    [NonSerialized]
    public MemberInfo Member;

    public override void GetObjectData(SerializationInfo info, StreamingContext context)
    {
      base.GetObjectData(info, context);
      info.AddValue("Expression", Expression);
      info.AddValue("Member", Member.ToSerializableForm());
    }

    public SerializableMemberExpression()
    {
    }

    public SerializableMemberExpression(SerializationInfo info, StreamingContext context)
      : base(info, context)
    {
      Expression = (SerializableExpression) info.GetValue("Expression", typeof (SerializableExpression));
      Member = info.GetString("Member").GetMemberFromSerializableForm();
    }
  }
}
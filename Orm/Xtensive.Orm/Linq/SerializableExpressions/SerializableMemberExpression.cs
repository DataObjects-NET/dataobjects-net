// Copyright (C) 2009-2021 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Denis Krjuchkov
// Created:    2009.05.12

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.Serialization;
using System.Security;
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
    public MemberInfo Member;

    [SecurityCritical]
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
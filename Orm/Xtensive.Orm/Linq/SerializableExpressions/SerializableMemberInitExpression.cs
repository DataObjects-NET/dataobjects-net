// Copyright (C) 2009-2021 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Denis Krjuchkov
// Created:    2009.05.13

using System;
using System.Linq.Expressions;
using System.Runtime.Serialization;
using System.Security;
using Xtensive.Linq.SerializableExpressions.Internals;

namespace Xtensive.Linq.SerializableExpressions
{
  /// <summary>
  /// A serializable representation of <see cref="MemberInitExpression"/>
  /// </summary>
  [Serializable]
  public sealed class SerializableMemberInitExpression : SerializableExpression
  {
    /// <summary>
    /// <see cref="MemberInitExpression.NewExpression"/>
    /// </summary>
    public SerializableNewExpression NewExpression;
    /// <summary>
    /// <see cref="MemberInitExpression.Bindings"/>
    /// </summary>
    public SerializableMemberBinding[] Bindings;

    [SecurityCritical]
    public override void GetObjectData(SerializationInfo info, StreamingContext context)
    {
      base.GetObjectData(info, context);
      info.AddValue("NewExpression", NewExpression);
      info.AddArray("Bindings", Bindings);
    }

    public SerializableMemberInitExpression()
    {
    }

    public SerializableMemberInitExpression(SerializationInfo info, StreamingContext context)
      : base(info, context)
    {
      NewExpression = (SerializableNewExpression) info.GetValue("NewExpression", typeof (SerializableNewExpression));
      Bindings = info.GetArrayFromSerializableForm<SerializableMemberBinding>("Bindings");
    }
  }
}
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

    [SecurityCritical]
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
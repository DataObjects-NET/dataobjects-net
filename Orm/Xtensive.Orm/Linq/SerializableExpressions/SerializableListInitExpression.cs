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
  /// A serializable representation of <see cref="ListInitExpression"/>.
  /// </summary>
  [Serializable]
  public sealed class SerializableListInitExpression : SerializableExpression
  {
    /// <summary>
    /// <see cref="ListInitExpression.NewExpression"/>
    /// </summary>
    public SerializableNewExpression NewExpression;
    /// <summary>
    /// <see cref="ListInitExpression.Initializers"/>
    /// </summary>
    public SerializableElementInit[] Initializers;

    [SecurityCritical]
    public override void GetObjectData(SerializationInfo info, StreamingContext context)
    {
      base.GetObjectData(info, context);
      info.AddValue("NewExpression",NewExpression);
      info.AddArray("Initializers", Initializers);
    }

    public SerializableListInitExpression()
    {
    }

    public SerializableListInitExpression(SerializationInfo info, StreamingContext context)
      : base(info, context)
    {
      NewExpression = (SerializableNewExpression) info.GetValue("NewExpression", typeof (SerializableNewExpression));
      Initializers = info.GetArrayFromSerializableForm<SerializableElementInit>("Initializers");
    }
  }
}
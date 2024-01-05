// Copyright (C) 2009-2021 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Denis Krjuchkov
// Created:    2009.05.12

using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.Serialization;
using System.Security;
using Xtensive.Linq.SerializableExpressions.Internals;

namespace Xtensive.Linq.SerializableExpressions
{
  /// <summary>
  /// A serializable representation of <see cref="BinaryExpression"/>.
  /// </summary>
  [Serializable]
  public sealed class SerializableBinaryExpression : SerializableExpression
  {
    /// <summary>
    /// <see cref="BinaryExpression.IsLiftedToNull"/>
    /// </summary>
    public bool IsLiftedToNull;
    /// <summary>
    /// <see cref="BinaryExpression.Left"/>.
    /// </summary>
    public SerializableExpression Left;
    /// <summary>
    /// <see cref="BinaryExpression.Right"/>
    /// </summary>
    public SerializableExpression Right;
    /// <summary>
    /// <see cref="BinaryExpression.Method"/>
    /// </summary>
    public MethodInfo Method;

    [SecurityCritical]
    public override void GetObjectData(SerializationInfo info, StreamingContext context)
    {
      base.GetObjectData(info, context);
      info.AddValue("IsLiftedToNull", IsLiftedToNull);
      info.AddValue("Left", Left);
      info.AddValue("Right", Right);
      info.AddValue("Method", Method.ToSerializableForm());
    }

    public SerializableBinaryExpression()
    {
    }

    public SerializableBinaryExpression(SerializationInfo info, StreamingContext context)
      : base(info, context)
    {
      IsLiftedToNull = info.GetBoolean("IsLiftedToNull");
      Left = (SerializableExpression) info.GetValue("Left", typeof (SerializableExpression));
      Right = (SerializableExpression) info.GetValue("Right", typeof (SerializableExpression));
      Method = info.GetString("Method").GetMethodFromSerializableForm();
    }
  }
}
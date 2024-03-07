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
  /// A serializable representation of <see cref="UnaryExpression"/>.
  /// </summary>
  [Serializable]
  public sealed class SerializableUnaryExpression : SerializableExpression
  {
    /// <summary>
    /// <see cref="UnaryExpression.Operand"/>
    /// </summary>
    public SerializableExpression Operand;
    /// <summary>
    /// <see cref="UnaryExpression.Method"/>
    /// </summary>
    public MethodInfo Method;

    [SecurityCritical]
    public override void GetObjectData(SerializationInfo info, StreamingContext context)
    {
      base.GetObjectData(info, context);
      info.AddValue("Operand", Operand);
      info.AddValue("Method", Method.ToSerializableForm());
    }

    public SerializableUnaryExpression()
    {
    }

    public SerializableUnaryExpression(SerializationInfo info, StreamingContext context)
      : base(info, context)
    {
      Operand = (SerializableExpression) info.GetValue("Operand", typeof (SerializableExpression));
      Method = info.GetString("Method").GetMethodFromSerializableForm();
    }
  }
}
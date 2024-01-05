// Copyright (C) 2009-2021 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Denis Krjuchkov
// Created:    2009.05.12

using System.Linq.Expressions;
using System.Runtime.Serialization;
using System.Security;
using Xtensive.Reflection;

namespace Xtensive.Linq.SerializableExpressions
{
  /// <summary>
  /// A serializable representation of <see cref="ConstantExpression"/>.
  /// </summary>
  [Serializable]
  public sealed class SerializableConstantExpression : SerializableExpression
  {
    /// <summary>
    /// <see cref="ConstantExpression.Value"/>
    /// </summary>
    public object Value;

    [SecurityCritical]
    public override void GetObjectData(SerializationInfo info, StreamingContext context)
    {
      base.GetObjectData(info, context);
      info.AddValue("Value", Value);
    }


    public SerializableConstantExpression()
    {
    }

    public SerializableConstantExpression(SerializationInfo info, StreamingContext context)
      : base(info, context)
    {
      Value = info.GetValue("Value", WellKnownTypes.Object);
    }
  }
}

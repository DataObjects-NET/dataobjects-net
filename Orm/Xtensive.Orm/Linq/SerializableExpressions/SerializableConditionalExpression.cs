// Copyright (C) 2009-2021 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Denis Krjuchkov
// Created:    2009.05.12

using System.Linq.Expressions;
using System.Runtime.Serialization;
using System.Security;

namespace Xtensive.Linq.SerializableExpressions
{
  /// <summary>
  /// A serializable representation of <see cref="ConditionalExpression"/>.
  /// </summary>
  [Serializable]
  public sealed class SerializableConditionalExpression : SerializableExpression
  {
    /// <summary>
    /// <see cref="ConditionalExpression.Test"/>
    /// </summary>
    public SerializableExpression Test;
    /// <summary>
    /// <see cref="ConditionalExpression.IfTrue"/>
    /// </summary>
    public SerializableExpression IfTrue;
    /// <summary>
    /// <see cref="ConditionalExpression.IfFalse"/>
    /// </summary>
    public SerializableExpression IfFalse;

    [SecurityCritical]
    public override void GetObjectData(SerializationInfo info, StreamingContext context)
    {
      base.GetObjectData(info, context);
      info.AddValue("Test", Test);
      info.AddValue("IfTrue", IfTrue);
      info.AddValue("IfFalse", IfFalse);
    }

    public SerializableConditionalExpression()
    {
    }

    public SerializableConditionalExpression(SerializationInfo info, StreamingContext context)
      : base(info, context)
    {
      Test = (SerializableExpression) info.GetValue("Test", typeof (SerializableExpression));
      IfTrue = (SerializableExpression) info.GetValue("IfTrue", typeof (SerializableExpression));
      IfFalse = (SerializableExpression) info.GetValue("IfFalse", typeof (SerializableExpression));
    }
  }
}
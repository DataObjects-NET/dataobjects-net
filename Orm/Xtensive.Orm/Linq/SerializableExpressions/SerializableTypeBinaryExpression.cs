// Copyright (C) 2009-2021 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Denis Krjuchkov
// Created:    2009.05.12

using System.Linq.Expressions;
using System.Runtime.Serialization;
using System.Security;
using Xtensive.Linq.SerializableExpressions.Internals;
using ExpressionFactory = System.Linq.Expressions.Expression;

namespace Xtensive.Linq.SerializableExpressions
{
  /// <summary>
  /// A serializable representation of <see cref="TypeBinaryExpression"/>.
  /// </summary>
  [Serializable]
  public sealed class SerializableTypeBinaryExpression : SerializableExpression
  {
    /// <summary>
    /// <see cref="TypeBinaryExpression.Expression"/>
    /// </summary>
    public SerializableExpression Expression;
    /// <summary>
    /// <see cref="TypeBinaryExpression.TypeOperand"/>
    /// </summary>
    public Type TypeOperand;

    [SecurityCritical]
    public override void GetObjectData(SerializationInfo info, StreamingContext context)
    {
      base.GetObjectData(info, context);
      info.AddValue("Expression", Expression);
      info.AddValue("TypeOperand", TypeOperand.ToSerializableForm());
    }

    public SerializableTypeBinaryExpression()
    {
    }

    public SerializableTypeBinaryExpression(SerializationInfo info, StreamingContext context)
      : base(info, context)
    {
      Expression = (SerializableExpression) info.GetValue("Expression", typeof (SerializableExpression));
      TypeOperand = info.GetString("TypeOperand").GetTypeFromSerializableForm();
    }
  }
}
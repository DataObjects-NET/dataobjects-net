// Copyright (C) 2009-2021 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Denis Krjuchkov
// Created:    2009.05.15

using System.Linq.Expressions;
using System.Runtime.Serialization;
using System.Security;

namespace Xtensive.Linq.SerializableExpressions
{
  /// <summary>
  /// A serializable representation of <see cref="MemberAssignment"/>
  /// </summary>
  [Serializable]
  public class SerializableMemberAssignment : SerializableMemberBinding
  {
    /// <summary>
    /// <see cref="MemberAssignment.Expression"/>
    /// </summary>
    public SerializableExpression Expression;

    [SecurityCritical]
    public override void GetObjectData(SerializationInfo info, StreamingContext context)
    {
      base.GetObjectData(info, context);
      info.AddValue("Expression", Expression);
    }

    public SerializableMemberAssignment()
    {
    }

    public SerializableMemberAssignment(SerializationInfo info, StreamingContext context)
      : base(info, context)
    {
      Expression = (SerializableExpression) info.GetValue("Expression", typeof (SerializableExpression));
    }
  }
}
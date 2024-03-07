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
  /// A serializable representation of <see cref="ParameterExpression"/>.
  /// </summary>
  [Serializable]
  public sealed class SerializableParameterExpression : SerializableExpression
  {
    /// <summary>
    /// <see cref="ParameterExpression.Name"/>.
    /// </summary>
    public string Name;

    [SecurityCritical]
    public override void GetObjectData(SerializationInfo info, StreamingContext context)
    {
      base.GetObjectData(info, context);
      info.AddValue("Name", Name);
    }

    public SerializableParameterExpression()
    {
    }

    public SerializableParameterExpression(SerializationInfo info, StreamingContext context)
      : base(info, context)
    {
      Name = info.GetString("Name");
    }
  }
}
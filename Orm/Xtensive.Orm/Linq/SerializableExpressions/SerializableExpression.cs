// Copyright (C) 2009-2021 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Denis Krjuchkov
// Created:    2009.05.12

using System.Linq.Expressions;
using System.Runtime.Serialization;
using System.Security;
using Xtensive.Linq.SerializableExpressions.Internals;

namespace Xtensive.Linq.SerializableExpressions
{
  /// <summary>
  /// A serializable representation of <see cref="Expression"/>.
  /// </summary>
  [Serializable]
  public abstract class SerializableExpression : ISerializable
  {
    /// <summary>
    /// <see cref="Expression.NodeType"/>.
    /// </summary>
    public ExpressionType NodeType;
    /// <summary>
    /// <see cref="Expression.Type"/>.
    /// </summary>
    public Type Type;

    [SecurityCritical]
    public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
    {
      info.AddValue("NodeType", NodeType.ToString());
      info.AddValue("Type", Type.ToSerializableForm());
    }

    protected SerializableExpression()
    {
    }

    protected SerializableExpression(SerializationInfo info, StreamingContext context)
    {
      NodeType = (ExpressionType) Enum.Parse(typeof (ExpressionType), info.GetString("NodeType"));
      Type = info.GetString("Type").GetTypeFromSerializableForm();
    }
  }
}
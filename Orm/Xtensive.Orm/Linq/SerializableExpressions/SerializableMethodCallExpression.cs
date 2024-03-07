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
  /// A serializable representation of <see cref="MethodCallExpression"/>.
  /// </summary>
  [Serializable]
  public sealed class SerializableMethodCallExpression : SerializableExpression
  {
    /// <summary>
    /// <see cref="MethodCallExpression.Arguments"/>
    /// </summary>
    public SerializableExpression[] Arguments;
    /// <summary>
    /// <see cref="MethodCallExpression.Method"/>
    /// </summary>
    public MethodInfo Method;

    /// <summary>
    /// <see cref="MethodCallExpression.Object"/>
    /// </summary>
    public SerializableExpression Object;

    [SecurityCritical]
    public override void GetObjectData(SerializationInfo info, StreamingContext context)
    {
      base.GetObjectData(info, context);
      info.AddArray("Arguments", Arguments);
      info.AddValue("Method", Method.ToSerializableForm());
      info.AddValue("Object", Object);
    }

    public SerializableMethodCallExpression()
    {
    }

    public SerializableMethodCallExpression(SerializationInfo info, StreamingContext context)
      : base(info, context)
    {
      Arguments = info.GetArrayFromSerializableForm<SerializableExpression>("Arguments");
      Method = info.GetString("Method").GetMethodFromSerializableForm();
      Object = (SerializableExpression) info.GetValue("Object", typeof (SerializableExpression));
    }
  }
}
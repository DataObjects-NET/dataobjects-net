// Copyright (C) 2009-2021 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Denis Kryuchkov
// Created:    2009.05.14

using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.Serialization;
using System.Security;
using Xtensive.Linq.SerializableExpressions.Internals;

namespace Xtensive.Linq.SerializableExpressions
{
  /// <summary>
  /// A serializable representation of <see cref="ElementInit"/>.
  /// </summary>
  [Serializable]
  public sealed class SerializableElementInit : ISerializable
  {
    /// <summary>
    /// <see cref="ElementInit.AddMethod"/>
    /// </summary>
    public MethodInfo AddMethod;
    /// <summary>
    /// <see cref="ElementInit.Arguments"/>
    /// </summary>
    public SerializableExpression[] Arguments;

    [SecurityCritical]
    public void GetObjectData(SerializationInfo info, StreamingContext context)
    {
      info.AddValue("AddMethod", AddMethod.ToSerializableForm());
      info.AddArray("Arguments", Arguments);
    }


    public SerializableElementInit()
    {
    }

    public SerializableElementInit(SerializationInfo info, StreamingContext context)
    {
      AddMethod = info.GetString("AddMethod").GetMethodFromSerializableForm();
      Arguments = info.GetArrayFromSerializableForm<SerializableExpression>("Arguments");
    }
  }
}
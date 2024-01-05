// Copyright (C) 2009-2021 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Denis Kryuchkov
// Created:    2009.05.15

using System.Linq.Expressions;
using System.Runtime.Serialization;
using System.Security;
using Xtensive.Linq.SerializableExpressions.Internals;

namespace Xtensive.Linq.SerializableExpressions
{
  /// <summary>
  /// A serializable representation of <see cref="MemberListBinding"/>.
  /// </summary>
  [Serializable]
  public sealed class SerializableMemberListBinding : SerializableMemberBinding
  {
    /// <summary>
    /// <see cref="MemberListBinding.Initializers"/>
    /// </summary>
    public SerializableElementInit[] Initializers;

    [SecurityCritical]
    public override void GetObjectData(SerializationInfo info, StreamingContext context)
    {
      base.GetObjectData(info, context);
      info.AddArray("Initializers", Initializers);
    }

    public SerializableMemberListBinding()
    {
    }

    public SerializableMemberListBinding(SerializationInfo info, StreamingContext context)
      : base(info, context)
    {
      Initializers = info.GetArrayFromSerializableForm<SerializableElementInit>("Initializers");
    }
  }
}
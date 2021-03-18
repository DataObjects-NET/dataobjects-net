// Copyright (C) 2009-2021 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Denis Krjuchkov
// Created:    2009.05.15

using System;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.Serialization;
using System.Security;
using Xtensive.Linq.SerializableExpressions.Internals;

namespace Xtensive.Linq.SerializableExpressions
{
  /// <summary>
  /// A serializable representation of <see cref="MemberBinding"/>
  /// </summary>
  [Serializable]
  public abstract class SerializableMemberBinding : ISerializable
  {
    /// <summary>
    /// <see cref="MemberBinding.BindingType"/>
    /// </summary>
    public MemberBindingType BindingType;
    /// <summary>
    /// <see cref="MemberBinding.Member"/>
    /// </summary>
    public MemberInfo Member;

    [SecurityCritical]
    public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
    {
      info.AddValue("Member", Member.ToSerializableForm());
      info.AddValue("BindingType", BindingType.ToString());
    }


    protected SerializableMemberBinding()
    {
    }

    protected SerializableMemberBinding(SerializationInfo info, StreamingContext context)
    {
      Member = info.GetString("Member").GetMemberFromSerializableForm();
      BindingType = (MemberBindingType) Enum.Parse(typeof(MemberBindingType), info.GetString("BindingType")); 
    }
  }
}
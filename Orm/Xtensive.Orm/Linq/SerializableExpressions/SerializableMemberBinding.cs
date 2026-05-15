// Copyright (C) 2009-2026 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Denis Krjuchkov
// Created:    2009.05.15

using System;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;
using Xtensive.Linq.SerializableExpressions.Internals;

namespace Xtensive.Linq.SerializableExpressions
{
  /// <summary>
  /// A serializable representation of <see cref="MemberBinding"/>
  /// </summary>
  [Serializable]
  [DataContract]
  [KnownType(typeof(SerializableMemberAssignment)),
    JsonDerivedType(typeof(SerializableMemberAssignment), nameof(SerializableMemberAssignment))]
  [KnownType(typeof(SerializableMemberListBinding)),
    JsonDerivedType(typeof(SerializableMemberListBinding), nameof(SerializableMemberListBinding))]
  [KnownType(typeof(SerializableMemberMemberBinding)),
    JsonDerivedType(typeof(SerializableMemberMemberBinding), nameof(SerializableMemberMemberBinding))]
  public abstract class SerializableMemberBinding
  {
    /// <summary>
    /// <see cref="MemberBinding.BindingType"/>
    /// </summary>
    [DataMember, JsonInclude]
    public MemberBindingType BindingType;
    /// <summary>
    /// <see cref="MemberBinding.Member"/>
    /// </summary>
    [DataMember, JsonInclude]
    public SerializableMemberInfo Member;
  }
}
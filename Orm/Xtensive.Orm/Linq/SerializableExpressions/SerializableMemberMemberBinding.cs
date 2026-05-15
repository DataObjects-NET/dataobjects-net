// Copyright (C) 2009-2026 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Denis Krjuchkov
// Created:    2009.05.15

using System;
using System.Linq.Expressions;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace Xtensive.Linq.SerializableExpressions
{
  /// <summary>
  /// A serializable representation of <see cref="MemberMemberBinding"/>.
  /// </summary>
  [Serializable]
  [DataContract]
  public sealed class SerializableMemberMemberBinding : SerializableMemberBinding
  {
    /// <summary>
    /// <see cref="MemberMemberBinding.Bindings"/>
    /// </summary>
    [DataMember, JsonInclude]
    public SerializableMemberBinding[] Bindings;
  }
}
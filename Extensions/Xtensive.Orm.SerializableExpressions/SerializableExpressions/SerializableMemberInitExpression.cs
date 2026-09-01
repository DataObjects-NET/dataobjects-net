// Copyright (C) 2009-2026 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Denis Krjuchkov
// Created:    2009.05.13

using System;
using System.Linq.Expressions;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace Xtensive.Orm.SerializableExpressions
{
  /// <summary>
  /// A serializable representation of <see cref="MemberInitExpression"/>
  /// </summary>
  [DataContract]
  public sealed class SerializableMemberInitExpression : SerializableExpression
  {
    /// <summary>
    /// <see cref="MemberInitExpression.NewExpression"/>
    /// </summary>
    [DataMember, JsonInclude]
    public SerializableNewExpression NewExpression;
    /// <summary>
    /// <see cref="MemberInitExpression.Bindings"/>
    /// </summary>
    [DataMember, JsonInclude]
    public SerializableMemberBinding[] Bindings;
  }
}
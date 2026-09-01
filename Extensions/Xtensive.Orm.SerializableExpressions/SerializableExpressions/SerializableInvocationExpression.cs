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
  /// A serializable representation of <see cref="InvocationExpression"/>.
  /// </summary>
  [DataContract]
  public sealed class SerializableInvocationExpression : SerializableExpression
  {
    /// <summary>
    /// <see cref="InvocationExpression.Expression"/>
    /// </summary>
    [DataMember, JsonInclude]
    public SerializableExpression Expression;
    /// <summary>
    /// <see cref="InvocationExpression.Arguments"/>
    /// </summary>
    [DataMember, JsonInclude]
    public SerializableExpression[] Arguments;
  }
}
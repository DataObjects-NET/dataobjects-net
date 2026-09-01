// Copyright (C) 2009-2021 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Denis Krjuchkov
// Created:    2009.05.12

using System;
using System.Linq.Expressions;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace Xtensive.Orm.SerializableExpressions
{
  /// <summary>
  /// A serializable representation of <see cref="TypeBinaryExpression"/>.
  /// </summary>
  [DataContract]
  public sealed class SerializableTypeBinaryExpression : SerializableExpression
  {
    /// <summary>
    /// <see cref="TypeBinaryExpression.Expression"/>
    /// </summary>
    [DataMember, JsonInclude]
    public SerializableExpression Expression;
    /// <summary>
    /// <see cref="TypeBinaryExpression.TypeOperand"/>
    /// </summary>
    [DataMember, JsonInclude]
    public SerializableType TypeOperand;
  }
}
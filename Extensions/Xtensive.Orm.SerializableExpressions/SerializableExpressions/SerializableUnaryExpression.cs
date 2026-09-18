// Copyright (C) 2009-2026 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Denis Krjuchkov
// Created:    2009.05.12

using System;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace Xtensive.Orm.SerializableExpressions
{
  /// <summary>
  /// A serializable representation of <see cref="UnaryExpression"/>.
  /// </summary>
  [DataContract]
  public sealed class SerializableUnaryExpression : SerializableExpression
  {
    /// <summary>
    /// <see cref="UnaryExpression.Operand"/>
    /// </summary>
    [DataMember, JsonInclude]
    public SerializableExpression Operand;
    /// <summary>
    /// <see cref="UnaryExpression.Method"/>
    /// </summary>
    [DataMember, JsonInclude]
    public SerializableMethodInfo Method;
  }
}
// Copyright (C) 2009-2026 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Denis Krjuchkov
// Created:    2009.05.12

using System;
using System.Linq.Expressions;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace Xtensive.Linq.SerializableExpressions
{
  /// <summary>
  /// A serializable representation of <see cref="ConditionalExpression"/>.
  /// </summary>
  [Serializable]
  [DataContract]
  public sealed class SerializableConditionalExpression : SerializableExpression
  {
    /// <summary>
    /// <see cref="ConditionalExpression.Test"/>
    /// </summary>
    [DataMember, JsonInclude]
    public SerializableExpression Test;
    /// <summary>
    /// <see cref="ConditionalExpression.IfTrue"/>
    /// </summary>
    [DataMember, JsonInclude]
    public SerializableExpression IfTrue;
    /// <summary>
    /// <see cref="ConditionalExpression.IfFalse"/>
    /// </summary>
    [DataMember, JsonInclude]
    public SerializableExpression IfFalse;
  }
}
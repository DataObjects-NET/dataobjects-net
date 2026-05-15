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
using Xtensive.Linq.SerializableExpressions.Internals;

namespace Xtensive.Linq.SerializableExpressions
{
  /// <summary>
  /// A serializable representation of <see cref="BinaryExpression"/>.
  /// </summary>
  [Serializable]
  [DataContract]
  public sealed class SerializableBinaryExpression : SerializableExpression
  {
    /// <summary>
    /// <see cref="BinaryExpression.IsLiftedToNull"/>
    /// </summary>
    [DataMember, JsonInclude]
    public bool IsLiftedToNull;
    /// <summary>
    /// <see cref="BinaryExpression.Left"/>.
    /// </summary>
    [DataMember, JsonInclude]
    public SerializableExpression Left;
    /// <summary>
    /// <see cref="BinaryExpression.Right"/>
    /// </summary>
    [DataMember, JsonInclude]
    public SerializableExpression Right;
    /// <summary>
    /// <see cref="BinaryExpression.Method"/>
    /// </summary>
    [DataMember, JsonInclude]
    public SerializableMethodInfo Method;
  }
}
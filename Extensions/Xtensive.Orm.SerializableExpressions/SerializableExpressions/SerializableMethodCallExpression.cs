// Copyright (C) 2009-2026 Xtensive LLC.
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
  /// A serializable representation of <see cref="MethodCallExpression"/>.
  /// </summary>
  [DataContract]
  public sealed class SerializableMethodCallExpression : SerializableExpression
  {
    /// <summary>
    /// <see cref="MethodCallExpression.Arguments"/>
    /// </summary>
    [DataMember, JsonInclude]
    public SerializableExpression[] Arguments;
    /// <summary>
    /// <see cref="MethodCallExpression.Method"/>
    /// </summary>
    [DataMember, JsonInclude]
    public SerializableMethodInfo Method;
    /// <summary>
    /// <see cref="MethodCallExpression.Object"/>
    /// </summary>
    [DataMember, JsonInclude]
    public SerializableExpression Object;
  }
}
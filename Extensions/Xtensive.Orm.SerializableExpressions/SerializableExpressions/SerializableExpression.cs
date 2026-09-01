// Copyright (C) 2009-2026 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Denis Krjuchkov
// Created:    2009.05.12

using System;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace Xtensive.Orm.SerializableExpressions
{
  /// <summary>
  /// A serializable representation of <see cref="Expression"/>.
  /// </summary>
  [DataContract]
  [KnownType(typeof(SerializableBinaryExpression)),      JsonDerivedType(typeof(SerializableBinaryExpression), nameof(SerializableBinaryExpression))]
  [KnownType(typeof(SerializableConditionalExpression)), JsonDerivedType(typeof(SerializableConditionalExpression), nameof(SerializableConditionalExpression))]
  [KnownType(typeof(SerializableConstantExpression)),    JsonDerivedType(typeof(SerializableConstantExpression), nameof(SerializableConstantExpression))]
  [KnownType(typeof(SerializableDefaultExpression)),     JsonDerivedType(typeof(SerializableDefaultExpression), nameof(SerializableDefaultExpression))]
  [KnownType(typeof(SerializableInvocationExpression)),  JsonDerivedType(typeof(SerializableInvocationExpression), nameof(SerializableInvocationExpression))]
  [KnownType(typeof(SerializableLambdaExpression)),      JsonDerivedType(typeof(SerializableLambdaExpression), nameof(SerializableLambdaExpression))]
  [KnownType(typeof(SerializableListInitExpression)),    JsonDerivedType(typeof(SerializableListInitExpression), nameof(SerializableListInitExpression))]
  [KnownType(typeof(SerializableMemberExpression)),      JsonDerivedType(typeof(SerializableMemberExpression), nameof(SerializableMemberExpression))]
  [KnownType(typeof(SerializableMemberInitExpression)),  JsonDerivedType(typeof(SerializableMemberInitExpression), nameof(SerializableMemberInitExpression))]
  [KnownType(typeof(SerializableMethodCallExpression)),  JsonDerivedType(typeof(SerializableMethodCallExpression), nameof(SerializableMethodCallExpression))]
  [KnownType(typeof(SerializableNewArrayExpression)),    JsonDerivedType(typeof(SerializableNewArrayExpression), nameof(SerializableNewArrayExpression))]
  [KnownType(typeof(SerializableNewExpression)),         JsonDerivedType(typeof(SerializableNewExpression), nameof(SerializableNewExpression))]
  [KnownType(typeof(SerializableParameterExpression)),   JsonDerivedType(typeof(SerializableParameterExpression), nameof(SerializableParameterExpression))]
  [KnownType(typeof(SerializableTypeBinaryExpression)),  JsonDerivedType(typeof(SerializableTypeBinaryExpression), nameof(SerializableTypeBinaryExpression))]
  [KnownType(typeof(SerializableUnaryExpression)),       JsonDerivedType(typeof(SerializableUnaryExpression), nameof(SerializableUnaryExpression))]
  public abstract class SerializableExpression
  {
    /// <summary>
    /// <see cref="Expression.NodeType"/>.
    /// </summary>
    [DataMember, JsonInclude]

    public ExpressionType NodeType;
    /// <summary>
    /// <see cref="Expression.Type"/>.
    /// </summary>
    [DataMember, JsonInclude]
    public SerializableType Type;
  }
}
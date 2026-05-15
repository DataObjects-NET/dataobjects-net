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
  /// A serializable representation of <see cref="ParameterExpression"/>.
  /// </summary>
  [Serializable]
  [DataContract]
  public sealed class SerializableParameterExpression : SerializableExpression
  {
    /// <summary>
    /// <see cref="ParameterExpression.Name"/>.
    /// </summary>
    [DataMember, JsonInclude]
    public string Name;
  }
}
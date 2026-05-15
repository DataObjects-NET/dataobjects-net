// Copyright (C) 2009-2026 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Denis Krjuchkov
// Created:    2009.05.12

using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;
using Xtensive.Linq.SerializableExpressions.Internals;

namespace Xtensive.Linq.SerializableExpressions
{
  /// <summary>
  /// A serializable representation of <see cref="NewExpression"/>
  /// </summary>
  [Serializable]
  [DataContract]
  public sealed class SerializableNewExpression : SerializableExpression
  {
    /// <summary>
    /// <see cref="NewExpression.Arguments"/>
    /// </summary>
    [DataMember, JsonInclude]
    public SerializableExpression[] Arguments;
    /// <summary>
    /// <see cref="NewExpression.Constructor"/>
    /// </summary>
    [DataMember, JsonInclude]
    public SerializableConstructorInfo Constructor;
    /// <summary>
    /// <see cref="NewExpression.Members"/>
    /// </summary>
    [DataMember, JsonInclude]
    public SerializableMemberInfo[] Members;
  }
}
// Copyright (C) 2009-2021 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Denis Kryuchkov
// Created:    2009.05.14

using System;
using System.Linq.Expressions;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace Xtensive.Orm.SerializableExpressions
{
  /// <summary>
  /// A serializable representation of <see cref="ElementInit"/>.
  /// </summary>
  [DataContract]
  public sealed class SerializableElementInit
  {
    /// <summary>
    /// <see cref="ElementInit.AddMethod"/>
    /// </summary>
    [DataMember, JsonInclude]
    public SerializableMethodInfo AddMethod;
    /// <summary>
    /// <see cref="ElementInit.Arguments"/>
    /// </summary>
    [DataMember, JsonInclude]
    public SerializableExpression[] Arguments;
  }
}
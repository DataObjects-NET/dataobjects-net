// Copyright (C) 2023-2026 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.

using System;
using System.Linq.Expressions;
using System.Runtime.Serialization;

namespace Xtensive.Orm.SerializableExpressions
{
  /// <summary>
  /// A serializable representation of <see cref="DefaultExpression"/>.
  /// </summary>
  [DataContract]
  public sealed class SerializableDefaultExpression : SerializableExpression
  {
  }
}
// Copyright (C) 2023 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.

using System;
using System.Linq.Expressions;
using System.Runtime.Serialization;

namespace Xtensive.Linq.SerializableExpressions
{
  /// <summary>
  /// A serializable representation of <see cref="DefaultExpression"/>.
  /// </summary>
  [Serializable]
  public sealed class SerializableDefaultExpression : SerializableExpression
  {
    public SerializableDefaultExpression()
    {
    }

    public SerializableDefaultExpression(SerializationInfo info, StreamingContext context)
      : base(info, context)
    {
    }
  }
}
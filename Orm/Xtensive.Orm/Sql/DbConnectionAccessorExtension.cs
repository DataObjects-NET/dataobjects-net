// Copyright (C) 2021 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.

using System.Collections.Generic;
using Xtensive.Orm;

namespace Xtensive.Sql
{
  /// <summary>
  /// Wrapper to pass <see cref="IDbConnectionAccessor"/>s to connection.
  /// </summary>
  public sealed class DbConnectionAccessorExtension
  {
    /// <summary>
    /// Collection of <see cref="IDbConnectionAccessor"/> instances.
    /// </summary>
    public IReadOnlyCollection<IDbConnectionAccessor> Accessors { get; }

    internal DbConnectionAccessorExtension(IReadOnlyCollection<IDbConnectionAccessor> connectionAccessors)
    {
      Accessors = connectionAccessors;
    }
  }
}

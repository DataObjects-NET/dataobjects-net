// Copyright (C) 2021 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.

using System.Collections.Generic;
using Xtensive.Orm;

namespace Xtensive.Sql
{
  /// <summary>
  /// Wrapper to pass handlers to connection.
  /// </summary>
  public sealed class ConnectionHandlersExtension
  {
    /// <summary>
    /// Collection of <see cref="IConnectionHandler"/> instances.
    /// </summary>
    public IReadOnlyCollection<IConnectionHandler> Handlers { get; }

    internal ConnectionHandlersExtension(IReadOnlyCollection<IConnectionHandler> handlers)
    {
      Handlers = handlers;
    }
  }
}

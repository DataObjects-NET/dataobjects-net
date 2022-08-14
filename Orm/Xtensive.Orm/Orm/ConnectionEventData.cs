// Copyright (C) 2021 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.

using System.Data.Common;
using Xtensive.Core;

namespace Xtensive.Orm
{
  /// <summary>
  /// Contains general data for <see cref="IDbConnectionAccessor"/> methods.
  /// </summary>
  public class ConnectionEventData
  {
    /// <summary>
    /// The connection for which event triggered.
    /// </summary>
    public DbConnection Connection { get; }

    /// <summary>
    /// Indicates whether event happened during an attempt to restore connection.
    /// </summary>
    public bool Reconnect { get; }

    public ConnectionEventData(DbConnection connection, bool reconnect = false)
    {
      ArgumentValidator.EnsureArgumentNotNull(connection, nameof(connection));
      Connection = connection;
      Reconnect = reconnect;
    }
  }
}

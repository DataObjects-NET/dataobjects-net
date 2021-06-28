using System.Data.Common;
using Xtensive.Core;

namespace Xtensive.Orm
{
  /// <summary>
  /// Contains general data for <see cref="IConnectionHandler"/> methods.
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

using System.Data.Common;
using Xtensive.Core;

namespace Xtensive.Orm
{
  /// <summary>
  /// Extended <see cref="ConnectionEventData"/> with connection initialization script
  /// </summary>
  public class ConnectionInitEventData : ConnectionEventData
  {
    /// <summary>
    /// Gets the script which will be used for connection initializatin
    /// </summary>
    public string InitializationScript { get; }

    public ConnectionInitEventData(string initializationScript, DbConnection connection, bool reconnect = false)
      : base(connection, reconnect)
    {
      ArgumentValidator.EnsureArgumentNotNullOrEmpty(initializationScript, nameof(initializationScript));
      InitializationScript = initializationScript;
    }
  }
}

// Copyright (C) 2021 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.

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

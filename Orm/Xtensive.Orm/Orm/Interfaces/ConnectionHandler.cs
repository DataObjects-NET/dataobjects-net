// Copyright (C) 2021 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.

using System.Threading;
using System.Threading.Tasks;

namespace Xtensive.Orm
{
  /// <summary>
  /// Base type for connection handlers to be inherited from.
  /// </summary>
  public abstract class ConnectionHandler : IConnectionHandler
  {
    /// <inheritdoc/>
    public virtual void ConnectionOpening(ConnectionEventData eventData)
    {
    }

    /// <inheritdoc/>
    public virtual void ConnectionInitialization(ConnectionInitEventData eventData)
    {
    }

    /// <inheritdoc/>
    public virtual void ConnectionOpened(ConnectionEventData eventData)
    {
    }

    /// <inheritdoc/>
    public virtual void ConnectionOpeningFailed(ConnectionErrorEventData eventData)
    {
    }
  }
}
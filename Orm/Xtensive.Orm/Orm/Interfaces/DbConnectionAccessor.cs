// Copyright (C) 2021 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.

using System.Threading;
using System.Threading.Tasks;

namespace Xtensive.Orm
{
  /// <summary>
  /// Base type for native database connection accessors to be inherited from.
  /// </summary>
  public abstract class DbConnectionAccessor : IDbConnectionAccessor
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
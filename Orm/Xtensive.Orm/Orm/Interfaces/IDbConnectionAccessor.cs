// Copyright (C) 2021 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.

using System.Threading;
using System.Threading.Tasks;

namespace Xtensive.Orm
{
  /// <summary>
  /// Offers event-like methods to access native database connection on different stages.
  /// </summary>
  public interface IDbConnectionAccessor
  {
    /// <summary>
    /// Executes before connection opening.
    /// </summary>
    /// <param name="eventData">Information connected with this event.</param>
    void ConnectionOpening(ConnectionEventData eventData);

    /// <summary>
    /// Executes before connection opening.
    /// </summary>
    /// <param name="eventData">Information connected with this event.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Task performing operation.</returns>
    Task ConnectionOpeningAsync(ConnectionEventData eventData, CancellationToken cancellationToken);

    /// <summary>
    /// Executes when connection is already opened but initialization script
    /// hasn't been executed yet.
    /// </summary>
    /// <param name="eventData">Information connected with this event.</param>
    void ConnectionInitialization(ConnectionInitEventData eventData);

    /// <summary>
    /// Executes when connection is already opened but initialization script
    /// hasn't been executed yet.
    /// </summary>
    /// <param name="eventData">Information connected with this event.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Task performing operation.</returns>
    Task ConnectionInitializationAsync(ConnectionInitEventData eventData, CancellationToken cancellationToken);

    /// <summary>
    /// Executes when connection is successfully opened and initialized.
    /// </summary>
    /// <param name="eventData">Information connected with this event.</param>
    void ConnectionOpened(ConnectionEventData eventData);

    /// <summary>
    /// Executes when connection is successfully opened and initialized.
    /// </summary>
    /// <param name="eventData">Information connected with this event.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Task performing operation.</returns>
    Task ConnectionOpenedAsync(ConnectionEventData eventData, CancellationToken cancellationToken);

    /// <summary>
    /// Executes if an error appeared on either connection opening or connection initialization.
    /// </summary>
    /// <param name="eventData">Information connected with this event.</param>
    void ConnectionOpeningFailed(ConnectionErrorEventData eventData);

    /// <summary>
    /// Executes if an error appeared on either connection opening or connection initialization.
    /// </summary>
    /// <param name="eventData">Information connected with this event.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Task performing operation.</returns>
    Task ConnectionOpeningFailedAsync(ConnectionErrorEventData eventData, CancellationToken cancellationToken);
  }
}

// Copyright (C) 2020 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.

using System.Threading;
using System.Threading.Tasks;
using Xtensive.Orm;
using Xtensive.Orm.Configuration;

namespace Xtensive.Orm.Interfaces
{
  /// <summary>
  /// Contract for types being able to open <see cref="Session"/>.
  /// </summary>
  public interface ISessionSource
  {
    /// <summary>
    /// Opens new <see cref="Session"/> with default <see cref="SessionConfiguration"/>.
    /// </summary>
    /// <returns>
    /// New <see cref="Session"/> object.
    /// </returns>
    Session OpenSession();

    /// <summary>
    /// Opens new <see cref="Session"/> of specified <see cref="SessionType"/>.
    /// </summary>
    /// <param name="type">The type of session.</param>
    /// <returns>
    /// New <see cref="Session"/> object.
    /// </returns>
    Session OpenSession(SessionType type);

    /// <summary>
    /// Opens new <see cref="Session"/> with specified <see cref="SessionConfiguration"/>.
    /// </summary>
    /// <param name="configuration">The session configuration.</param>
    /// <returns>
    /// New <see cref="Session"/> object.
    /// </returns>
    Session OpenSession(SessionConfiguration configuration);

    /// <summary>
    /// Asynchronously opens new <see cref="Session"/> with default <see cref="SessionConfiguration"/>.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task<Session> OpenSessionAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Asynchronously opens new <see cref="Session"/> of specified <see cref="SessionType"/> asynchronously.
    /// </summary>
    /// <param name="type">The type of session.</param>
    /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task<Session> OpenSessionAsync(SessionType type, CancellationToken cancellationToken = default);

    /// <summary>
    /// Opens new <see cref="Session"/> with specified <see cref="SessionConfiguration"/> asynchronously.
    /// </summary>
    /// <param name="configuration">The session configuration.</param>
    /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task<Session> OpenSessionAsync(SessionConfiguration configuration, CancellationToken cancellationToken = default);
  }
}

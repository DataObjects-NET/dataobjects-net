using System.Threading;
using System.Threading.Tasks;
using Xtensive.Orm;
using Xtensive.Orm.Configuration;

namespace Xtensive.Orm.Interfaces
{
  /// <summary>
  /// Defines <see cref="StorageNode"/>-dependant operations.
  /// </summary>
  public interface ISelectedStorageNode
  {
    /// <summary>
    /// Opens new <see cref="Session"/> with default <see cref="SessionConfiguration"/>.
    /// </summary>
    /// <returns>
    /// New <see cref="Session"/> object.
    /// </returns>
    /// <sample><code>
    /// using (var session = domain.SelectStorageNode(nodeId).OpenSession()) {
    ///   // work with persistent objects here.
    /// }
    /// </code></sample>
    /// <seealso cref="Session"/>
    Session OpenSession();

    /// <summary>
    /// Opens new <see cref="Session"/> of specified <see cref="SessionType"/>.
    /// </summary>
    /// <param name="type">The type of session.</param>
    /// <returns>
    /// New <see cref="Session"/> object.
    /// </returns>
    /// <sample><code>
    /// using (var session = domain.SelectStorageNode(nodeId).OpenSession(sessionType)) {
    ///   // work with persistent objects here.
    /// }
    /// </code></sample>
    Session OpenSession(SessionType type);

    /// <summary>
    /// Opens new <see cref="Session"/> with specified <see cref="SessionConfiguration"/>.
    /// </summary>
    /// <param name="configuration">The session configuration.</param>
    /// <returns>
    /// New <see cref="Session"/> object.
    /// </returns>
    /// <sample><code>
    /// using (var session = domain.SelectStorageNode(nodeId).OpenSession(configuration)) {
    ///   // work with persistent objects here
    /// }
    /// </code></sample>
    /// <seealso cref="Session"/>
    Session OpenSession(SessionConfiguration configuration);

    /// <summary>
    /// Asynchronously opens new <see cref="Session"/> with default <see cref="SessionConfiguration"/>.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    /// <sample><code>
    /// /// var ctSource = new CancellationTokenSource();
    /// using (var session = await domain.SelectStorageNode(nodeId).OpenSessionAsync(ctSource.Token)) {
    ///   // work with persistent objects here.
    /// }
    /// </code></sample>
    /// <seealso cref="Session"/>
    Task<Session> OpenSessionAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Asynchronously opens new <see cref="Session"/> of specified <see cref="SessionType"/> asynchronously.
    /// </summary>
    /// <param name="type">The type of session.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    /// <sample><code>
    /// var ctSource = new CancellationTokenSource();
    /// using (var session = await domain.SelectStorageNode(nodeId).OpenSessionAsync(sessionType, ctSource.Token)) {
    ///   // work with persistent objects here.
    /// }
    /// </code></sample>
    Task<Session> OpenSessionAsync(SessionType type, CancellationToken cancellationToken = default);

    /// <summary>
    /// Opens new <see cref="Session"/> with specified <see cref="SessionConfiguration"/> asynchronously.
    /// </summary>
    /// <param name="configuration">The session configuration.</param>
    /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    /// <sample><code>
    /// var ctSource = new CancellationTokenSource();
    /// using (var session = await domain.SelectStorageNode(nodeId).OpenSessionAsync(configuration, ctSource.Token)) {
    ///   // work with persistent objects here
    /// }
    /// </code></sample>
    /// <seealso cref="Session"/>
    Task<Session> OpenSessionAsync(SessionConfiguration configuration, CancellationToken cancellationToken = default);
  }
}

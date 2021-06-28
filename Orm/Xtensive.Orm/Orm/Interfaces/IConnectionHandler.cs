namespace Xtensive.Orm
{
  /// <summary>
  /// Offers event-like methods to access native database connection on different stages
  /// </summary>
  public interface IConnectionHandler
  {
    /// <summary>
    /// Executes before connection opening.
    /// </summary>
    /// <param name="eventData">Information connected with this event.</param>
    void ConnectionOpening(ConnectionEventData eventData) { }

    /// <summary>
    /// Executes when connection is already opened but initialization script
    /// hasn't been executed yet.
    /// </summary>
    /// <param name="eventData">Information connected with this event.</param>
    void ConnectionInitialization(ConnectionInitEventData eventData) { }

    /// <summary>
    /// Executes when connection is successfully opened and initialized
    /// </summary>
    /// <param name="eventData">Information connected with this event</param>
    void ConnectionOpened(ConnectionEventData eventData) { }

    /// <summary>
    /// Executes if an error appeared on either connection opening or connection initialization.
    /// </summary>
    /// <param name="eventData">Information connected with this event</param>
    void ConnectionOpeningFailed(ConnectionErrorEventData eventData) { }
  }
}

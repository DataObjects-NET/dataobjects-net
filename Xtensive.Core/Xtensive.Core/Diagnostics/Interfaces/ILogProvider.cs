namespace Xtensive.Core.Diagnostics
{
  public interface ILogProvider
  {
    /// <summary>
    /// Gets <see cref="ILog"/> object forwarding logging messages to console.
    /// </summary>
    ILog ConsoleLog { get; }

    /// <summary>
    /// Gets <see cref="ILog"/> object forwarding logging messages to nothing.
    /// </summary>
    ILog NullLog { get; }

    /// <summary>
    /// Gets the <see cref="ILog"/> object by its <paramref name="key"/>.
    /// </summary>
    /// <param name="key">The key to get the log for.</param>
    /// <returns>The <see cref="ILog"/> object.</returns>
    ILog GetLog(string key);
  }
}
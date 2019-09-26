using System;

namespace Xtensive.Orm.Reprocessing
{
  /// <summary>
  /// Contains <see cref="ReprocessableDomainBuilder.Error"/> event data.
  /// </summary>
  public class DomainBuildErrorEventArgs : EventArgs
  {
    /// <summary>
    /// Initializes a new instance of the <see cref="DomainBuildErrorEventArgs"/> class.
    /// </summary>
    /// <param name="exception">The exception.</param>
    /// <param name="attempt">The attempt number.</param>
    public DomainBuildErrorEventArgs(Exception exception, int attempt)
    {
      Exception = exception;
      Attempt = attempt;
    }

    /// <summary>
    /// Gets the exception.
    /// </summary>
    public Exception Exception { get; private set; }
    /// <summary>
    /// Gets the attempt number.
    /// </summary>
    public int Attempt { get; private set; }
  }
}

// Copyright (C) 2003-2022 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
using System;

namespace Xtensive.Orm.Reprocessing
{
  /// <summary>
  /// Contains <see cref="ReprocessableDomainBuilder.Error"/> event data.
  /// </summary>
  public readonly struct DomainBuildErrorEventArgs
  {
    /// <summary>
    /// Gets the exception.
    /// </summary>
    public Exception Exception { get; }
    /// <summary>
    /// Gets the attempt number.
    /// </summary>
    public int Attempt { get; }

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
  }
}

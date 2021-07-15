// Copyright (C) 2021 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.

using System;
using System.Data.Common;
using Xtensive.Core;

namespace Xtensive.Orm
{
  /// <summary>
  /// Extended <see cref="ConnectionEventData"/> with error happend during connection opening, restoration or initialization.
  /// </summary>
  public class ConnectionErrorEventData : ConnectionEventData
  {
    /// <summary>
    /// The exception appeared.
    /// </summary>
    public Exception Exception { get; }

    public ConnectionErrorEventData(Exception exception, DbConnection connection, bool reconnect = false)
      : base(connection, reconnect)
    {
      ArgumentValidator.EnsureArgumentNotNull(exception, nameof(exception));
      Exception = exception;
    }
  }
}

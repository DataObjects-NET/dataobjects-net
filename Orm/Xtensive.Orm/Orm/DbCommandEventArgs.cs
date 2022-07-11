// Copyright (C) 2003-2022 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;


namespace Xtensive.Orm
{
  /// <summary>
  /// Events args for <see cref="SessionEventAccessor.DbCommandExecuting"/>
  /// and <see cref="SessionEventAccessor.DbCommandExecuted"/>.
  /// </summary>
  public readonly struct DbCommandEventArgs
  {
    /// <summary>
    /// Gets executed command.
    /// </summary>
    public DbCommand Command { get; }

    /// <summary>
    /// Gets exception, thrown during command execution. <see langword="null" /> if command executed successfully.
    /// </summary>
    public Exception Exception { get; }

    /// <summary>
    /// Initializes a new instance of this class.
    /// <param name="command">Executed command.</param>
    /// <param name="exception" >Exception, appeared during <paramref name="command"/> execution or <see langword="null" />.</param>
    /// </summary>
    public DbCommandEventArgs(DbCommand command, Exception exception = null)
    {
      Command = command;
      Exception = exception;
    }
  }
}

using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using Xtensive.Internals.DocTemplates;

namespace Xtensive.Orm
{
  /// <summary>
  /// Events args for <see cref="SessionEventAccessor.DbCommandExecuting"/>
  /// and <see cref="SessionEventAccessor.DbCommandExecuted"/>.
  /// </summary>
  public class DbCommandEventArgs : EventArgs
  {
    /// <summary>
    /// Gets executed command.
    /// </summary>
    public DbCommand Command { get; private set; }

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// <param name="command">Executed command.</param>
    /// </summary>
    public DbCommandEventArgs(DbCommand command)
    {
      Command = command;
    }
  }
}

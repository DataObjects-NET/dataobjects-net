// Copyright (C) 2026 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.

using System;
using System.Data;


namespace Xtensive.Sql.Drivers.MySql
{
  internal static class MySqlSqlHelper
  {
    /// <summary>
    /// Reduces the isolation level to the most commonly supported by PostgreSQL 10+.
    /// </summary>
    /// <param name="level">The level.</param>
    /// <returns>Converted isolation level.</returns>
    internal static IsolationLevel ReduceIsolationLevel(IsolationLevel level)
    {
      switch (level) {
        case IsolationLevel.ReadUncommitted:
          return IsolationLevel.ReadUncommitted;
        case IsolationLevel.ReadCommitted:
          return IsolationLevel.ReadCommitted;
        case IsolationLevel.RepeatableRead:
          return IsolationLevel.RepeatableRead;
        case IsolationLevel.Serializable:
        case IsolationLevel.Snapshot:
          return IsolationLevel.Serializable;
        default:
          throw new NotSupportedException(string.Format(Resources.Strings.ExIsolationLevelXIsNotSupported, level));
      }
    }
  }
}

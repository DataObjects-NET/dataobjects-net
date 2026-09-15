// Copyright (C) 2026 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.

using System;
using Xtensive.Core;

namespace Xtensive.Orm.Tests
{
  public sealed class QueryExecutionDetector
  {
    private int dbCommandCounter = 0;
    private int queryCounter = 0;

    public bool DbCommandsDetected => dbCommandCounter > 0;
    public bool QueriesDetected => queryCounter > 0;

    public IDisposable Attach(Session session)
    {
      session.Events.DbCommandExecuted += Events_DbCommandExecuted;

      return new Disposable((b) => { session.Events.DbCommandExecuted -= Events_DbCommandExecuted; });
    }

    public void Reset()
    {
      dbCommandCounter = 0;
      queryCounter = 0;
    }

    private void Events_DbCommandExecuted(object sender, DbCommandEventArgs e) => dbCommandCounter++;
  }
}

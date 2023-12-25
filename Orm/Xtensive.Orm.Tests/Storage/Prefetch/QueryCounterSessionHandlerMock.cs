using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xtensive.Orm.Internals;
using Xtensive.Orm.Providers;

namespace Xtensive.Orm.Tests.Storage.Prefetch
{
  internal class QueryCounterSessionHandlerMock : ChainingSessionHandler
  {
    private volatile int syncCounter;
    private volatile int asyncCounter;

    public int GetSyncCounter() => syncCounter;

    public int GetAsyncCounter() => asyncCounter;

    public QueryCounterSessionHandlerMock(SessionHandler chainedHandler) : base(chainedHandler)
    {
    }

    public override void ExecuteQueryTasks(IEnumerable<QueryTask> queryTasks, bool allowPartialExecution)
    {
      _ = Interlocked.Increment(ref syncCounter);
      base.ExecuteQueryTasks(queryTasks, allowPartialExecution);
    }

    public override Task ExecuteQueryTasksAsync(IEnumerable<QueryTask> queryTasks, bool allowPartialExecution, CancellationToken token)
    {
      _ = Interlocked.Increment(ref asyncCounter);
      return base.ExecuteQueryTasksAsync(queryTasks, allowPartialExecution, token);
    }
  }
}

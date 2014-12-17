using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Xtensive.Orm.Internals
{
  internal class IncompletedTaskInfo
  {
    public Task Task { get; private set; }

    public CancellationTokenSource CancellationTokenSource { get; private set; }

    public IncompletedTaskInfo(Task task, CancellationTokenSource cancellationTokenSource)
    {
      Task = task;
      CancellationTokenSource = cancellationTokenSource;
    }
  }
}
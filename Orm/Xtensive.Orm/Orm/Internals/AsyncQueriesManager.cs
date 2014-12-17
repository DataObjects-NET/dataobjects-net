using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xtensive.Orm.Providers;

namespace Xtensive.Orm.Internals
{
  internal class AsyncQueriesManager: SessionBound
  {
    private readonly Dictionary<StateLifetimeToken, Dictionary<Task, IncompletedTaskInfo>> asyncQueries;
    private readonly Dictionary<StateLifetimeToken, IList<Command>> blockingCommands;
    private readonly object lockableObject = new object();

    internal long WorkingAsyncQueriesCount { get; private set; }

    internal long BlockingCommandsCount { get; private set; }

    /// <summary>
    /// Registers information about new asynchronous query
    /// </summary>
    /// <param name="lifetimeToken"></param>
    /// <param name="task"></param>
    /// <param name="cancellationTokenSource"></param>
    internal void AddNewAsyncQuery(StateLifetimeToken lifetimeToken, Task task, CancellationTokenSource cancellationTokenSource)
    {
      lock (lockableObject) {
        Dictionary<Task, IncompletedTaskInfo> tasks;
        if (asyncQueries.TryGetValue(lifetimeToken, out tasks)) {
          tasks.Add(task, new IncompletedTaskInfo(task, cancellationTokenSource));
          WorkingAsyncQueriesCount++;
        }
        else {
          tasks = new Dictionary<Task, IncompletedTaskInfo>();
          tasks.Add(task, new IncompletedTaskInfo(task, cancellationTokenSource));
          asyncQueries.Add(lifetimeToken, tasks);
          WorkingAsyncQueriesCount++;
        }
      }
    }

    /// <summary>
    /// Removes information about asynchronous query
    /// </summary>
    /// <param name="lifetimeToken"></param>
    /// <param name="task"></param>
    /// <returns></returns>
    internal bool TryRemoveFinishedAsyncQuery(StateLifetimeToken lifetimeToken, Task task)
    {
      lock (lockableObject) {
        Dictionary<Task, IncompletedTaskInfo> tasks;
        if (asyncQueries.TryGetValue(lifetimeToken, out tasks)) {
          var removingResult = tasks.Remove(task);
          if (removingResult)
            WorkingAsyncQueriesCount--;
          if (tasks.Count==0)
            asyncQueries.Remove(lifetimeToken);
          return removingResult;
        }
        return false;
      }
    }

    internal bool TryCancelAsyncQuery(StateLifetimeToken stateLifetimeToken, Task task)
    {
      lock (lockableObject) {
        Dictionary<Task, IncompletedTaskInfo> tasks;
        if (asyncQueries.TryGetValue(stateLifetimeToken, out tasks)) {
          IncompletedTaskInfo taskInfo;
          if (tasks.TryGetValue(task, out taskInfo)) {
            taskInfo.CancellationTokenSource.Cancel();
            if (tasks.Remove(task)) 
              WorkingAsyncQueriesCount--;
            if (tasks.Count==0)
              asyncQueries.Remove(stateLifetimeToken);
            return true;
          }
          return false;
        }
        return false;
      }
    }

    internal bool TryCancelAllAsyncQueriesForToken(StateLifetimeToken stateLifetimeToken)
    {
      lock (lockableObject) {
        Dictionary<Task, IncompletedTaskInfo> tasks;
        if (asyncQueries.TryGetValue(stateLifetimeToken, out tasks)) {
          foreach (var incompletedTaskInfo in tasks) {
            incompletedTaskInfo.Value.CancellationTokenSource.Cancel();
          }
          WorkingAsyncQueriesCount = WorkingAsyncQueriesCount - tasks.Count;
          asyncQueries.Remove(stateLifetimeToken);
        }
        return false;
      }
    }

    internal bool TryCancelAllAsyncQueries()
    {
      lock (lockableObject) {
        foreach (var asyncQuery in asyncQueries)
          foreach (var taskInfo in asyncQuery.Value)
            taskInfo.Value.CancellationTokenSource.Cancel();
        ClearAsyncQueryList();
        return true;
      }
    }

    internal bool HasAsyncQueries()
    {
      return asyncQueries.Count > 0;
    }

    internal bool HasAsyncQueriesForToken(StateLifetimeToken stateLifetimeToken)
    {
      return asyncQueries.ContainsKey(stateLifetimeToken);
    }

    internal void AddNewBlockingCommand(StateLifetimeToken token, Command command)
    {
      lock (lockableObject) {
        IList<Command> commands;
        if (blockingCommands.TryGetValue(token, out commands)) {
          commands.Add(command);
        }
        else {
          var list = new List<Command> {command};
          blockingCommands.Add(token, list);
        }
      }
    }

    internal void DisposeBlockingCommands()
    {
      lock (lockableObject) {
        foreach (var blockingCommand in blockingCommands)
          foreach (var command in blockingCommand.Value)
            command.Dispose();
        ClearBlockingCommandList();
      }
    }

    internal void DisposeBlockingCommandForToken(StateLifetimeToken stateLifetimeToken)
    {
      lock (lockableObject) {
        IList<Command> commands;
        if (blockingCommands.TryGetValue(stateLifetimeToken, out commands)) {
          foreach (var command in commands)
            command.Dispose();
          blockingCommands.Remove(stateLifetimeToken);
        }
      }
    }

    internal bool HasBlockingCommandsForToken(StateLifetimeToken token)
    {
      lock (lockableObject) {
        IList<Command> commands;
        if (blockingCommands.TryGetValue(token, out commands)) {
          return commands.Any(el => !el.IsDisposed);
        }
      }
      return false;
    }

    internal bool HasBlockingCommands()
    {
      lock (lockableObject) {
        foreach (var blockingCommand in blockingCommands) {
          foreach (var command in blockingCommand.Value) {
            if (!command.IsDisposed)
              return true;
          }
        }
      }
      return false;
    }

    private void ClearAsyncQueryList()
    {
      asyncQueries.Clear();
      WorkingAsyncQueriesCount = 0;
    }

    private void ClearBlockingCommandList()
    {
      blockingCommands.Clear();
    }

    internal AsyncQueriesManager(Session session)
      : base(session)
    {
      blockingCommands = new Dictionary<StateLifetimeToken, IList<Command>>();
      asyncQueries = new Dictionary<StateLifetimeToken, Dictionary<Task, IncompletedTaskInfo>>();
      WorkingAsyncQueriesCount = 0;
    }
  }
}
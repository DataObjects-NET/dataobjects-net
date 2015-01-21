// Copyright (C) 2012 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kulakov
// Created:    2014.08.29

using System;
using System.Threading;
using System.Threading.Tasks;

namespace Xtensive.Orm
{
#if NET45
  public abstract class DelayedTask : SessionBound
  {
    protected enum DelayedTaskStatus {
      Created,
      Started,
      Completed,
      Faulted,
      Canceled
    }

    protected Exception Exception;
    protected bool IsExecutionStarter;
    protected DelayedTaskStatus InternalStatus;
    protected CancellationTokenSource CancellationTokenSource;
    protected Task InternalTask;

    /// <summary>
    /// Gets status of delayed task.
    /// </summary>
    public TaskStatus Status
    {
      get {
        switch (InternalStatus) {
        case DelayedTaskStatus.Canceled:
          return TaskStatus.Canceled;
        case DelayedTaskStatus.Faulted:
          return TaskStatus.Faulted;
        case DelayedTaskStatus.Started:
          return TaskStatus.Running;
        case DelayedTaskStatus.Completed:
          return TaskStatus.RanToCompletion;
        case DelayedTaskStatus.Created: {
          return TaskStatus.Created;
        }
        }
        return InternalTask!=null ? InternalTask.Status : TaskStatus.Created;
      }
    }

    internal abstract void SetStarted();
    internal abstract void SetFaulted(Exception exception);
    internal abstract void SetCanceled();
    internal abstract void SetCompleted();

    internal DelayedTask(Session session)
      : base(session)
    {
      CancellationTokenSource = new CancellationTokenSource();
      IsExecutionStarter = false;
    }
  }
#endif
}

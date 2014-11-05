// Copyright (C) 2012 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kulakov
// Created:    2014.08.29

using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Xtensive.Orm.Internals;

namespace Xtensive.Orm
{
#if NET45
  /// <summary>
  /// Represents delayed query task.
  /// </summary>
  public class DelayedTask<TResult> : SessionBound
  {
    private enum DelayedTaskStatus
    {
      Created,
      Started,
      Completed,
      Failed,
      Canceled
    }

    private readonly TaskCompletionSource<TResult> source;
    private readonly DelayedQueryResult<TResult> delayedResult;
    private DelayedTaskStatus internalStatus;

    internal static DelayedTaskFactory<TResult> Factory { get; private set; }

    public TaskStatus Status
    {
      get {
        switch (internalStatus) {
          case DelayedTaskStatus.Canceled:
          case DelayedTaskStatus.Failed:
          case DelayedTaskStatus.Started:
          case DelayedTaskStatus.Completed:
            return source.Task.Status;
          case DelayedTaskStatus.Created: {
            if (!delayedResult.LifetimeToken.IsActive)
              throw new InvalidOperationException(Strings.ExThisInstanceIsExpiredDueToTransactionBoundaries);
            if (delayedResult.Task.Result!=null) {
              var delayedResultAsDelayed = delayedResult as Delayed<TResult>;
              if (delayedResultAsDelayed!=null) {
                source.SetResult(delayedResultAsDelayed.Value);
                internalStatus = DelayedTaskStatus.Completed;
              }
              else {
                object aa = delayedResult;
                source.SetResult((TResult) aa);
                internalStatus = DelayedTaskStatus.Completed;
              }
            }
            return source.Task.Status;
          }
          default:
            return source.Task.Status;
        }
      }
    }

    /// <summary>
    /// Gets awaiter for this delayed task.
    /// </summary>
    /// <returns>Returns awaiter</returns>
    public TaskAwaiter<TResult> GetAwaiter()
    {
      var awaiter = source.Task.GetAwaiter();
      switch (internalStatus) {
      case DelayedTaskStatus.Canceled:
      case DelayedTaskStatus.Failed:
      case DelayedTaskStatus.Started:
      case DelayedTaskStatus.Completed:
        return awaiter;
      case DelayedTaskStatus.Created: {
        Session.ExecuteDelayedQueriesAsync(false).ContinueWith(
          task => {
            if (task.IsFaulted) {
              source.SetException(task.Exception.InnerException);
              internalStatus = DelayedTaskStatus.Failed;
              return;
            }
            if (!delayedResult.LifetimeToken.IsActive) {
              source.SetException(new InvalidOperationException(Strings.ExThisInstanceIsExpiredDueToTransactionBoundaries));
              return;
            }
            if (task.IsCanceled) {
              source.SetCanceled();
              internalStatus = DelayedTaskStatus.Canceled;
              return;
            }
            var delayedResultAsDelayed = delayedResult as Delayed<TResult>;
            if (delayedResultAsDelayed!=null) {
              if (!delayedResultAsDelayed.LifetimeToken.IsActive) {
                source.SetException(new InvalidOperationException(Strings.ExThisInstanceIsExpiredDueToTransactionBoundaries));
                return;
              }
              source.SetResult(delayedResultAsDelayed.Value);
              internalStatus = DelayedTaskStatus.Completed;
              return;
            }
            object aa = delayedResult;
            source.SetResult((TResult) aa);
            internalStatus = DelayedTaskStatus.Completed;
          });
        return awaiter;
      }
      default:
        return awaiter;
      }
    }

    /// <summary>
    /// Convert this instance to Task.
    /// </summary>
    /// <returns>Conversion result.</returns>
    public Task<TResult> ToTask()
    {
      switch (internalStatus) {
      case DelayedTaskStatus.Canceled:
      case DelayedTaskStatus.Failed:
      case DelayedTaskStatus.Started:
      case DelayedTaskStatus.Completed:
        return source.Task;
      case DelayedTaskStatus.Created: {
        Session.ExecuteDelayedQueriesAsync(false).ContinueWith(
          task => {
            if (task.IsFaulted) {
              source.SetException(task.Exception.InnerException);
              internalStatus = DelayedTaskStatus.Failed;
              return;
            }
            if (!delayedResult.LifetimeToken.IsActive) {
              source.SetException(new InvalidOperationException(Strings.ExThisInstanceIsExpiredDueToTransactionBoundaries));
              return;
            }
            if (task.IsCanceled) {
              source.SetCanceled();
              internalStatus = DelayedTaskStatus.Canceled;
              return;
            }
            var delayedResultAsDelayed = delayedResult as Delayed<TResult>;
            if (delayedResultAsDelayed!=null) {
              source.SetResult(delayedResultAsDelayed.Value);
              internalStatus = DelayedTaskStatus.Completed;
              return;
            }
            object aa = delayedResult;
            source.SetResult((TResult) aa);
            internalStatus = DelayedTaskStatus.Completed;
          });
        return source.Task;
      }
      default:
        return source.Task;
      }
    }

    static DelayedTask()
    {
      Factory = new DelayedTaskFactory<TResult>();
    }

    internal DelayedTask(Session session, DelayedQueryResult<TResult> delayed) //TResult
      : base(session)
    {
      source = new TaskCompletionSource<TResult>();
      delayedResult = delayed;
      internalStatus = DelayedTaskStatus.Created;
    }
  }
#endif
}

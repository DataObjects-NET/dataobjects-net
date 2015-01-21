// Copyright (C) 2014 Xtensive LLC.
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
  public sealed class DelayedTask<TResult> : DelayedTask
  {
    private static DelayedTaskFactory<TResult> factory;
    private readonly TaskCompletionSource<TResult> source;
    private readonly DelayedQueryResult<TResult> delayedResult;
    
    
    internal static DelayedTaskFactory<TResult> Factory
    {
      get { return factory ?? (factory = new DelayedTaskFactory<TResult>()); }
    }

    /// <summary>
    /// Gets result of task. If task not started yet, then task will started and wait until task finished.
    /// </summary>
    public TResult Result
    {
      get {
        EnsureInternalTaskInitialized();
        return source.Task.Result;
      }
    }

    /// <summary>
    /// Runs task syncroniously.
    /// </summary>
    public void RunSyncroniously()
    {
      Session.ExecuteUserDefinedDelayedQueries(false);
    }

    /// <summary>
    /// Wait for DelayedTask{<typeparamref name="TResult"/>} to complete execution.
    /// </summary>
    /// <exception cref="T:System.AggregateException">The exception was thrown during the execution of the task.</exception>
    public void Wait()
    {
      if (InternalStatus==DelayedTaskStatus.Completed ||
        InternalStatus==DelayedTaskStatus.Canceled ||
        InternalStatus==DelayedTaskStatus.Faulted)
        return;
      EnsureInternalTaskInitialized();
      source.Task.Wait();
    }

    /// <summary>
    /// Waits for the <see cref="T:System.Threading.Tasks.Task"/> to complete execution within a specified number of milliseconds.
    /// </summary>
    /// 
    /// <returns><see langword="true"/> if the delayed task completed execution within the allotted time; otherwise, <see langword="false"/>.</returns>
    /// <param name="millisecondsTimeout">The number of milliseconds to wait, or <see cref="F:System.Threading.Timeout.Infinite"/> (-1) to wait indefinitely.</param>
    /// <exception cref="T:System.ArgumentOutOfRangeException"><paramref name="millisecondsTimeout"/> is a negative number other than -1, which represents an infinite time-out.</exception>
    /// <exception cref="T:System.AggregateException">The exception was thrown during the execution of the task.</exception>
    public bool Wait(Int32 millisecondsTimeout)
    {
      if (InternalStatus==DelayedTaskStatus.Completed ||
        InternalStatus==DelayedTaskStatus.Canceled ||
        InternalStatus==DelayedTaskStatus.Faulted)
        return true;
      EnsureInternalTaskInitialized();
      return source.Task.Wait(millisecondsTimeout);
    }

    /// <summary>
    /// Wait for DelayedTask{<typeparamref name="TResult"/>} to complete execution.
    /// </summary>
    /// <param name="timeout">A <see cref="T:System.TimeSpan"/> that represents the number of milliseconds to wait, 
    /// or a <see cref="T:System.TimeSpan"/> that represents -1 milliseconds to wait indefinitely.</param>
    /// <returns><see langword="true"/> if the delayed task completed execution within the allotted time; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="T:System.ArgumentOutOfRangeException"><paramref name="timeout"/> is a negative number other than -1 milliseconds, which represents an infinite time-out -or- timeout is greater than <see cref="F:System.Int32.MaxValue"/>.</exception>
    /// <exception cref="T:System.AggregateException">The <see cref="T:System.Threading.Tasks.Task"/> was canceled -or- an exception was thrown during the execution of the <see cref="T:System.Threading.Tasks.Task"/>.</exception>
    public bool Wait(TimeSpan timeout)
    {
      if (InternalStatus==DelayedTaskStatus.Completed ||
        InternalStatus==DelayedTaskStatus.Canceled ||
        InternalStatus==DelayedTaskStatus.Faulted)
        return true;
      EnsureInternalTaskInitialized();
      return source.Task.Wait(timeout);
    }

    /// <summary>
    /// Gets awaiter for this delayed task.
    /// </summary>
    /// <returns>Returns awaiter.</returns>
    public TaskAwaiter<TResult> GetAwaiter()
    {
      EnsureInternalTaskInitialized();
      return source.Task.GetAwaiter();
    }

    /// <summary>
    /// Convert this instance to Task.
    /// </summary>
    /// <returns>Conversion result.</returns>
    public Task<TResult> ToTask()
    {
      EnsureInternalTaskInitialized();
      return source.Task;
    }

    internal override void SetStarted()
    {
      EnsureStatusIsNot(DelayedTaskStatus.Canceled,Strings.ExUnableToStartDelayedTaskTaskHasBeenCancelled);
      EnsureStatusIsNot(DelayedTaskStatus.Completed, Strings.ExUnableToStartDelayedTaskTaskhasBeenCompleted);
      EnsureStatusIsNot(DelayedTaskStatus.Faulted, Strings.ExUnableToStartDelayedTaskTaskHasBeenCompletedWithFault);
      EnsureStatusIsNot(DelayedTaskStatus.Started, Strings.ExUnableToStartDelayedTaskTaskHasAlreadyBeenStarted);
      InternalStatus = DelayedTaskStatus.Started;
    }

    internal override void SetCompleted()
    {
      EnsureStatusIsNot(DelayedTaskStatus.Completed, Strings.ExUnableToCompleteDelayedTaskTaskHasBeenCompleted);
      EnsureStatusIsNot(DelayedTaskStatus.Canceled, Strings.ExUnableToCompleteDelayedTaskTaskHasBeenCancelled);
      EnsureStatusIsNot(DelayedTaskStatus.Faulted, Strings.ExUnableToCompleteDelayedTaskTaskHasBeenCompletedWithFault);
      EnsureStatusIsNot(DelayedTaskStatus.Created, Strings.ExUnableToCompleteDelayedTaskTaskHasNotBeenStartedYet);
      InternalStatus = DelayedTaskStatus.Completed;
      if(IsExecutionStarter)
        return;
      var delayedResultAsDelayed = delayedResult as Delayed<TResult>;
      if (delayedResultAsDelayed!=null) {
        source.SetResult(delayedResultAsDelayed.Value);
        return;
      }
      object aa = delayedResult;
      source.SetResult((TResult)aa);
    }

    internal override void SetFaulted(Exception exception)
    {
      EnsureStatusIsNot(DelayedTaskStatus.Faulted, Strings.ExTaskHasAlreadyBeenCompletedWithFault);
      EnsureStatusIsNot(DelayedTaskStatus.Canceled, Strings.ExUnableToCompleteDelayedTaskWithFaultTaskHasBeenCancelled);
      EnsureStatusIsNot(DelayedTaskStatus.Completed, Strings.ExUnableToCompleteDelayedTaskWithFaultTaskHasBeenCompleted);
      EnsureStatusIsNot(DelayedTaskStatus.Created, Strings.ExUnableToCompleteWithFaultUnstartedDelayedTaskTaskHasNotBeenStartedYet);
      if (IsExecutionStarter)
        Exception = exception;
      else source.TrySetException(exception);
      InternalStatus = DelayedTaskStatus.Faulted;
    }

    internal override void SetCanceled()
    {
      EnsureStatusIsNot(DelayedTaskStatus.Canceled, Strings.ExUnableToCancelDelayedTaskTaskHasBeenCanceled);
      EnsureStatusIsNot(DelayedTaskStatus.Faulted, Strings.ExUnableToCancelDelayedTaskTaskHasBeenCompletedWithFault);
      EnsureStatusIsNot(DelayedTaskStatus.Completed, Strings.ExUnableToCancelDelayedTaskTaskHasBeenCompleted);
      if (!IsExecutionStarter)
        source.TrySetCanceled();
      InternalStatus = DelayedTaskStatus.Canceled;
    }


    private void EnsureInternalTaskInitialized()
    {
      switch (InternalStatus) {
        case DelayedTaskStatus.Canceled:
        case DelayedTaskStatus.Faulted:
        case DelayedTaskStatus.Completed:
        case DelayedTaskStatus.Started:
          return;
        case DelayedTaskStatus.Created: {
          IsExecutionStarter = true;
          InternalTask = Session.ExecuteDelayedQueriesAsync(false, CancellationTokenSource.Token);
            Session.AddNewAsyncQuery(InternalTask, CancellationTokenSource);
          InternalTask
            .ContinueWith(
              (task, session) => {
                ((Session) session).RemoveFinishedAsyncQuery(task);
                if (InternalStatus==DelayedTaskStatus.Canceled) {
                  source.SetCanceled();
                  return;
                }
                if (InternalStatus==DelayedTaskStatus.Faulted) {
                  source.SetException(Exception);
                  return;
                }
                if (InternalStatus==DelayedTaskStatus.Completed) {
                  var delayedResultAsDelayed = delayedResult as Delayed<TResult>;
                  if (delayedResultAsDelayed!=null) {
                    source.SetResult(delayedResultAsDelayed.Value);
                    return;
                  }
                  object aa = delayedResult;
                  source.SetResult((TResult)aa);
                }
              },Session);
          return;
        }
      }
    }

    private void EnsureStatusIsNot(DelayedTaskStatus unexpectedStatus, string errorMessage)
    {
      if (InternalStatus==unexpectedStatus)
        throw new InvalidOperationException(errorMessage);
    }

    internal DelayedTask(Session session, DelayedQueryResult<TResult> delayed)
      : base(session)
    {
      source = new TaskCompletionSource<TResult>();
      delayedResult = delayed;
      InternalStatus = DelayedTaskStatus.Created;
    }
  }


#endif
}

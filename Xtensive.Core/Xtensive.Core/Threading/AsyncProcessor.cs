// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexander Nikolaev
// Created:    2009.08.12

using System;
using System.Collections.Generic;
using System.Threading;
using Xtensive.Core;
using Xtensive.Internals.DocTemplates;

namespace Xtensive.Threading
{
  /// <summary>
  /// Executes provided delegates at separate thread.
  /// </summary>
  public sealed class AsyncProcessor : IDisposable
  {
    private volatile bool isRunning = true;
    private volatile Exception error;
    private readonly Queue<ITask> tasks = new Queue<ITask>();
    private readonly ManualResetEvent executionCompletedEvent = new ManualResetEvent(true);
    private readonly AutoResetEvent taskAddedEvent = new AutoResetEvent(false);
    
    /// <summary>
    /// Gets the exception intercepted during a task's execution.
    /// </summary>
    public Exception Error { get { return error;} }

    /// <summary>
    /// Gets a value indicating whether this instance has an intercepted exception.
    /// </summary>
    public bool HasError { get { return error!=null;} }

    /// <summary>
    /// Gets or sets a value indicating whether this instance executes 
    /// a task at the thread that queued it (i.e. synchronously).
    /// </summary>
    public bool IsSynchronous { get; set; }

    /// <summary>
    /// Enqueue the specified delegate for the execution at another thread 
    /// or executes it at the calling thread.
    /// </summary>
    /// <typeparam name="T">The result type.</typeparam>
    /// <param name="function">The delegate to be executed.</param>
    /// <param name="synchronously">if set to <see langword="true"/> 
    /// then the delegate is executed at the calling thread; otherwise it is queued 
    /// to be executed in <see cref="AsyncProcessor"/>'s thread.</param>
    /// <returns>The <see cref="TaskResult{T}"/> that should be used to obtain 
    /// a result of delegate's execution.</returns>
    public TaskResult<T> Execute<T>(Func<T> function, bool synchronously)
    {
      ArgumentValidator.EnsureArgumentNotNull(function, "function");
      EnsureHasNotError();
      var task = new Task<T>(function);
      ExecuteTask(task, synchronously);
      return task.Result;
    }

    /// <summary>
    /// Enqueue the specified delegate for the execution at another thread 
    /// or executes it at the calling thread.
    /// </summary>
    /// <param name="action">The delegate to be executed.</param>
    /// <param name="synchronously">if set to <see langword="true"/> 
    /// then the delegate is executed at the calling thread; otherwise it is queued 
    /// to be executed in <see cref="AsyncProcessor"/>'s thread.</param>
    public void Execute(Action action, bool synchronously)
    {
      ArgumentValidator.EnsureArgumentNotNull(action, "action");
      EnsureHasNotError();
      var task = new Task(action);
      ExecuteTask(task, synchronously);
    }

    /// <summary>
    /// Resets an error.
    /// </summary>
    public void ResetError()
    {
      error = null;
    }

    /// <summary>
    /// Waits for the completion of all queued tasks.
    /// </summary>
    public void WaitForCompletion()
    {
      executionCompletedEvent.WaitOne();
    }

    #region Private / internal members

    private void ExecuteTask(ITask task, bool synchronously)
    {
      if (IsSynchronous || synchronously) {
        WaitForCompletion();
        ExecuteTask(task);
      }
      else
        EnqueueTask(task);
    }
    
    private void EnqueueTask(ITask task)
    {
      lock (tasks)
        tasks.Enqueue(task);
      executionCompletedEvent.Reset();
      taskAddedEvent.Set();
    }

    private void Execute()
    {
      while (isRunning) {
        ITask task = null;
        do {
          if (task!=null)
            ExecuteTask(task);
          lock (tasks)
            task = tasks.Count > 0 ? tasks.Dequeue() : null;
        } while (task!=null);
        executionCompletedEvent.Set();
        taskAddedEvent.WaitOne();
      }
    }

    private void ExecuteTask(ITask task)
    {
      try {
        task.Execute();
      }
      catch (Exception e) {
        error = e;
        lock (tasks) {
          while (tasks.Count > 0)
            tasks.Dequeue().Terminate(new TaskExecutionException(e));
          executionCompletedEvent.Set();
        }
      }
    }

    private void EnsureHasNotError()
    {
      if (HasError)
        throw new TaskExecutionException(error);
    }

    #endregion


    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    public AsyncProcessor()
    {
      ThreadPool.QueueUserWorkItem(_ => Execute());
    }
    
    // IDisposable methods

    /// <summary>
    /// <see cref="DisposableDocTemplate.Dispose()" copy="true"/>
    /// </summary>
    public void Dispose()
    {
      Dispose(true);
    }

    private void Dispose(bool disposing)
    {
      isRunning = false;
      if (disposing) {
        GC.SuppressFinalize(this);
        taskAddedEvent.Set();
      }
    }

    // Finalizer

    /// <summary>
    /// <see cref="ClassDocTemplate.Dtor" copy="true"/>
    /// </summary>
    ~AsyncProcessor()
    {
      Dispose(false);
    }
  }
}
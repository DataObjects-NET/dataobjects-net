// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexander Nikolaev
// Created:    2009.08.12

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using Xtensive.Core.Internals.DocTemplates;

namespace Xtensive.Core.Threading
{
  /// <summary>
  /// Executes provided delegates at separate thread.
  /// </summary>
  public sealed class AsyncProcessor :
    IDisposable
  {
    private readonly Thread thread;
    private volatile Exception interceptedException;
    private readonly Queue<ITask> tasks = new Queue<ITask>();
    private readonly ManualResetEvent completionEvent = new ManualResetEvent(true);
    private volatile bool isStarted = true;
    
    /// <summary>
    /// Gets the exception intercepted during a task's execution.
    /// </summary>
    public Exception InterceptedException { get { return interceptedException;} }

    /// <summary>
    /// Gets a value indicating whether this instance has an intercepted exception.
    /// </summary>
    public bool HasError { get { return interceptedException!=null;} }

    /// <summary>
    /// Gets or sets a value indicating whether this instance executes 
    /// a task at the thread that queued it.
    /// </summary>
    public bool IsModeSynchronous { get; set; }

    /// <summary>
    /// Enqueue the specified delegate for the execution at another thread 
    /// or executes it at the calling thread.
    /// </summary>
    /// <typeparam name="T">The result type.</typeparam>
    /// <param name="function">The delegate to be executed.</param>
    /// <param name="invokeSynchronously">if set to <see langword="true"/> 
    /// then the delegate is executed at the calling thread; otherwise it is queued 
    /// to be executed at another thread.</param>
    /// <returns>The <see cref="TaskResult{T}"/> that should be used to obtain 
    /// a result of delegate's execution.</returns>
    public TaskResult<T> Execute<T>(Func<T> function, bool invokeSynchronously)
    {
      ArgumentValidator.EnsureArgumentNotNull(function, "function");
      EnsureHasNotError();
      var taskResult = new TaskResult<T>();
      var task = new Task<T>(function, taskResult);
      ProcessTask(task, invokeSynchronously);
      return taskResult;
    }

    /// <summary>
    /// Enqueue the specified delegate for the execution at another thread 
    /// or executes it at the calling thread.
    /// </summary>
    /// <param name="action">The delegate to be executed.</param>
    /// <param name="invokeSynchronously">if set to <see langword="true"/> 
    /// then the delegate is executed at the calling thread; otherwise it is queued 
    /// to be executed at another thread.</param>
    public void Execute(Action action, bool invokeSynchronously)
    {
      ArgumentValidator.EnsureArgumentNotNull(action, "action");
      EnsureHasNotError();
      var task = new Task(action);
      ProcessTask(task, invokeSynchronously);
    }

    /// <summary>
    /// Resets an error.
    /// </summary>
    public void ResetError()
    {
      interceptedException = null;
    }

    /// <summary>
    /// Waits for the completion of all queued tasks.
    /// </summary>
    public void WaitForCompletionOfAllTasks()
    {
      completionEvent.WaitOne();
    }

    #region Private / internal members

    private void ProcessTask(ITask task, bool invokeSynchronously)
    {
      if (IsModeSynchronous || invokeSynchronously) {
        WaitForCompletionOfAllTasks();
        InvokeTaskDelegate(task);
      }
      else
        EnqueueTask(task);
    }
    
    private void EnqueueTask(ITask task)
    {
      lock (tasks) {
        tasks.Enqueue(task);
        completionEvent.Reset();
      }
    }

    private void Execute()
    {
      while (isStarted) {
        ITask task;
        lock (tasks) {
          task = tasks.Count > 0 ? tasks.Dequeue() : null;
          if (task == null)
            completionEvent.Set();
        }
        if (task !=null)
          InvokeTaskDelegate(task);
        else
          Thread.Sleep(10);
      }
    }

    private void InvokeTaskDelegate(ITask task)
    {
      try {
        task.Execute();
      }
      catch(Exception e) {
        Console.WriteLine(e.Message);
        interceptedException = e;
        lock (tasks) {
          while (tasks.Count > 0)
            tasks.Dequeue().RegisterException(new TaskExecutionException(e));
          completionEvent.Set();
        }
      }
    }

    private void EnsureHasNotError()
    {
      if (HasError) {
        throw new TaskExecutionException(interceptedException);
      }
    }

    #endregion


    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    public AsyncProcessor()
    {
      thread = new Thread(Execute);
      thread.Start();
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
      isStarted = false;
      if (disposing)
        GC.SuppressFinalize(this);
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
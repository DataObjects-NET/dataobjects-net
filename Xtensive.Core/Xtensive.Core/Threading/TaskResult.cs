// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexander Nikolaev
// Created:    2009.08.12

using System;
using System.Threading;

namespace Xtensive.Threading
{
  /// <summary>
  /// Result of a task executed by <see cref="AsyncProcessor"/>.
  /// </summary>
  /// <typeparam name="T">The type of a result.</typeparam>
  [Serializable]
  public sealed class TaskResult<T>
  {
    private bool isResultReady; // Prevents possible attempt to set the result twice
    private T result;
    private Exception exception;
    private readonly ManualResetEvent resultReadyEvent = new ManualResetEvent(false);

    /// <summary>
    /// Gets the result of a task.
    /// </summary>
    /// <exception cref="Exception">An <see cref="Exception"/>, if it was caught during the
    /// asynchronous task execution.</exception>
    public T Result {
      get {
        resultReadyEvent.WaitOne();
        lock (this) { 
          // To ensure CPU cache sync 
          if (exception!=null)
            throw exception;
          return result;
        }
      }
    }

    internal void SetResult(T result)
    {
      lock (this) {
        // To ensure CPU cache sync
        if (isResultReady)
          return;
        isResultReady = true;
        this.result = result;
      }
      resultReadyEvent.Set();
    }

    internal void SetException(Exception exception)
    {
      lock (this) {
        // To ensure CPU cache sync
        if (isResultReady)
          return;
        isResultReady = true;
        this.exception = exception;
      }
      resultReadyEvent.Set();
    }
  }
}
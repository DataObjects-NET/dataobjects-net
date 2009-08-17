// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexander Nikolaev
// Created:    2009.08.12

using System;
using System.Diagnostics;
using System.Threading;

namespace Xtensive.Core.Threading
{
  /// <summary>
  /// Result of a task executed by <see cref="AsyncProcessor"/>.
  /// </summary>
  /// <typeparam name="T">The type of a result.</typeparam>
  [Serializable]
  public sealed class TaskResult<T> : ITaskResult
  {
    private T result;
    private Exception exception;
    private volatile bool isResultReady;

    /// <summary>
    /// Gets the result of a task.
    /// </summary>
    public T Result {
      get {
        while (!isResultReady)
          Thread.Sleep(10);
        if (Exception != null)
          throw Exception;
        return result;
      }
      internal set {
        result = value;
        isResultReady = true;
      }
    }

    internal Exception Exception {
      get {
        return exception;
      }
    }

    void ITaskResult.SetException(Exception exception)
    {
      this.exception = exception;
      isResultReady = true;
    }
  }
}
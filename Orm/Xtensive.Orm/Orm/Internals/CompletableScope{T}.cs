// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2010.08.04

using System;

namespace Xtensive.Orm.Internals
{
  /// <summary>
  /// A <see cref="ICompletableScope"/> implementation allowing to use delegates
  /// to define its logic.
  /// </summary>
  /// <typeparam name="T">The type of data passed between calls.</typeparam>
  internal sealed class CompletableScope<T> : ICompletableScope
  {
    private readonly T data;
    private readonly Action<T> onComplete;
    private readonly Action<T, bool> onDispose;
    private bool isDisposed;

    /// <summary>
    /// Gets a value indicating whether this instance is <see cref="Complete"/>d.
    /// </summary>
    public bool IsCompleted { get; private set; }

    /// <summary>
    /// Completes this scope by invoking "on Complete" action, if it was provided on construction.
    /// </summary>
    public void Complete()
    {
      if (IsCompleted)
        return;
      IsCompleted = true;
      if (onComplete!=null)
        onComplete.Invoke(data);
    }


    // Constructors

    /// <summary>
    /// Initializes a new instance of this class.
    /// </summary>
    /// <param name="data">The data.</param>
    /// <param name="onDispose">"On <see cref="Dispose"/>" handler. 
    /// Boolean argument there is value of <see cref="ICompletableScope.IsCompleted"/> flag.</param>
    public CompletableScope(T data, Action<T, bool> onDispose)
      : this(data, null, onDispose)
    {
    }

    /// <summary>
    /// Initializes a new instance of this class.
    /// </summary>
    /// <param name="data">The data.</param>
    /// <param name="onComplete">"On <see cref="Complete"/>" handler.</param>
    /// <param name="onDispose">"On <see cref="Dispose"/>" handler.</param>
    public CompletableScope(T data, Action<T> onComplete, Action<T, bool> onDispose)
    {
      this.data = data;
      this.onComplete = onComplete;
      this.onDispose = onDispose;
    }

    // Disposal

    /// <summary>
    /// Disposes this scope by invoking "on Dispose" action, if it was provided on construction.
    /// </summary>
    public void Dispose()
    {
      if (isDisposed)
        return;
      isDisposed = true;
      onDispose.Invoke(data, IsCompleted);
    }
  }
}
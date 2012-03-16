// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2010.08.04

using System;
using System.Diagnostics;
using Xtensive.Internals.DocTemplates;

namespace Xtensive.Disposing
{
  /// <summary>
  /// A <see cref="CompletableScope"/> implementation allowing to use delegates
  /// to define its logic.
  /// </summary>
  /// <typeparam name="T">The type of data passed between calls.</typeparam>
  public sealed class CompletableScope<T> : CompletableScope
  {
    private T data;
    private Action<T> onComplete;
    private Action<T, bool> onDispose;
    private bool isDisposed;

    /// <summary>
    /// Completes this scope by invoking "on Complete" action, if it was provided on construction.
    /// </summary>
    public override void Complete()
    {
      if (IsCompleted)
        return;
      if (onComplete!=null)
        onComplete.Invoke(data);
      base.Complete();
    }


    // Constructors

    /// <summary>
    ///	<see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="data">The data.</param>
    /// <param name="onDispose">"On <see cref="Dispose"/>" handler. 
    /// Boolean argument there is value of <see cref="CompletableScope.IsCompleted"/> flag.</param>
    public CompletableScope(T data, Action<T, bool> onDispose)
      : this(data, null, onDispose)
    {
    }

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
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
    public override void Dispose()
    {
      if (isDisposed)
        return;
      isDisposed = true;
      onDispose.Invoke(data, IsCompleted);
    }
  }
}
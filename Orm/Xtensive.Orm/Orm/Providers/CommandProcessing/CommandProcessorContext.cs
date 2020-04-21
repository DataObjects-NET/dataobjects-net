// Copyright (C) 2019 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kulakov
// Created:    2019.07.12

using System;
using System.Collections.Generic;

namespace Xtensive.Orm.Providers
{
  /// <summary>
  /// Context for <see cref="CommandProcessor"/> and its descendants used during command execution.
  /// </summary>
  public sealed class CommandProcessorContext : IDisposable
  {
    /// <summary>
    /// Gets the value indicating that partial execution is preferred if it is possible.
    /// </summary>
    public bool AllowPartialExecution { get; internal set; }

    /// <summary>
    /// Processing tasks during requests execution.
    /// </summary>
    public Queue<SqlTask> ProcessingTasks { get; internal set; }

    /// <summary>
    /// Active command which executes current portion of requests.
    /// </summary>
    public Command ActiveCommand { get; internal set; }

    /// <summary>
    /// Stores portion of data loading requests executing by <see cref="ActiveCommand"/>.
    /// </summary>
    public IList<SqlLoadTask> ActiveTasks { get; internal set; }

    /// <summary>
    /// Counts how many times command allocation got hit.
    /// </summary>
    public int ReenterCount { get; internal set; }

    internal event EventHandler Disposed;

    public void Dispose()
    {
      if (ActiveTasks!=null) {
        ActiveTasks.Clear();
        ActiveTasks = null;
      }
      if (ProcessingTasks!=null) {
        ProcessingTasks.Clear();
        ProcessingTasks = null;
      }
      if (ActiveCommand!=null) {
        ActiveCommand.Dispose();
        ActiveCommand = null;
      }
      NotifyDisposed();
    }

    private void NotifyDisposed()
    {
      if (Disposed!=null)
        Disposed(this, EventArgs.Empty);
    }

    internal CommandProcessorContext()
      : this(false)
    {
    }

    internal CommandProcessorContext(bool allowPartialExecution)
    {
      AllowPartialExecution = allowPartialExecution;
      ProcessingTasks = new Queue<SqlTask>();
      ActiveTasks = new List<SqlLoadTask>();
    }
  }
}
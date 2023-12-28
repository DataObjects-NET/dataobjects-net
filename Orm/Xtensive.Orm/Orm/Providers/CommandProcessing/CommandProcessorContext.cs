// Copyright (C) 2019-2020 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Alexey Kulakov
// Created:    2019.07.12

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xtensive.Core;

namespace Xtensive.Orm.Providers
{
  /// <summary>
  /// Context for <see cref="CommandProcessor"/> and its descendants used during command execution.
  /// </summary>
  public sealed class CommandProcessorContext : IDisposable, IAsyncDisposable
  {
    /// <summary>
    /// <see cref="ParameterContext"/> instance with the values of parameters of currently executing query.
    /// </summary>
    public ParameterContext ParameterContext { get; }

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

    internal SqlTask CurrentTask { get; set; }

    internal event EventHandler Disposed;

    public void Dispose() => DisposeImpl(false).GetAwaiter().GetResult();

    public ValueTask DisposeAsync() => DisposeImpl(true);

    private async ValueTask DisposeImpl(bool isAsync)
    {
      if (ActiveTasks != null) {
        ActiveTasks.Clear();
        ActiveTasks = null;
      }

      if (ProcessingTasks != null) {
        ProcessingTasks.Clear();
        ProcessingTasks = null;
      }

      if (ActiveCommand != null) {
        if (isAsync) {
          await ActiveCommand.DisposeAsync().ConfigureAwaitFalse();
        }
        else {
          ActiveCommand.Dispose();
        }

        ActiveCommand = null;
      }

      NotifyDisposed();
    }

    private void NotifyDisposed()
    {
      if (Disposed != null) {
        Disposed(this, EventArgs.Empty);
      }
    }

    internal CommandProcessorContext(ParameterContext parameterContext, bool allowPartialExecution = false)
    {
      ParameterContext = parameterContext;
      AllowPartialExecution = allowPartialExecution;
      ProcessingTasks = new Queue<SqlTask>();
      ActiveTasks = new List<SqlLoadTask>();
    }
  }
}

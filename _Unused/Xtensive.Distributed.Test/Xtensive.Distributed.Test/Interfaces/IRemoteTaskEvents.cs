using System;

namespace Xtensive.Distributed.Test
{
  public interface IRemoteTaskEvents
  {
    /// <summary>
    /// Signals <see cref="RemoteTask"/> that external process successfully started 
    /// and instance of task successfully created
    /// </summary>
    /// <param name="instance"><see cref="MarshalByRefObject"/> instance of task.</param>
    void TaskStarted(MarshalByRefObject instance);

    /// <summary>
    /// Signals <see cref="RemoteTask"/> that external process successfully started 
    /// but instance of task was not created
    /// </summary>
    /// <param name="exception"><see cref="Exception"/> thrown in external process while task instance creation.</param>
    void TaskStartError(Exception exception);

    /// <summary>
    /// Waits for console readers initialized. Used by remote process to ensure all console output will be
    /// handled by <see cref="RemoteTask"/>.
    /// </summary>
    void WaitForConsoleReader();

    /// <summary>
    /// Process unhandled exception if occurred.
    /// </summary>
    /// <param name="exception">Exception to process.</param>
    void ProcessUnhandledException(Exception exception);
  }
}
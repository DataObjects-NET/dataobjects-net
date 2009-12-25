// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Gamzov
// Created:    2007.09.13

using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Runtime.Remoting;
using System.Security.Permissions;
using System.Threading;
using Xtensive.Core;
using Xtensive.Core.Internals.DocTemplates;
using Xtensive.Core.Threading;
using Xtensive.Distributed.Test.Resources;

namespace Xtensive.Distributed.Test
{
  /// <summary>
  /// Manages task on the remote side.
  /// </summary>
  public class RemoteTask: MarshalByRefObject, IRemoteTaskEvents, IIdentified<Guid>, IDisposable, ISynchronizable
  {
    private static readonly TimeSpan taskStartTimeout = TimeSpan.FromSeconds(5);
    private const string StartPath = "Xtensive.Distributed.Test.TaskActivator.exe";

    private readonly RemoteFileManager fileManager;
    private Process process;
    private readonly ReaderWriterLockSlim syncRoot = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);
    private readonly Guid identifier;
    private readonly string url;
    private readonly AutoResetEvent consoleReadersInitializedWaitHandle = new AutoResetEvent(false);
    private readonly AutoResetEvent taskStartedWaitHandle = new AutoResetEvent(false);
    private MarshalByRefObject taskInstance;
    private Exception startException;
    private readonly Type instanceType;
    private ProcessPriorityClass processPriorityClass = ProcessPriorityClass.BelowNormal;
    private readonly string clientId;
    private bool disposed;

    #region Events

    /// <summary>
    /// Task writes string to console output or error output
    /// </summary>
    internal event EventHandler<ConsoleReadEventArgs> ConsoleRead;

    /// <summary>
    /// Tasks's process finished in unexpected way. 
    /// </summary>
    internal event EventHandler<ProcessAbortedEventArgs> ProcessAborted;

    #endregion

    #region Properties

    /// <summary>
    /// Gets client unique identifier.
    /// </summary>
    public string ClientId
    {
      get { return clientId; }
    }

    /// <summary>
    /// Gets <see cref="MarshalByRefObject"/> instance of task.
    /// </summary>
    public MarshalByRefObject TaskInstance
    {
      get { return taskInstance; }
    }


    /// <summary>
    /// Gets <see cref="TaskInfo"/> of task.
    /// </summary>
    public TaskInfo Info
    {
      get
      {
        using (syncRoot.ReadRegion()) {
          TaskState state;
          DateTime? startTime = null;
          if (process != null && !process.HasExited) {
            state = TaskState.Running;
            startTime = process.StartTime;
          }
          else
            state = TaskState.Stopped;
          return new TaskInfo(identifier, url, state, startTime, instanceType, processPriorityClass, clientId);
        }
      }
    }

    /// <summary>
    /// Gets instance type.
    /// </summary>
    public Type InstanceType
    {
      get { return instanceType; }
    }

    /// <summary>
    /// Gets or sets process priority class.
    /// </summary>
    public ProcessPriorityClass ProcessPriorityClass
    {
      get { return processPriorityClass; }
      set {
        using (syncRoot.ReadRegion())
        {
          processPriorityClass = value;
          if (process != null && !process.HasExited)
          {
            process.PriorityClass = value;
          }
        }
      }
    }

    /// <summary>
    /// Gets <see cref="RemoteFileManager"/> initialized with root path for the task.
    /// </summary>
    public RemoteFileManager FileManager
    {
      get { return fileManager; }
    }

    #endregion

    #region Methods

    /// <summary>
    /// Obtains a lifetime service object to control the lifetime policy for this instance.
    /// </summary>
    /// <returns>
    /// An object of type <see cref="T:System.Runtime.Remoting.Lifetime.ILease"></see> used to control the lifetime policy for this instance. This is the current lifetime service object for this instance if one exists; otherwise, a new lifetime service object initialized to the value of the <see cref="P:System.Runtime.Remoting.Lifetime.LifetimeServices.LeaseManagerPollTime"></see> property.
    /// </returns>
    /// <exception cref="T:System.Security.SecurityException">The immediate caller does not have infrastructure permission. </exception><filterpriority>2</filterpriority><PermissionSet><IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="RemotingConfiguration, Infrastructure" /></PermissionSet>
    [SecurityPermission(SecurityAction.LinkDemand, Flags=SecurityPermissionFlag.Infrastructure)]
    public override object InitializeLifetimeService()
    {
      return null;
    }



    /// <summary>
    /// Writes line to standard input of task.
    /// </summary>
    /// <param name="text">Text to write to task's input</param>
    public void WriteLine(string text)
    {
      using (syncRoot.WriteRegion()) {
        if (process == null || process.HasExited)
          throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, Strings.ExTaskIsNotRunning,url));
        using (StreamWriter sw = process.StandardInput) {
          sw.WriteLine(text);
          sw.Flush();
        }
      }
    }

    /// <summary>
    /// Writes text to standard input of task.
    /// </summary>
    /// <param name="text">Text to write to task's input</param>
    public void Write(string text)
    {
      using (syncRoot.WriteRegion()) {
        if (process == null || process.HasExited)
          throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, Strings.ExTaskIsNotRunning, url));
        using (StreamWriter sw = process.StandardInput) {
          sw.Write(text);
          sw.Flush();
        }
      }
    }

    /// <summary>
    /// Stops task execution and kills external process.
    /// </summary>
    public void Kill()
    {
      using (syncRoot.WriteRegion()) {
        if (process != null && !process.HasExited) {
          try {
            process.Kill();
            process.WaitForExit();
            process = null;
          }
          catch (Exception e) {
            Log.Error(e, Strings.LogKillProcessXError, process.ProcessName);
          }
        }
      }
    }

    /// <summary>
    /// Starts new task. Creates new external process, intercepts console input and output.
    /// </summary>
    /// <returns><see cref="MarshalByRefObject"/> instance of task.</returns>
    public MarshalByRefObject Start()
    {
      using (syncRoot.WriteRegion()) {
        if (process != null && !process.HasExited)
          throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, Strings.ExTaskIsAlreadyStarted,url));

        string commandString = url;
        var processStartInfo = new ProcessStartInfo(StartPath, commandString)
          {
            UseShellExecute = false, 
            RedirectStandardOutput = true, 
            RedirectStandardError = true, 
            RedirectStandardInput = true, 
            CreateNoWindow = true
          };
        process = new Process
          {
            EnableRaisingEvents = true, 
            StartInfo = processStartInfo
          };
        process.Exited += ProcessExited;
        process.OutputDataReceived += ProcessOutputDataReceived;
        process.ErrorDataReceived += ProcessErrorDataReceived;
        process.Start();
        process.BeginErrorReadLine();
        process.BeginOutputReadLine();
        process.PriorityClass = processPriorityClass;
        consoleReadersInitializedWaitHandle.Set();
      }
      taskStartedWaitHandle.WaitOne(taskStartTimeout, true);
      using (syncRoot.ReadRegion()) {
        if (taskInstance != null)
          return taskInstance;
        else {
          if (startException!=null)
            throw startException;
          else {
            throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, Strings.ExUnknownTaskXStartError, url));
          }
        }
      }
    }

    #endregion

    #region IRemoteTaskEvents
    /// <inheritdoc/>
    void IRemoteTaskEvents.TaskStarted(MarshalByRefObject instance)
    {
      using (syncRoot.WriteRegion())
      {
        taskInstance = instance;
        taskStartedWaitHandle.Set();
      }
    }

    /// <summary>
    /// Signals <see cref="RemoteTask"/> that external process successfully started 
    /// but instance of task was not created.
    /// </summary>
    /// <param name="exception"><see cref="Exception"/> thrown in external process while task instance creation.</param>
    void IRemoteTaskEvents.TaskStartError(Exception exception)
    {
      Log.Error(exception, Strings.LogTaskStartError);
      startException = exception;
    }

    /// <summary>
    /// Waits for console readers initialized. Used by remote process to ensure all console output will be
    /// handled by <see cref="RemoteTask"/>.
    /// </summary>
    void IRemoteTaskEvents.WaitForConsoleReader()
    {
      consoleReadersInitializedWaitHandle.WaitOne();
    }

    void IRemoteTaskEvents.ProcessUnhandledException(Exception exception)
    {
      RaiseProcessAbortedEvent(exception);
    }
    #endregion

    #region IIdentified<Guid>

    /// <summary>
    /// Gets object identifier.
    /// </summary>
    Guid IIdentified<Guid>.Identifier
    {
      get { return identifier; }
    }

    /// <summary>
    /// Gets object identifier.
    /// </summary>
    public object Identifier
    {
      get { return identifier; }
    }

    #endregion

    #region ISynchronizable

    /// <summary>
    /// Indicates whether object supports synchronized access to it, or not.
    /// </summary>
    public bool IsSynchronized
    {
      get { return true; }
    }

    /// <summary>
    /// Gets or sets the synchronization root of the object.
    /// </summary>
    public object SyncRoot
    {
      get { return syncRoot; }
    }

    #endregion

    #region Private methods

    private void ProcessExited(object sender, EventArgs e)
    {
      RaiseProcessAbortedEvent(null);
    }

    private void RaiseProcessAbortedEvent(Exception exception)
    {
      taskStartedWaitHandle.Set();
      EventHandler<ProcessAbortedEventArgs> processAborted = ProcessAborted;
      using (syncRoot.WriteRegion())
      {
        if (processAborted != null && process != null)
        {
          process.Dispose();
          process = null;
          processAborted(this, new ProcessAbortedEventArgs(exception));
        }
      }
    }

    private void ProcessErrorDataReceived(object sender, DataReceivedEventArgs e)
    {
      EventHandler<ConsoleReadEventArgs> consoleReadEventHandler = ConsoleRead;
      if (consoleReadEventHandler != null)
        consoleReadEventHandler(this, new ConsoleReadEventArgs(e.Data, true));
    }

    private void ProcessOutputDataReceived(object sender, DataReceivedEventArgs e)
    {
      EventHandler<ConsoleReadEventArgs> consoleReadEventHandler = ConsoleRead;
      if (consoleReadEventHandler != null)
        consoleReadEventHandler(this, new ConsoleReadEventArgs(e.Data, false));
    }

    #endregion

    #region Constructors

    /// <summary>
    /// Creates new instance of <see cref="RemoteTask"/>.
    /// </summary>
    /// <param name="instanceType">Type of instance.</param>
    /// <param name="url">Url of task.</param>
    /// <param name="id">Task identifier.</param>
    /// <param name="folder">Path to folder on remote machine to store task's assembly files.</param>
    /// <param name="clientId">Client's Id.</param>
    internal RemoteTask(Type instanceType, string url, Guid id, string folder, string clientId)
    {
      identifier = id;
      this.url = url;
      this.instanceType = instanceType;
      fileManager = new RemoteFileManager(folder);
      this.clientId = clientId;
      RemotingServices.Marshal(this, new UrlInfo(url).Resource);
    }

    #endregion

    #region Dispose and Finalize

    /// <summary>
    /// <see cref="DisposableDocTemplate.Dispose()" copy="true"/>
    /// </summary>
    public void Dispose()
    {
      Dispose(true);
      GC.SuppressFinalize(this);
    }

    /// <summary>
    /// <see cref="DisposableDocTemplate.Dispose(bool)" copy="true"/>
    /// </summary>
    protected virtual void Dispose(bool disposing)
    {
      using (Locker.WriteRegion(SyncRoot)) {
        if (disposed)
          return;
        disposed = true;
        RemotingServices.Disconnect(this);
        Kill();
        fileManager.DeleteAllFiles();
      }
      if (disposing) {
        consoleReadersInitializedWaitHandle.Close();
        taskStartedWaitHandle.Close();
        if (process!=null)
          process.Dispose();
      }
    }

    /// <summary>
    /// <see cref="DisposableDocTemplate.Dtor()" copy="true"/>
    /// </summary>
    ~RemoteTask()
    {
      Dispose(false);
    }

    #endregion

  }
}
// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Gamzov
// Created:    2007.09.17

using System;
using System.Diagnostics;
using Xtensive.Core;
using Xtensive.Core.Internals.DocTemplates;

namespace Xtensive.Distributed.Test
{
  /// <summary>
  /// Proxy-class for remote task. Provides methods for task management other the network.
  /// </summary>
  public class Task<T> : IDisposable
    where T : MarshalByRefObject, new()
  {
    #region Private fields

    private readonly Agent agent;
    private readonly RemoteTask remoteTask;
    private readonly FileManager fileManager;

    #endregion

    #region Events

    /// <summary>
    /// Task writes string to console output or error output
    /// </summary>
    public event EventHandler<ConsoleReadEventArgs> ConsoleRead;

    /// <summary>
    /// Tasks's process finished in unexpected way. 
    /// </summary>
    public event EventHandler<ProcessAbortedEventArgs> ProcessAborted;

    #endregion

    #region Properties

    /// <summary>
    /// Gets url of remote task.
    /// </summary>
    public string Url
    {
      get { return remoteTask.Info.Url; }
    }

    /// <summary>
    /// Gets <see cref="FileManager"/> initialized by this task. 
    /// </summary>
    public FileManager FileManager
    {
      get { return fileManager; }
    }

    public ProcessPriorityClass ProcessPriorityClass
    {
      get { return remoteTask.ProcessPriorityClass; }
      set { remoteTask.ProcessPriorityClass = value; }
    }

    #endregion

    #region Methods

    /// <summary>
    /// Creates task instance on <see cref="Agent"/>.
    /// </summary>
    /// <returns></returns>
    public T Start()
    {
      return (T) remoteTask.Start();
    }

    /// <summary>
    /// Stops task and kill corresponding process on <see cref="Agent"/>.
    /// </summary>
    public void Kill()
    {
      remoteTask.Kill();
    }

    #endregion

    #region Private methods

    private void RemoteTaskConsoleRead(object sender, ConsoleReadEventArgs e)
    {
      EventHandler<ConsoleReadEventArgs> consoleRead = ConsoleRead;
      if (consoleRead!=null) {
        consoleRead(this, e);
      }
    }

    private void RemoteProcessAborted(object sender, ProcessAbortedEventArgs e)
    {
      EventHandler<ProcessAbortedEventArgs> processAborted = ProcessAborted;
      if (processAborted!=null) {
        processAborted(this, e);
      }
    }

    #endregion

    #region Constructors

    /// <summary>
    /// Creates new instance of <see cref="Task{T}"/>.
    /// </summary>
    /// <param name="agentUrl">Url of agent to create task on.</param>
    /// <param name="clientId">Client unique identifier.</param>
    internal Task(string agentUrl, string clientId)
    {
      ArgumentValidator.EnsureArgumentNotNullOrEmpty(agentUrl, "agentUrl");
      agent = (Agent) Activator.GetObject(typeof (Agent), agentUrl);
      remoteTask = agent.CreateTask(typeof (T), clientId);
      fileManager = new FileManager(remoteTask.FileManager);
      remoteTask.ConsoleRead += RemoteTaskConsoleRead;
      remoteTask.ProcessAborted += RemoteProcessAborted;
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
    protected void Dispose(bool disposing)
    {
      if (disposing) {
        try {
          remoteTask.Dispose();
        }
        catch (Exception e) {
          Log.Info(e, Resources.Strings.LogUnableToDisposeRemoteTask);
        }
      }
    }

    /// <summary>
    /// <see cref="DisposableDocTemplate.Dtor()" copy="true"/>
    /// </summary>
    ~Task()
    {
      Dispose(false);
    }

    #endregion
  }
}
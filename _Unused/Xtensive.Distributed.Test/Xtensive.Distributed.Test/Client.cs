// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Gamzov
// Created:    2007.09.13

using System;
using System.Collections.Generic;
using System.Threading;
using Xtensive.Core;
using Xtensive.Core.Collections;
using Xtensive.Core.Internals.DocTemplates;
using Xtensive.Core.Threading;
using Xtensive.Distributed.Test.Resources;


namespace Xtensive.Distributed.Test
{
  /// <summary>
  /// Distributed test client. 
  /// Communicates with <see cref="Agent"/>s and <see cref="Server"/>, 
  /// creates <see cref="Task{T}"/> objects and manage them.
  /// </summary>
  public class Client : IDisposable, ISynchronizable
  {
    #region Private fields

    private readonly UrlInfo serverUrl;
    private readonly Server server;
    private readonly List<IDisposable> tasks = new List<IDisposable>();
    private string clientId;
    private readonly SetSlim<string> affectedAgents = new SetSlim<string>();
    private readonly Thread heartbeatThread;
    private readonly ReaderWriterLockSlim syncRoot = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);

    #endregion

    #region Properties

    /// <summary>
    /// Client's unique identifier.
    /// </summary>
    public string ClientId
    {
      get
      {
        using (syncRoot.ReadRegion()) {
          if (clientId==null)
            ClientId = System.Net.Dns.GetHostName() + @"\" + Guid.NewGuid();
          return clientId;
        }
      }
      set
      {
        using (syncRoot.WriteRegion()) {
          ArgumentValidator.EnsureArgumentNotNullOrEmpty(value, "value");
          if (clientId!=null)
            Exceptions.AlreadyInitialized("ClientId");
          clientId = value;
        }
      }
    }

    public AgentInfo[] Agents
    {
      get { return server.AgentInfos; }
    }

    #endregion

    #region ISynchronizable Members

    ///<summary>
    ///
    ///            Indicates whether object supports synchronized access to it, or not.
    ///            
    ///</summary>
    ///
    public bool IsSynchronized
    {
      get { return true; }
    }

    #endregion

    #region IHasSyncRoot Members

    ///<summary>
    ///
    ///            Gets or sets the synchronization root of the object.
    ///            
    ///</summary>
    ///
    public object SyncRoot
    {
      get { return syncRoot; }
    }

    #endregion

    #region Methods

    /// <summary>
    /// Creates new <see cref="Task{T}"/>.
    /// </summary>
    /// <typeparam name="T">Type of task to create. Must be inherited from <see cref="MarshalByRefObject"/>.</typeparam>
    /// <param name="url">Url to deploy task to.</param>
    /// <returns>Task instance.</returns>
    public Task<T> CreateTask<T>(string url)
      where T : MarshalByRefObject, new()
    {
      ArgumentValidator.EnsureArgumentNotNullOrEmpty(url, "url");

      bool found = false;
      foreach (AgentInfo agentInfo in Agents) {
        if (agentInfo.Url==url) {
          found = true;
          break;
        }
      }
      if (!found)
        throw new InvalidOperationException(Strings.ExAgentNotFound);
      Task<T> task = new Task<T>(url, ClientId);
      task.ConsoleRead += TaskConsoleRead;
      using (syncRoot.WriteRegion()) {
        tasks.Add(task);
        affectedAgents.Add(url);
      }
      return task;
    }

    /// <summary>
    /// Creates new <see cref="Task{T}"/>.
    /// </summary>
    /// <typeparam name="T">Type of task to create. Must be inherited from <see cref="MarshalByRefObject"/>.</typeparam>
    /// <returns>Task instance.</returns>
    /// <remarks>Deploys task on first free agent.</remarks>
    public Task<T> CreateTask<T>() where T : MarshalByRefObject, new()
    {
      AgentInfo[] agents = Agents;
      if (agents.Length==0)
        throw new InvalidOperationException(Strings.ExAgentNotFound);
      return CreateTask<T>(agents[0].Url);
    }

    #endregion

    #region Private methods

    private static void TaskConsoleRead(object sender, ConsoleReadEventArgs e)
    {
      Console.ForegroundColor = e.IsError ? ConsoleColor.Red : ConsoleColor.White;
      Console.WriteLine(e.Message);
    }

    private void HeartbeatThread()
    {
      while (true) {
        using (syncRoot.ReadRegion()) {
          foreach (string affectedAgent in affectedAgents) {
            try {
              var agent = (Agent) Activator.GetObject(typeof (Agent), affectedAgent);
              agent.Heartbeat(ClientId);
            }
            catch (Exception e) {
              Log.Error(e, Strings.LogAgentXHeartbeatFailedE, affectedAgent);
            }
          }
        }
        Thread.Sleep(Agent.ClientKeepAliveTimeout);
      }
    }

    #endregion

    #region Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="serverUrl">Url of server.</param>
    public Client(string serverUrl)
    {
      ArgumentValidator.EnsureArgumentNotNullOrEmpty(serverUrl, "serverUrl");

      this.serverUrl = new UrlInfo(serverUrl);
      server = (Server) Activator.GetObject(typeof (Server), this.serverUrl.Url);
      heartbeatThread = new Thread(HeartbeatThread);
      heartbeatThread.Start();
    }

    #endregion

    #region Dispose and finalize

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
      heartbeatThread.Abort();
      if (disposing) {
        using (syncRoot.WriteRegion()) {
          foreach (IDisposable task in tasks) {
            task.Dispose();
          }
          tasks.Clear();
        }
      }
    }

    /// <summary>
    /// <see cref="DisposableDocTemplate.Dtor()" copy="true"/>
    /// </summary>
    ~Client()
    {
      Dispose(false);
    }

    #endregion
  }
}
// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Gamzov
// Created:    2007.09.26

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Http;
using System.Runtime.Remoting.Channels.Tcp;
using System.Security.Permissions;
using System.Threading;
using Xtensive.Core;
using Xtensive.Core.Internals.DocTemplates;
using Xtensive.Core.Threading;
using Xtensive.Distributed.Test.Resources;

namespace Xtensive.Distributed.Test
{
  /// <summary>
  /// Testing agent. 
  /// Executes the testing tasks provided by <see cref="Server"/>.
  /// </summary>
  public class Agent : MarshalByRefObject, IDisposable, ISynchronizable
  {
    #region Constants

    private const int ProcessorLoadQueueLenght = 10;

    /// <summary>
    /// Period of time client sends "KeepAlive" requests.
    /// </summary>
    public static readonly TimeSpan ClientKeepAliveTimeout = TimeSpan.FromSeconds(10);

    #endregion

    #region Private fields

    private readonly Dictionary<Guid, RemoteTask> tasks = new Dictionary<Guid, RemoteTask>();
    private readonly UrlInfo serverUrlInfo;
    private readonly Thread heartbeatThread;
    private readonly Server server;
    private readonly UrlInfo localUrlInfo;
    private readonly TimeSpan heartbeatInterval = TimeSpan.FromSeconds(5);
    private readonly string rootFolder;
    private readonly IChannel outcomeChannel;
    private readonly IChannel incomeChannel;
    private readonly Queue<float> processorUsage = new Queue<float>();

    private readonly PerformanceCounter processorCounter = new PerformanceCounter("processor", "% Processor Time", "_total", true);

    private readonly PerformanceCounter memoryCounter = new PerformanceCounter("memory", "Available MBytes", "", true);
    private readonly ReaderWriterLockSlim syncRoot = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);
    private bool disposed;
    private readonly Dictionary<string, Pair<DateTime, string>> clients = new Dictionary<string, Pair<DateTime, string>>();
    private readonly Thread cleanupThread;

    #endregion

    #region Public methods

    /// <summary>
    /// Creates new task on agent.
    /// </summary>
    /// <returns><see cref="RemoteTask"/> instance.</returns>
    public RemoteTask CreateTask(Type taskType, string clientId)
    {
      Heartbeat(clientId);
      Guid taskId = Guid.NewGuid();
      string url = localUrlInfo + "/" + taskId;
      var task = new RemoteTask(taskType, url, taskId, Path.Combine(rootFolder, taskId.ToString()), clientId);
      syncRoot.ExecuteWriter(() => tasks.Add(taskId, task));
      return task;
    }

    /// <summary>
    /// Gets <see cref="RemoteTask"/>s deployed on the agent.-
    /// </summary>
    public TaskInfo[] Tasks
    {
      get
      {
        var result = new List<TaskInfo>(tasks.Count);
        using (syncRoot.ReadRegion()) {
          foreach (KeyValuePair<Guid, RemoteTask> task in tasks) {
            result.Add(task.Value.Info);
          }
          return result.ToArray();
        }
      }
    }

    /// <inheritdoc/>
    public bool IsSynchronized
    {
      get { return true; }
    }

    /// <inheritdoc/>
    public object SyncRoot
    {
      get { return syncRoot; }
    }

    /// <inheritdoc/>
    [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.Infrastructure)]
    public override object InitializeLifetimeService()
    {
      return null;
    }

    #endregion

    #region Internal

    internal void KillClientTasks(string clientId)
    {
      using (syncRoot.ReadRegion()) {
        var taskIds = new List<Guid>();
        foreach (KeyValuePair<Guid, RemoteTask> task in tasks) {
          if (task.Value.ClientId==clientId) {
            taskIds.Add(task.Key);
            task.Value.Dispose();
          }
        }
        using (syncRoot.WriteRegion()) {
          foreach (Guid taskId in taskIds) {
            tasks.Remove(taskId);
          }
        }
      }
    }

    /// <summary>
    /// Gets heartbeats from clients to be sure client still alive.
    /// </summary>
    /// <param name="clientId">Client identifier.</param>
    public void Heartbeat(string clientId)
    {
      using (syncRoot.WriteRegion()) {
        clients[clientId] = new Pair<DateTime, string>(DateTime.Now, clientId);
      }
    }

    #endregion

    #region Private methods

    /// <summary>
    /// Calls server with specified period of time and sends agent's statistic such as processor load, memory usage.
    /// </summary>
    private void HeartbeatSenderThread()
    {
      while (true) {
        try {
          processorUsage.Enqueue(processorCounter.NextValue());
          while (processorUsage.Count > ProcessorLoadQueueLenght) {
            processorUsage.Dequeue();
          }
          float usage = 0f;
          if (processorUsage.Count > 0) {
            foreach (float processorUsageValue in processorUsage) {
              usage += processorUsageValue;
            }
            usage /= processorUsage.Count;
          }
          var info = new AgentInfo(localUrlInfo.Url, usage, memoryCounter.NextValue());
          server.Heartbeat(info, Tasks);
        }
        catch (Exception e) {
          if (!(e is ThreadAbortException))
            Log.Error(e, Strings.LogServerHeartbeatErrorX);
        }
        Thread.Sleep(heartbeatInterval);
      }
    }

    private void CleanupDeadClientsThread()
    {
      TimeSpan sleepTimeout = TimeSpan.FromMilliseconds(ClientKeepAliveTimeout.TotalMilliseconds / 2);
      while (true) {
        CleanupDeadClients();
        Thread.Sleep(sleepTimeout);
      }
    }

    private void CleanupDeadClients()
    {
      using (syncRoot.WriteRegion()) {
        var keys = new List<string>();
        DateTime now = DateTime.Now;
        foreach (KeyValuePair<string, Pair<DateTime, string>> clientPair in clients) {
          if ((now - clientPair.Value.First) > ClientKeepAliveTimeout) {
            keys.Add(clientPair.Key);
          }
        }
        foreach (string key in keys) {
          KillClientTasks(key);
          clients.Remove(key);
        }
      }
    }

    #endregion

    #region Constructors

    /// <summary>
    /// Creates new instance of <see cref="Agent"/>.
    /// </summary>
    /// <param name="serverUrl">String, representing server's url. For example "tcp://127.0.0.1/Server"</param>
    /// will be sent to <see cref="Server"/> in <see cref="HeartbeatSenderThread"/>.</param>
    /// <param name="rootFolder">Local path where to create tasks.</param>
    public Agent(string serverUrl, string rootFolder)
      : this(serverUrl, rootFolder, TimeSpan.FromSeconds(10))
    {
    }

    /// <summary>
    /// Creates new instance of <see cref="Agent"/>.
    /// </summary>
    /// <param name="serverUrl">String, representing server's url. For example "tcp://127.0.0.1/Server"</param>
    /// will be sent to <see cref="Server"/> in <see cref="HeartbeatSenderThread"/>.</param>
    /// <param name="rootFolder">Local path where to create tasks.</param>
    /// <param name="heartbeatInterval"><see cref="TimeSpan"/> describes how often agent will send heartbeats to server.</param>
    public Agent(string serverUrl, string rootFolder, TimeSpan heartbeatInterval)
      :
        this(serverUrl, rootFolder, heartbeatInterval, Protocol.Tcp)
    {
    }

    /// <summary>
    /// Creates new instance of <see cref="Agent"/>.
    /// </summary>
    /// <param name="serverUrl">String, representing server's url. For example "tcp://127.0.0.1/Server"</param>
    /// will be sent to <see cref="Server"/> in <see cref="HeartbeatSenderThread"/>.</param>
    /// <param name="rootFolder">Local path where to create tasks.</param>
    /// <param name="heartbeatInterval"><see cref="TimeSpan"/> describes how often agent will send heartbeats to server.</param>
    public Agent(string serverUrl, string rootFolder, TimeSpan heartbeatInterval, Protocol protocol)
    {
      ArgumentValidator.EnsureArgumentNotNullOrEmpty(serverUrl, "serverUrl");
      ArgumentValidator.EnsureArgumentNotNullOrEmpty(rootFolder, "rootFolder");

      serverUrlInfo = new UrlInfo(serverUrl);
      if (!serverUrlInfo.Protocol.Equals("tcp", StringComparison.CurrentCultureIgnoreCase)
        && !serverUrlInfo.Protocol.Equals("http", StringComparison.CurrentCultureIgnoreCase))
        throw new ArgumentException(string.Format(
          Strings.ExInvalidProtocol, serverUrlInfo.Protocol), "serverUrl");

      IDictionary props = new Hashtable();
      props["typeFilterLevel"] = "Full";
      int incomePort;
      switch (protocol) {
        case Protocol.Tcp:
          var binarySinkProvider = new BinaryServerFormatterSinkProvider(props, null);
          var tcpServerChannel = new TcpServerChannel(Guid.NewGuid().ToString(), 0, binarySinkProvider);
          incomePort = new UrlInfo(tcpServerChannel.GetChannelUri() + "/").Port;
          incomeChannel = tcpServerChannel;
          break;
        case Protocol.Http:
          var soapSinkProvider = new SoapServerFormatterSinkProvider(props, null);
          var httpServerChannel = new HttpServerChannel(Guid.NewGuid().ToString(), 0, soapSinkProvider);
          incomePort = new UrlInfo(httpServerChannel.GetChannelUri() + "/").Port;
          incomeChannel = httpServerChannel;
          break;
        default:
          throw new InvalidOperationException(string.Format(
            Strings.ExInvalidProtocol, localUrlInfo.Protocol));
      }
      localUrlInfo =
        new UrlInfo(
          string.Format(CultureInfo.InvariantCulture, "{0}://{1}:{2}/TestAgent_{3}", protocol, System.Net.Dns.GetHostName(), incomePort,
            Guid.NewGuid()));
      RemotingServices.Marshal(this, localUrlInfo.Resource);
      ChannelServices.RegisterChannel(incomeChannel, false);

      switch (serverUrlInfo.Protocol) {
        case "tcp":
          outcomeChannel = new TcpClientChannel(Guid.NewGuid().ToString(), null);
          break;
        case "http":
          outcomeChannel = new HttpClientChannel(Guid.NewGuid().ToString(), null);
          break;
        default:
          throw new InvalidOperationException(string.Format(
            Strings.ExInvalidProtocol, serverUrlInfo.Protocol));
      }
      ChannelServices.RegisterChannel(outcomeChannel, false);

      server = (Server) Activator.GetObject(typeof (Server), serverUrlInfo.Url);
      this.heartbeatInterval = heartbeatInterval;
      this.rootFolder = Path.GetFullPath(rootFolder);
      if (Directory.Exists(this.rootFolder)) {
        foreach (FileSystemInfo fileSystemInfo in new DirectoryInfo(this.rootFolder).GetFileSystemInfos()) {
          fileSystemInfo.Delete();
        }
      }
      else {
        Directory.CreateDirectory(this.rootFolder);
      }
      heartbeatThread = new Thread(HeartbeatSenderThread);
      heartbeatThread.Start();
      cleanupThread = new Thread(CleanupDeadClientsThread);
      cleanupThread.Start();
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
      cleanupThread.Abort();
      if (!disposed && disposing) {
        disposed = true;
        ChannelServices.UnregisterChannel(incomeChannel);
        ChannelServices.UnregisterChannel(outcomeChannel);
        RemotingServices.Disconnect(this);
        processorCounter.Dispose();
        memoryCounter.Dispose();
        using (syncRoot.WriteRegion()) {
          foreach (KeyValuePair<Guid, RemoteTask> task in tasks) {
            task.Value.Dispose();
          }
        }
      }
    }

    /// <summary>
    /// <see cref="DisposableDocTemplate.Dtor()" copy="true"/>
    /// </summary>
    ~Agent()
    {
      Dispose(false);
    }

    #endregion
  }
}
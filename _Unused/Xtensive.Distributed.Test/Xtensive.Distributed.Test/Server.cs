// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Gamzov
// Created:    2007.09.19

using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Http;
using System.Runtime.Remoting.Channels.Tcp;
using System.Threading;
using Xtensive.Core;
using Xtensive.Core.Internals.DocTemplates;
using Xtensive.Core.Threading;
using Xtensive.Distributed.Test.Resources;

namespace Xtensive.Distributed.Test
{
  /// <summary>
  /// Distributed testing server.
  /// Manages available testing <see cref="Agent"/>s, 
  /// gathers heartbeats and agents' statistics.
  /// </summary>
  public class Server : MarshalByRefObject, IDisposable, ISynchronizable
  {
    #region Private fields

    /// <summary>
    /// Key is agent's URL, value is pair of heartbeat time and <see cref="AgentInfo"/>.
    /// </summary>
    private readonly Dictionary<string, Triplet<DateTime, AgentInfo, TaskInfo[]>> agents = new Dictionary<string, Triplet<DateTime, AgentInfo, TaskInfo[]>>();

    private readonly ReaderWriterLockSlim syncRoot = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);
    private readonly Thread clearThread;
    private readonly TimeSpan agentDeathTimeout;
    private readonly UrlInfo urlInfo;
    private readonly IChannel channel;
    private bool disposed;

    #endregion

    #region Properties

    /// <summary>
    /// Gets timeout for wait for heartbeats from agents. If no heartbeat received in specified 
    /// period of time, <see cref="Agent"/> will be removed from cluster.
    /// </summary>
    public TimeSpan AgentDeathTimeout
    {
      get { return agentDeathTimeout; }
    }


    /// <summary>
    /// Gets list of <see cref="AgentInfo"/> ordered by agent's processor and memory load.
    /// </summary>
    public AgentInfo[] AgentInfos
    {
      get
      {
        using (syncRoot.ReadRegion()) {
          if (disposed)
            throw new ObjectDisposedException(typeof (Server).ToString());
          var agentList = new List<AgentInfo>(agents.Count);
          foreach (KeyValuePair<string, Triplet<DateTime, AgentInfo, TaskInfo[]>>agentPair in agents) {
            agentList.Add(agentPair.Value.Second);
          }
          agentList.Sort((a, b) => -a.Load.CompareTo(b.Load));
          return agentList.ToArray();
        }
      }
    }

    #endregion

    #region Methods

    /// <summary>
    /// Used by <see cref="Agent"/>s to signal server about state and load.
    /// </summary>
    /// <param name="info"><see cref="AgentInfo"/> with information about agent's state and load.</param>
    /// <param name="tasks">List of <see cref="TaskInfo"/> currently exist on <see cref="Agent"/>.</param>
    public void Heartbeat(AgentInfo info, TaskInfo[] tasks)
    {
      using (syncRoot.WriteRegion()) {
        agents[info.Url] = new Triplet<DateTime, AgentInfo, TaskInfo[]>(DateTime.Now, info, tasks);
      }
    }

    /// <summary>
    /// Obtains a lifetime service object to control the lifetime policy for this instance.
    /// </summary>
    /// <returns>
    /// An object of type <see cref="T:System.Runtime.Remoting.Lifetime.ILease"></see> used to control the lifetime policy for this instance. This is the current lifetime service object for this instance if one exists; otherwise, a new lifetime service object initialized to the value of the <see cref="P:System.Runtime.Remoting.Lifetime.LifetimeServices.LeaseManagerPollTime"></see> property.
    /// </returns>
    /// <exception cref="T:System.Security.SecurityException">The immediate caller does not have infrastructure permission. </exception><filterpriority>2</filterpriority><PermissionSet><IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="RemotingConfiguration, Infrastructure" /></PermissionSet>
    public override object InitializeLifetimeService()
    {
      return null;
    }

    #endregion

    #region ISynchronizable & IHasSyncRoot

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

    private void CleanupDeadAgentsThread()
    {
      TimeSpan sleepTimeout = TimeSpan.FromMilliseconds(agentDeathTimeout.TotalMilliseconds / 2);
      while (true) {
        CleanupDeadAgents();
        Thread.Sleep(sleepTimeout);
      }
    }

    private void CleanupDeadAgents()
    {
      using (syncRoot.WriteRegion()) {
        var keys = new List<string>();
        DateTime now = DateTime.Now;
        foreach (KeyValuePair<string, Triplet<DateTime, AgentInfo, TaskInfo[]>> agentPair in agents) {
          if ((now - agentPair.Value.First) > agentDeathTimeout) {
            keys.Add(agentPair.Key);
          }
        }
        foreach (string key in keys) {
          agents.Remove(key);
        }
      }
    }

    #endregion

    #region Constructors

    /// <summary>
    /// Creates new instance of <see cref="Server"/>.
    /// </summary>
    /// <param name="url">Url where sever listen for incoming connection from <see cref="Client"/>s and <see cref="Agent"/>s</param>
    public Server(string url)
      : this(url, TimeSpan.FromSeconds(60))
    {
    }

    /// <summary>
    /// Creates new instance of <see cref="Server"/>.
    /// </summary>
    /// <param name="url">Url where sever listen for incoming connection from <see cref="Client"/>s and <see cref="Agent"/>s</param>
    /// <param name="agentDeathTimeout">Time to wait for agent's heartbeat before remove it from cluster.</param>
    public Server(string url, TimeSpan agentDeathTimeout)
    {
      ArgumentValidator.EnsureArgumentNotNullOrEmpty(url, "url");

      urlInfo = new UrlInfo(url);
      if (!urlInfo.Protocol.Equals("tcp", StringComparison.CurrentCultureIgnoreCase)
        && !urlInfo.Protocol.Equals("http", StringComparison.CurrentCultureIgnoreCase))
        throw new ArgumentException(string.Format(
          Strings.ExInvalidProtocol, urlInfo.Protocol), "url");
      IDictionary props = new Hashtable();
      props["typeFilterLevel"] = "Full";
      switch (urlInfo.Protocol) {
        case "tcp":
          var binarySinkProvider = new BinaryServerFormatterSinkProvider(props, null);
          channel = new TcpServerChannel(Guid.NewGuid().ToString(), urlInfo.Port, binarySinkProvider);
          break;
        case "http":
          var soapSinkProvider = new SoapServerFormatterSinkProvider(props, null);
          channel = new HttpServerChannel(Guid.NewGuid().ToString(), urlInfo.Port, soapSinkProvider);
          break;
        default:
          throw new InvalidOperationException(string.Format(Strings.ExInvalidProtocol, urlInfo.Protocol));
      }
      ChannelServices.RegisterChannel(channel, false);
      RemotingServices.Marshal(this, urlInfo.Resource);
      clearThread = new Thread(CleanupDeadAgentsThread);
      clearThread.Start();
      this.agentDeathTimeout = agentDeathTimeout;
    }

    #endregion

    #region Dispose and finalize

    /// <summary>
    /// <see cref="DisposableDocTemplate.Dispose()" copy="true"/>
    /// </summary>
    public void Dispose()
    {
      GC.SuppressFinalize(this);
      Dispose(true);
    }

    /// <summary>
    /// <see cref="DisposableDocTemplate.Dispose(bool)" copy="true"/>
    /// </summary>
    protected virtual void Dispose(bool disposing)
    {
      using (syncRoot.WriteRegion()) {
        if (disposed)
          return;
        disposed = true;
        ChannelServices.UnregisterChannel(channel);
        RemotingServices.Disconnect(this);
        clearThread.Abort();
      }
    }

    /// <summary>
    /// <see cref="DisposableDocTemplate.Dtor()" copy="true"/>
    /// </summary>
    ~Server()
    {
      Dispose(false);
    }

    #endregion
  }
}
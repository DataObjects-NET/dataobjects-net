// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2007.10.23

using System;
using System.Configuration;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.ServiceProcess;
using Xtensive.Core.Internals.DocTemplates;
using Xtensive.Distributed.Test.AgentService.Resources;
using System.IO;

namespace Xtensive.Distributed.Test.AgentService
{
  /// <summary>
  /// Agent service.
  /// </summary>
  public class Service : ServiceBase
  {
    private ServiceStatus serviceStatus;
    private Agent agent;

    [DllImport("ADVAPI32.DLL", EntryPoint = "SetServiceStatus")]
    private static extern bool SetServiceStatus(IntPtr hServiceStatus, ServiceStatus lpServiceStatus);

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor"/>
    /// </summary>
    public Service()
    {
      ServiceName = Strings.ServiceName;
    }

    protected override void OnStart(string[] arguments)
    {
      IntPtr handle = ServiceHandle;
      serviceStatus.currentState = (int)State.StartPending;
      SetServiceStatus(handle, serviceStatus);
      EventLog.WriteEntry(Strings.ServiceName, Strings.ServiceStarting, EventLogEntryType.Information);

      string serverUrl = ConfigurationManager.AppSettings["ServerUrl"];
      string rootFolder = ConfigurationManager.AppSettings["RootFolder"]??Environment.CurrentDirectory;
      if (!Path.IsPathRooted(rootFolder))
        rootFolder = Path.GetFullPath(rootFolder);
      agent = new Agent(serverUrl, rootFolder);

      EventLog.WriteEntry(Strings.ServiceName, Strings.ServiceStarted, EventLogEntryType.Information);
      serviceStatus.currentState = (int)State.Running;
      SetServiceStatus(handle, serviceStatus);
    }

    protected override void OnStop()
    {
      EventLog.WriteEntry(Strings.ServiceName, Strings.ServiceStopping, EventLogEntryType.Information);
      if (agent!=null) {
        agent.Dispose();
        agent = null;
      }
      // Indicate a successful exit.
      ExitCode = 0;
    }
  }
}
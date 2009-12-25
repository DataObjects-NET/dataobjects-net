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
using Xtensive.Distributed.Test.ServerService.Resources;

namespace Xtensive.Distributed.Test.ServerService
{
  /// <summary>
  /// Server service.
  /// </summary>
  public class Service : ServiceBase
  {
    private ServiceStatus serviceStatus;
    private Server server;

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

      string url = ConfigurationManager.AppSettings["ServerUrl"];
      server = new Server(url);

      EventLog.WriteEntry(Strings.ServiceName, Strings.ServiceStarted, EventLogEntryType.Information);
      serviceStatus.currentState = (int)State.Running;
      SetServiceStatus(handle, serviceStatus);
    }

    protected override void OnStop()
    {
      EventLog.WriteEntry(Strings.ServiceName, Strings.ServiceStopping, EventLogEntryType.Information);
      if (server!=null) {
        server.Dispose();
        server = null;
      }
      // Indicate a successful exit.
      ExitCode = 0;
    }
  }
}
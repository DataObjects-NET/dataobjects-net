using System;
using System.Collections;
using System.Globalization;
using System.IO;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Http;
using System.Runtime.Remoting.Channels.Tcp;
using System.Threading;
using Xtensive.Core;
using Xtensive.Core.Reflection;
using Xtensive.Distributed.Test;
using System.Reflection;
using Xtensive.Distributed.Test.TaskActivator.Resources;

namespace Xtensive.Distributed.Test.TaskActivator
{
  /// <summary>
  /// Task activator.
  /// </summary>
  public static class Program
  {
    private static IRemoteTaskEvents taskEvents;
    private static IChannel outcomeChannel;
    private static IChannel incomeChannel;

    /// <summary>
    /// Starts new task process. Loads assemblies and executes entry point type.
    /// </summary>
    /// <param name="args">An array of parameters. 
    /// A single argument (URL of entry point type) is expected.</param>
    private static int Main(string[] args)
    {
      if (args.Length!=1) {
        Log.Error(Strings.LogInvalidCommandLineArguments);
        return (int)ExecutionResultType.Error;
      }
      string taskUrl = args[0];
      IDictionary props = new Hashtable();
      props["typeFilterLevel"] = "Full";
      try {
        string protocol = new UrlInfo(taskUrl).Protocol;
        switch (protocol) {
        case "tcp":
          outcomeChannel = new TcpClientChannel(Guid.NewGuid().ToString(), null);
          var binarySinkProvider = new BinaryServerFormatterSinkProvider(props, null);
          incomeChannel = new TcpServerChannel(Guid.NewGuid().ToString(), 0, binarySinkProvider);
          break;
        case "http":
          outcomeChannel = new HttpClientChannel(Guid.NewGuid().ToString(), null);
          var soapSinkProvider = new SoapServerFormatterSinkProvider(props, null);
          incomeChannel = new HttpServerChannel(Guid.NewGuid().ToString(), 0, soapSinkProvider);
          break;
        default:
          Log.Error(Strings.LogInvalidProtocol, protocol);
          return (int)ExecutionResultType.Error;
        }
        ChannelServices.RegisterChannel(outcomeChannel, false);
        ChannelServices.RegisterChannel(incomeChannel, false);
      }
      catch (Exception e) {
        Log.Error(e, Strings.LogCantInitializeRemotingChannel);
        return (int)ExecutionResultType.Error;
      }

      RemoteTask task;
      try {
        task = (RemoteTask)Activator.GetObject(typeof(RemoteTask), taskUrl);
      }
      catch (Exception e) {
        Log.Error(e, Strings.CantConnectToAgentX, taskUrl);
        return (int)ExecutionResultType.Error;
      }

      AppDomain.CurrentDomain.UnhandledException += AppDomainUnhandledException;
      taskEvents = task;

      Type instanceType = task.InstanceType;
      string path = task.FileManager.RootPath;
      taskEvents.WaitForConsoleReader();

      string[] files = Directory.GetFiles(Path.GetFullPath(path), "*.dll", SearchOption.TopDirectoryOnly);
      foreach (string file in files)
        try {
          Assembly.LoadFile(file);
        }
        catch (Exception e) {
          taskEvents.TaskStartError(new InvalidOperationException(string.Format(
            Strings.ExCantLoadAssemblies, file), e));
          return (int)ExecutionResultType.Error;
        }
      object obj;
      try {
        obj = instanceType.InvokeMember("", BindingFlags.CreateInstance, null, null, new object[0], CultureInfo.CurrentCulture);
      }
      catch (Exception e) {
        taskEvents.TaskStartError(new InvalidOperationException(string.Format(
          Strings.ExCantCreateInstanceOfTypeX, instanceType.GetShortName()), e));
        return (int)ExecutionResultType.Error;
      }
      if (!(obj is MarshalByRefObject)) {
        taskEvents.TaskStartError(new InvalidOperationException(string.Format(
          Strings.ExTaskInstanceIsNotMarshalByRefObject, instanceType.GetShortName())));
        return (int)ExecutionResultType.Error;
      }
      taskEvents.TaskStarted((MarshalByRefObject)obj);
      Thread.Sleep(Timeout.Infinite);
      return (int)ExecutionResultType.Success;
    }

    static void AppDomainUnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
      Log.Error(e.ExceptionObject as Exception, Strings.LogUnhandledExceptionX );
      taskEvents.ProcessUnhandledException(e.ExceptionObject as Exception);
      Environment.Exit((int)ExecutionResultType.Success);
    }
  }
}
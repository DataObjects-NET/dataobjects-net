// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Gamzov
// Created:    2007.10.05

using System;
using System.Threading;

namespace Xtensive.Distributed.Test.Tests.RemoteAssembly
{
  public class ConsoleTest: MarshalByRefObject, IDisposable
  {
    public const string ConsoleErrorString = "ConsoleErrorString";
    public const string ConsoleOutputString = "ConsoleOutputString";
    private Thread thread;

    public ConsoleTest()
    {
      Console.Error.WriteLine(ConsoleErrorString);
      Console.WriteLine(ConsoleOutputString);
    }

    public void WriteToConsole(string s)
    {
      thread = new Thread(Write);
      thread.Start(s);
    }

    private void Write(object str)
    {
      bool error = false;
      while (true) {
        if (error) {
          Console.Error.WriteLine("ERROR: " + (string)str);
        }
        else {
          Console.WriteLine(str);
        }
        error = !error;
        Thread.Sleep(1000);
      }
    }


    public void Dispose()
    {
      GC.SuppressFinalize(this);
      Dispose(true);
    }

    protected void Dispose(bool disposing)
    {
      if (thread != null) {
        thread.Abort();
      }
    }

    ~ConsoleTest()
    {
      Dispose(false);
    }
  }
}
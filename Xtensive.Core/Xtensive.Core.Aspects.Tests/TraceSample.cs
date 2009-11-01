// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2007.10.11

using System;
using Xtensive.Core.Aspects;
using Xtensive.Core.Diagnostics;

namespace Xtensive.Core.Aspects.Tests
{
  public class TraceSample
  {
    [Trace]
    public static void StaticVoid()
    {
      Log.Info("Inside LogTestSample.StaticVoid");
      StaticVoid2();
    }

    [Trace]
    public static void StaticVoid2()
    {
      Log.Info("Inside LogTestSample.StaticVoid2");
    }

    [Trace("WriteMessage", TraceOptions.All, 
      EventType = LogEventTypes.Warning, LogType = typeof(Xtensive.Core.Log))]
    public bool WriteMessage(string message)
    {
      Log.Info("Inside LogTestSample.WriteMessage(\"{0}\")", message);
      ArgumentValidator.EnsureArgumentNotNullOrEmpty(message, "message");
      return (message.Length%2) == 0;
    }

    [Trace(TraceOptions.All)]
    public void SimpleMethod(string str1, string str2)
    {
      Log.Info("Inside LogTestSample.SimpleMethod");
    }

    [Trace(TraceOptions.All)]
    public void ComplexMethod(params object[] args)
    {
      Log.Info("Inside LogTestSample.ComplexMethod");
      if (args!=null && args.Length==1 && args[0] is Exception)
        throw (Exception)args[0];
    }

    [Trace(LogType = typeof(NullLog))]
    public void DoNothing()
    {
    }
  }
}
// Copyright (C) 2013 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kulakov
// Created:    2013.10.14

using System;
using System.Collections.Generic;
using NUnit.Framework;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Logging;
using Xtensive.Orm.Logging.Internals;

namespace Xtensive.Orm.Tests.Core.Logging
{
  [TestFixture]
  public class LogManagerTests
  {
    [Test]
    public void LogManagerInitializationFromConfigLogs()
    {
      LogManager.Reset();
      LogManager.Initialize();
    }

    [Test]
    public void LogManagerInitializatioLogsFromCode()
    {
      var configuration = new LoggingConfiguration();
      var logs = new List<LogConfiguration>();
      logs.Add(new LogConfiguration("FileLog", "log.txt"));
      logs.Add(new LogConfiguration("ConsoleLog", "Console"));
      logs.Add(new LogConfiguration("DebugOnlyConsoleLog, AnotherFileLog", "None"));
      configuration.Logs = logs;
      LogManager.Reset();
      LogManager.Initialize(configuration);
    }

    [Test]
    public void LogManagerInitializationByProvider()
    {
      var provider = new InternalLogProvider();
      LogManager.Reset();
      LogManager.Initialize(provider);
    }

    [Test]
    [ExpectedException(typeof(InvalidOperationException))]
    public void TryGetLogBeforeInitialization()
    {
      LogManager.Reset();
      LogManager.GetLog("FileLog");
      LogManager.Initialize();
    }

    [Test]
    [ExpectedException(typeof(InvalidOperationException))]
    public void LogManagerAutoInitializationBeforeInitializationTest()
    {
      LogManager.Reset();
      LogManager.AutoInitialize();
      LogManager.Initialize();
    }

    [Test]
    public void LogManagerAutoInitializationAfterInitializationTest()
    {
      LogManager.Reset();
      LogManager.Initialize();
      LogManager.AutoInitialize();
    }
  }
}
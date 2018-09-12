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

namespace Xtensive.Orm.Tests.Core.Logging
{
  [TestFixture]
  public class LogManagerTests : HasConfigurationAccessTest
  {
    [Test]
    public void LogManagerInitializationFromConfigLogs()
    {
      LogManager manager = new LogManager();
      manager.Initialize(Configuration);
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
      LogManager manager = new LogManager(); 
      manager.Initialize(configuration);
    }

    [Test]
    public void LogManagerInitializationByProvider()
    {
      var provider = new InternalLogProvider();
      LogManager manager = new LogManager();
      manager.Initialize(provider);
    }

    [Test]
    public void TryGetLogBeforeInitialization()
    {
      Assert.Throws<InvalidOperationException>(() => {
        LogManager manager = new LogManager();
        manager.GetLog("FileLog");
        manager.Initialize(Configuration);
      });
    }

    [Test]
    public void LogManagerAutoInitializationBeforeInitializationTest()
    {
      Assert.Throws<InvalidOperationException>(() => {
        LogManager manager = new LogManager();
        manager.AutoInitialize();
        manager.Initialize(Configuration);
      });
    }

    [Test]
    public void LogManagerAutoInitializationAfterInitializationTest()
    {
      LogManager manager = new LogManager();
      manager.Initialize(Configuration);
      manager.AutoInitialize();
    }
  }
}
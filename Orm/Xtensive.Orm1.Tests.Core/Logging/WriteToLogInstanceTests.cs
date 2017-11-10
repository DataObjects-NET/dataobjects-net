// Copyright (C) 2013 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kulakov
// Created:    2013.10.14

using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using NUnit.Framework;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Logging;

namespace Xtensive.Orm.Tests.Core.Logging
{
  [TestFixture]
  public class WriteToLogInstanceTests
  {
    private LoggingConfiguration configuration;

#if NETCOREAPP
    [OneTimeSetUp]
#else
    [TestFixtureSetUp]
#endif
    public void Setup()
    {
      configuration = LoggingConfiguration.Load("LoggingConfiguration");
    }

    [Test]
    public void WriteToConsole()
    {
      var manager = new LogManager();
      manager.Initialize(configuration);
      var logger = manager.GetLog("LogName");

      logger.Debug("Test message", null);
      logger.Debug("Test message with parameter {0}", new object[] { 1 });
      logger.Debug("Test message with parameter {0}", new object[] { 1 }, new Exception("exception"));
      logger.Debug("Test message", new object[] { 1 });
      logger.Debug("Test message {0}", null);
      logger.Debug(null, null, new Exception("exception"));
      logger.Debug(null, new object[] { 1 });

      logger.Info("Test message", null);
      logger.Info("Test message with parameter {0}", new object[] { 1 });
      logger.Info("Test message with parameter {0}", new object[] { 1 }, new Exception("exception"));
      logger.Info("Test message", new object[] { 1 });
      logger.Info("Test message {0}", null);
      logger.Info(null, null, new Exception("exception"));
      logger.Info(null, new object[] { 1 });

      logger.Warning("Test message", null);
      logger.Warning("Test message with parameter {0}", new object[] { 1 });
      logger.Warning("Test message with parameter {0}", new object[] { 1 }, new Exception("exception"));
      logger.Warning("Test message", new object[] { 1 });
      logger.Warning("Test message {0}", null);
      logger.Warning(null, null, new Exception("exception"));
      logger.Warning(null, new object[] { 1 });

      logger.Error("Test message", null);
      logger.Error("Test message with parameter {0}", new object[] { 1 });
      logger.Error("Test message with parameter {0}", new object[] { 1 }, new Exception("exception"));
      logger.Error("Test message", new object[] { 1 });
      logger.Error("Test message {0}", null);
      logger.Error(null, null, new Exception("exception"));
      logger.Error(null, new object[] { 1 });

      logger.FatalError("Test message", null);
      logger.FatalError("Test message with parameter {0}", new object[] { 1 });
      logger.FatalError("Test message with parameter {0}", new object[] { 1 }, new Exception("exception"));
      logger.FatalError("Test message", new object[] { 1 });
      logger.FatalError("Test message {0}", null);
      logger.FatalError(null, null, new Exception("exception"));
      logger.FatalError(null, new object[] { 1 });
    }

    [Test]
    public void WriteToDebugOnlyConsole()
    {
      if (!Debugger.IsAttached)
        throw new IgnoreException("This test requeres to attach a debugger.");
      var manager = new LogManager();
      manager.Initialize(configuration);
      var logger = manager.GetLog("SomeLogName");

      logger.Debug("Test message", null);
      logger.Debug("Test message with parameter {0}", new object[] { 1 });
      logger.Debug("Test message with parameter {0}", new object[] { 1 }, new Exception("exception"));
      logger.Debug("Test message", new object[] { 1 });
      logger.Debug("Test message {0}", null);
      logger.Debug(null, null, new Exception("exception"));
      logger.Debug(null, new object[] { 1 });

      logger.Info("Test message", null);
      logger.Info("Test message with parameter {0}", new object[] { 1 });
      logger.Info("Test message with parameter {0}", new object[] { 1 }, new Exception("exception"));
      logger.Info("Test message", new object[] { 1 });
      logger.Info("Test message {0}", null);
      logger.Info(null, null, new Exception("exception"));
      logger.Info(null, new object[] { 1 });

      logger.Warning("Test message", null);
      logger.Warning("Test message with parameter {0}", new object[] { 1 });
      logger.Warning("Test message with parameter {0}", new object[] { 1 }, new Exception("exception"));
      logger.Warning("Test message", new object[] { 1 });
      logger.Warning("Test message {0}", null);
      logger.Warning(null, null, new Exception("exception"));
      logger.Warning(null, new object[] { 1 });

      logger.Error("Test message", null);
      logger.Error("Test message with parameter {0}", new object[] { 1 });
      logger.Error("Test message with parameter {0}", new object[] { 1 }, new Exception("exception"));
      logger.Error("Test message", new object[] { 1 });
      logger.Error("Test message {0}", null);
      logger.Error(null, null, new Exception("exception"));
      logger.Error(null, new object[] { 1 });

      logger.FatalError("Test message", null);
      logger.FatalError("Test message with parameter {0}", new object[] { 1 });
      logger.FatalError("Test message with parameter {0}", new object[] { 1 }, new Exception("exception"));
      logger.FatalError("Test message", new object[] { 1 });
      logger.FatalError("Test message {0}", null);
      logger.FatalError(null, null, new Exception("exception"));
      logger.FatalError(null, new object[] { 1 });
    }

    [Test]
    public void WriteToFileAbsolutePath()
    {
      string filePath = Path.GetFullPath("log.txt");
      var localConfiguration = new LoggingConfiguration();
      localConfiguration.Logs.Add(new LogConfiguration("FileLog", filePath));

      if (File.Exists(filePath))
        File.Delete(filePath);
      var manager = new LogManager();
      manager.Initialize(localConfiguration);
      var logger = manager.GetLog("FileLog");

      logger.Debug("Test message", null);
      logger.Debug("Test message with parameter {0}", new object[] { 1 });
      logger.Debug("Test message with parameter {0}", new object[] { 1 }, new Exception("exception"));
      logger.Debug("Test message", new object[] { 1 });
      logger.Debug("Test message {0}", null);
      logger.Debug(null, null, new Exception("exception"));
      logger.Debug(null, new object[] { 1 });

      logger.Info("Test message", null);
      logger.Info("Test message with parameter {0}", new object[] { 1 });
      logger.Info("Test message with parameter {0}", new object[] { 1 }, new Exception("exception"));
      logger.Info("Test message", new object[] { 1 });
      logger.Info("Test message {0}", null);
      logger.Info(null, null, new Exception("exception"));
      logger.Info(null, new object[] { 1 });

      logger.Warning("Test message", null);
      logger.Warning("Test message with parameter {0}", new object[] { 1 });
      logger.Warning("Test message with parameter {0}", new object[] { 1 }, new Exception("exception"));
      logger.Warning("Test message", new object[] { 1 });
      logger.Warning("Test message {0}", null);
      logger.Warning(null, null, new Exception("exception"));
      logger.Warning(null, new object[] { 1 });

      logger.Error("Test message", null);
      logger.Error("Test message with parameter {0}", new object[] { 1 });
      logger.Error("Test message with parameter {0}", new object[] { 1 }, new Exception("exception"));
      logger.Error("Test message", new object[] { 1 });
      logger.Error("Test message {0}", null);
      logger.Error(null, null, new Exception("exception"));
      logger.Error(null, new object[] { 1 });

      logger.FatalError("Test message", null);
      logger.FatalError("Test message with parameter {0}", new object[] { 1 });
      logger.FatalError("Test message with parameter {0}", new object[] { 1 }, new Exception("exception"));
      logger.FatalError("Test message", new object[] { 1 });
      logger.FatalError("Test message {0}", null);
      logger.FatalError(null, null, new Exception("exception"));
      logger.FatalError(null, new object[] { 1 });

      Assert.IsTrue(File.Exists(filePath));
      Assert.AreEqual(File.ReadAllLines(filePath).Count(), 35);
    }

    [Test]
    public void WriteToFileRelativePath()
    {
      string filePath = "log.txt";
      if (File.Exists(filePath))
        File.Delete(filePath);
      var manager = new LogManager();
      manager.Initialize(configuration);
      var logger = manager.GetLog("FileLog");

      logger.Debug("Test message", null);
      logger.Debug("Test message with parameter {0}", new object[] { 1 });
      logger.Debug("Test message with parameter {0}", new object[] { 1 }, new Exception("exception"));
      logger.Debug("Test message", new object[] { 1 });
      logger.Debug("Test message {0}", null);
      logger.Debug(null, null, new Exception("exception"));
      logger.Debug(null, new object[] { 1 });

      logger.Info("Test message", null);
      logger.Info("Test message with parameter {0}", new object[] { 1 });
      logger.Info("Test message with parameter {0}", new object[] { 1 }, new Exception("exception"));
      logger.Info("Test message", new object[] { 1 });
      logger.Info("Test message {0}", null);
      logger.Info(null, null, new Exception("exception"));
      logger.Info(null, new object[] { 1 });

      logger.Warning("Test message", null);
      logger.Warning("Test message with parameter {0}", new object[] { 1 });
      logger.Warning("Test message with parameter {0}", new object[] { 1 }, new Exception("exception"));
      logger.Warning("Test message", new object[] { 1 });
      logger.Warning("Test message {0}", null);
      logger.Warning(null, null, new Exception("exception"));
      logger.Warning(null, new object[] { 1 });

      logger.Error("Test message", null);
      logger.Error("Test message with parameter {0}", new object[] { 1 });
      logger.Error("Test message with parameter {0}", new object[] { 1 }, new Exception("exception"));
      logger.Error("Test message", new object[] { 1 });
      logger.Error("Test message {0}", null);
      logger.Error(null, null, new Exception("exception"));
      logger.Error(null, new object[] { 1 });

      logger.FatalError("Test message", null);
      logger.FatalError("Test message with parameter {0}", new object[] { 1 });
      logger.FatalError("Test message with parameter {0}", new object[] { 1 }, new Exception("exception"));
      logger.FatalError("Test message", new object[] { 1 });
      logger.FatalError("Test message {0}", null);
      logger.FatalError(null, null, new Exception("exception"));
      logger.FatalError(null, new object[] { 1 });

      Assert.IsTrue(File.Exists(filePath));
      Assert.AreEqual(File.ReadAllLines(filePath).Count(), 35);
    }
  }
}
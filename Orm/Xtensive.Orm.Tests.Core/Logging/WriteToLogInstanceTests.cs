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

    [TestFixtureSetUp]
    public void Setup()
    {
      configuration = LoggingConfiguration.Load("LoggingConfiguration");
    }

    [Test]
    public void WriteToConsole()
    {
      LogManager.Reset();
      LogManager.Initialize(configuration);
      var logger = LogManager.GetLog("LogName");

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

      logger.Warn("Test message", null);
      logger.Warn("Test message with parameter {0}", new object[] { 1 });
      logger.Warn("Test message with parameter {0}", new object[] { 1 }, new Exception("exception"));
      logger.Warn("Test message", new object[] { 1 });
      logger.Warn("Test message {0}", null);
      logger.Warn(null, null, new Exception("exception"));
      logger.Warn(null, new object[] { 1 });

      logger.Error("Test message", null);
      logger.Error("Test message with parameter {0}", new object[] { 1 });
      logger.Error("Test message with parameter {0}", new object[] { 1 }, new Exception("exception"));
      logger.Error("Test message", new object[] { 1 });
      logger.Error("Test message {0}", null);
      logger.Error(null, null, new Exception("exception"));
      logger.Error(null, new object[] { 1 });
      logger.Error(new Exception("exception"));

      logger.Fatal("Test message", null);
      logger.Fatal("Test message with parameter {0}", new object[] { 1 });
      logger.Fatal("Test message with parameter {0}", new object[] { 1 }, new Exception("exception"));
      logger.Fatal("Test message", new object[] { 1 });
      logger.Fatal("Test message {0}", null);
      logger.Fatal(null, null, new Exception("exception"));
      logger.Fatal(null, new object[] { 1 });
    }

    [Test]
    public void WriteToDebugOnlyConsole()
    {
      if (!Debugger.IsAttached)
        throw new IgnoreException("This test requeres to attach a debugger.");
      LogManager.Reset();
      LogManager.Initialize(configuration);
      var logger = LogManager.GetLog("SomeLogName");

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

      logger.Warn("Test message", null);
      logger.Warn("Test message with parameter {0}", new object[] { 1 });
      logger.Warn("Test message with parameter {0}", new object[] { 1 }, new Exception("exception"));
      logger.Warn("Test message", new object[] { 1 });
      logger.Warn("Test message {0}", null);
      logger.Warn(null, null, new Exception("exception"));
      logger.Warn(null, new object[] { 1 });

      logger.Error("Test message", null);
      logger.Error("Test message with parameter {0}", new object[] { 1 });
      logger.Error("Test message with parameter {0}", new object[] { 1 }, new Exception("exception"));
      logger.Error("Test message", new object[] { 1 });
      logger.Error("Test message {0}", null);
      logger.Error(null, null, new Exception("exception"));
      logger.Error(null, new object[] { 1 });
      logger.Error(new Exception("exception"));

      logger.Fatal("Test message", null);
      logger.Fatal("Test message with parameter {0}", new object[] { 1 });
      logger.Fatal("Test message with parameter {0}", new object[] { 1 }, new Exception("exception"));
      logger.Fatal("Test message", new object[] { 1 });
      logger.Fatal("Test message {0}", null);
      logger.Fatal(null, null, new Exception("exception"));
      logger.Fatal(null, new object[] { 1 });
    }

    [Test]
    public void WriteToFileAbsolutePath()
    {
      string filePath = @"D:\log.txt";
      if (File.Exists(filePath))
        File.Delete(filePath);
      LogManager.Reset();
      LogManager.Initialize(configuration);
      var logger = LogManager.GetLog("FirstLogName");

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

      logger.Warn("Test message", null);
      logger.Warn("Test message with parameter {0}", new object[] { 1 });
      logger.Warn("Test message with parameter {0}", new object[] { 1 }, new Exception("exception"));
      logger.Warn("Test message", new object[] { 1 });
      logger.Warn("Test message {0}", null);
      logger.Warn(null, null, new Exception("exception"));
      logger.Warn(null, new object[] { 1 });

      logger.Error("Test message", null);
      logger.Error("Test message with parameter {0}", new object[] { 1 });
      logger.Error("Test message with parameter {0}", new object[] { 1 }, new Exception("exception"));
      logger.Error("Test message", new object[] { 1 });
      logger.Error("Test message {0}", null);
      logger.Error(null, null, new Exception("exception"));
      logger.Error(null, new object[] { 1 });
      logger.Error(new Exception("exception"));

      logger.Fatal("Test message", null);
      logger.Fatal("Test message with parameter {0}", new object[] { 1 });
      logger.Fatal("Test message with parameter {0}", new object[] { 1 }, new Exception("exception"));
      logger.Fatal("Test message", new object[] { 1 });
      logger.Fatal("Test message {0}", null);
      logger.Fatal(null, null, new Exception("exception"));
      logger.Fatal(null, new object[] { 1 });

      Assert.IsTrue(File.Exists(filePath));
      Assert.AreEqual(File.ReadAllLines(filePath).Count(), 36);
    }

    [Test]
    public void WriteToFileRelativePath()
    {
      string filePath = "log.txt";
      if (File.Exists(filePath))
        File.Delete(filePath);
      LogManager.Reset();
      LogManager.Initialize(configuration);
      var logger = LogManager.GetLog("FileLog");

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

      logger.Warn("Test message", null);
      logger.Warn("Test message with parameter {0}", new object[] { 1 });
      logger.Warn("Test message with parameter {0}", new object[] { 1 }, new Exception("exception"));
      logger.Warn("Test message", new object[] { 1 });
      logger.Warn("Test message {0}", null);
      logger.Warn(null, null, new Exception("exception"));
      logger.Warn(null, new object[] { 1 });

      logger.Error("Test message", null);
      logger.Error("Test message with parameter {0}", new object[] { 1 });
      logger.Error("Test message with parameter {0}", new object[] { 1 }, new Exception("exception"));
      logger.Error("Test message", new object[] { 1 });
      logger.Error("Test message {0}", null);
      logger.Error(null, null, new Exception("exception"));
      logger.Error(null, new object[] { 1 });
      logger.Error(new Exception("exception"));

      logger.Fatal("Test message", null);
      logger.Fatal("Test message with parameter {0}", new object[] { 1 });
      logger.Fatal("Test message with parameter {0}", new object[] { 1 }, new Exception("exception"));
      logger.Fatal("Test message", new object[] { 1 });
      logger.Fatal("Test message {0}", null);
      logger.Fatal(null, null, new Exception("exception"));
      logger.Fatal(null, new object[] { 1 });

      Assert.IsTrue(File.Exists(filePath));
      Assert.AreEqual(File.ReadAllLines(filePath).Count(), 36);
    }
  }
}
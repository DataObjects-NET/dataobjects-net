// Copyright (C) 2013-2021 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Alexey Kulakov
// Created:    2013.10.14

using System;
using System.IO;
using System.Linq;
using NUnit.Framework;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Logging;

namespace Xtensive.Orm.Tests.Core.Logging
{
  [TestFixture]
  public class WriteToLogTests : HasConfigurationAccessTest
  {
    private const string filePath = "Log.txt";

    [Test]
    public void OrmLogTest()
    {
      if (File.Exists(filePath)) {
        File.Delete(filePath);
      }

      OrmLog.Debug("Test message", null);
      OrmLog.Debug("Test message with parameter {0}", new object[] { 1 });
      _ = OrmLog.Debug(new Exception("Some exception"), "Test message with parameter {0}", new object[] { 1 });
      OrmLog.Debug("Test message", new object[] { 1 });
      OrmLog.Debug("Test message {0}", null);
      _ = OrmLog.Debug(new Exception("Some exeption"));
      OrmLog.Debug(null, new object[] { 1 });

      OrmLog.Info("Test message", null);
      OrmLog.Info("Test message with parameter {0}", new object[] { 1 });
      _ = OrmLog.Info(new Exception("Some exception"), "Test message with parameter {0}", new object[] { 1 });
      OrmLog.Info("Test message", new object[] { 1 });
      OrmLog.Info("Test message {0}", null);
      _ = OrmLog.Info(new Exception("Some exeption"));
      OrmLog.Info(null, new object[] { 1 });

      OrmLog.Warning("Test message", null);
      OrmLog.Warning("Test message with parameter {0}", new object[] { 1 });
      _ = OrmLog.Warning(new Exception("Some exception"), "Test message with parameter {0}", new object[] { 1 });
      OrmLog.Warning("Test message", new object[] { 1 });
      OrmLog.Warning("Test message {0}", null);
      _ = OrmLog.Warning(new Exception("Some exeption"));
      OrmLog.Warning(null, new object[] { 1 });

      OrmLog.Error("Test message", null);
      OrmLog.Error("Test message with parameter {0}", new object[] { 1 });
      _ = OrmLog.Error(new Exception("Some exception"), "Test message with parameter {0}", new object[] { 1 });
      OrmLog.Error("Test message", new object[] { 1 });
      OrmLog.Error("Test message {0}", null);
      _ = OrmLog.Error(new Exception("Some exeption"));
      OrmLog.Error(null, new object[] { 1 });

      OrmLog.FatalError("Test message", null);
      OrmLog.FatalError("Test message with parameter {0}", new object[] { 1 });
      _ = OrmLog.FatalError(new Exception("Some exception"), "Test message with parameter {0}", new object[] { 1 });
      OrmLog.FatalError("Test message", new object[] { 1 });
      OrmLog.FatalError("Test message {0}", null);
      _ = OrmLog.FatalError(new Exception("Some exeption"));
      OrmLog.FatalError(null, new object[] { 1 });

      Assert.IsTrue(File.Exists(filePath));
      Assert.AreEqual(File.ReadAllLines(filePath).Count(), 35);
    }

    [Test]
    public void BuildLogTest()
    {
      if (File.Exists(filePath))
        File.Delete(filePath);
      BuildLog.Debug("Test message", null);
      BuildLog.Debug("Test message with parameter {0}", new object[] { 1 });
      _ = BuildLog.Debug(new Exception("Some exception"), "Test message with parameter {0}", new object[] { 1 });
      BuildLog.Debug("Test message", new object[] { 1 });
      BuildLog.Debug("Test message {0}", null);
      _ = BuildLog.Debug(new Exception("Some exeption"));
      BuildLog.Debug(null, new object[] { 1 });

      BuildLog.Info("Test message", null);
      BuildLog.Info("Test message with parameter {0}", new object[] { 1 });
      _ = BuildLog.Info(new Exception("Some exception"), "Test message with parameter {0}", new object[] { 1 });
      BuildLog.Info("Test message", new object[] { 1 });
      BuildLog.Info("Test message {0}", null);
      _ = BuildLog.Info(new Exception("Some exeption"));
      BuildLog.Info(null, new object[] { 1 });

      BuildLog.Warning("Test message", null);
      BuildLog.Warning("Test message with parameter {0}", new object[] { 1 });
      _ = BuildLog.Warning(new Exception("Some exception"), "Test message with parameter {0}", new object[] { 1 });
      BuildLog.Warning("Test message", new object[] { 1 });
      BuildLog.Warning("Test message {0}", null);
      _ = BuildLog.Warning(new Exception("Some exeption"));
      BuildLog.Warning(null, new object[] { 1 });

      BuildLog.Error("Test message", null);
      BuildLog.Error("Test message with parameter {0}", new object[] { 1 });
      _ = BuildLog.Error(new Exception("Some exception"), "Test message with parameter {0}", new object[] { 1 });
      BuildLog.Error("Test message", new object[] { 1 });
      BuildLog.Error("Test message {0}", null);
      _ = BuildLog.Error(new Exception("Some exeption"));
      BuildLog.Error(null, new object[] { 1 });

      BuildLog.FatalError("Test message", null);
      BuildLog.FatalError("Test message with parameter {0}", new object[] { 1 });
      _ = BuildLog.FatalError(new Exception("Some exception"), "Test message with parameter {0}", new object[] { 1 });
      BuildLog.FatalError("Test message", new object[] { 1 });
      BuildLog.FatalError("Test message {0}", null);
      _ = BuildLog.FatalError(new Exception("Some exeption"));
      BuildLog.FatalError(null, new object[] { 1 });

      Assert.IsTrue(File.Exists(filePath));
      Assert.AreEqual(File.ReadAllLines(filePath).Count(), 35);
    }

    [Test]
    public void UpgradeLogTest()
    {
      if (File.Exists(filePath))
        File.Delete(filePath);
      UpgradeLog.Debug("Test message", null);
      UpgradeLog.Debug("Test message with parameter {0}", new object[] { 1 });
      UpgradeLog.Debug(new Exception("Some exception"), "Test message with parameter {0}", new object[] { 1 });
      UpgradeLog.Debug("Test message", new object[] { 1 });
      UpgradeLog.Debug("Test message {0}", null);
      UpgradeLog.Debug(new Exception("Some exeption"));
      UpgradeLog.Debug(null, new object[] { 1 });

      UpgradeLog.Info("Test message", null);
      UpgradeLog.Info("Test message with parameter {0}", new object[] { 1 });
      UpgradeLog.Info(new Exception("Some exception"), "Test message with parameter {0}", new object[] { 1 });
      UpgradeLog.Info("Test message", new object[] { 1 });
      UpgradeLog.Info("Test message {0}", null);
      UpgradeLog.Info(new Exception("Some exeption"));
      UpgradeLog.Info(null, new object[] { 1 });

      UpgradeLog.Warning("Test message", null);
      UpgradeLog.Warning("Test message with parameter {0}", new object[] { 1 });
      UpgradeLog.Warning(new Exception("Some exception"), "Test message with parameter {0}", new object[] { 1 });
      UpgradeLog.Warning("Test message", new object[] { 1 });
      UpgradeLog.Warning("Test message {0}", null);
      UpgradeLog.Warning(new Exception("Some exeption"));
      UpgradeLog.Warning(null, new object[] { 1 });

      UpgradeLog.Error("Test message", null);
      UpgradeLog.Error("Test message with parameter {0}", new object[] { 1 });
      UpgradeLog.Error(new Exception("Some exception"), "Test message with parameter {0}", new object[] { 1 });
      UpgradeLog.Error("Test message", new object[] { 1 });
      UpgradeLog.Error("Test message {0}", null);
      UpgradeLog.Error(new Exception("Some exeption"));
      UpgradeLog.Error(null, new object[] { 1 });

      UpgradeLog.FatalError("Test message", null);
      UpgradeLog.FatalError("Test message with parameter {0}", new object[] { 1 });
      UpgradeLog.FatalError(new Exception("Some exception"), "Test message with parameter {0}", new object[] { 1 });
      UpgradeLog.FatalError("Test message", new object[] { 1 });
      UpgradeLog.FatalError("Test message {0}", null);
      UpgradeLog.FatalError(new Exception("Some exeption"));
      UpgradeLog.FatalError(null, new object[] { 1 });

      Assert.IsTrue(File.Exists(filePath));
      Assert.AreEqual(File.ReadAllLines(filePath).Count(), 35);
    }

    [Test]
    public void SqlLogTest()
    {
      if (File.Exists(filePath))
        File.Delete(filePath);
      SqlLog.Debug("Test message", null);
      SqlLog.Debug("Test message with parameter {0}", new object[] { 1 });
      _ = SqlLog.Debug(new Exception("Some exception"), "Test message with parameter {0}", new object[] { 1 });
      SqlLog.Debug("Test message", new object[] { 1 });
      SqlLog.Debug("Test message {0}", null);
      _ = SqlLog.Debug(new Exception("Some exeption"));
      SqlLog.Debug(null, new object[] { 1 });

      SqlLog.Info("Test message", null);
      SqlLog.Info("Test message with parameter {0}", new object[] { 1 });
      _ = SqlLog.Info(new Exception("Some exception"), "Test message with parameter {0}", new object[] { 1 });
      SqlLog.Info("Test message", new object[] { 1 });
      SqlLog.Info("Test message {0}", null);
      _ = SqlLog.Info(new Exception("Some exeption"));
      SqlLog.Info(null, new object[] { 1 });

      SqlLog.Warning("Test message", null);
      SqlLog.Warning("Test message with parameter {0}", new object[] { 1 });
      _ = SqlLog.Warning(new Exception("Some exception"), "Test message with parameter {0}", new object[] { 1 });
      SqlLog.Warning("Test message", new object[] { 1 });
      SqlLog.Warning("Test message {0}", null);
      _ = SqlLog.Warning(new Exception("Some exeption"));
      SqlLog.Warning(null, new object[] { 1 });

      SqlLog.Error("Test message", null);
      SqlLog.Error("Test message with parameter {0}", new object[] { 1 });
      _ = SqlLog.Error(new Exception("Some exception"), "Test message with parameter {0}", new object[] { 1 });
      SqlLog.Error("Test message", new object[] { 1 });
      SqlLog.Error("Test message {0}", null);
      _ = SqlLog.Error(new Exception("Some exeption"));
      SqlLog.Error(null, new object[] { 1 });

      SqlLog.FatalError("Test message", null);
      SqlLog.FatalError("Test message with parameter {0}", new object[] { 1 });
      _ = SqlLog.FatalError(new Exception("Some exception"), "Test message with parameter {0}", new object[] { 1 });
      SqlLog.FatalError("Test message", new object[] { 1 });
      SqlLog.FatalError("Test message {0}", null);
      _ = SqlLog.FatalError(new Exception("Some exeption"));
      SqlLog.FatalError(null, new object[] { 1 });

      Assert.IsTrue(File.Exists(filePath));
      Assert.AreEqual(File.ReadAllLines(filePath).Count(), 35);
    }
  }
}
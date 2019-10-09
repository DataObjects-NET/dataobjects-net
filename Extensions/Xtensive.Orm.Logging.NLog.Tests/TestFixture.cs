// Copyright (C) 2003-2013 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2013.12.13

using NUnit.Framework;

namespace Xtensive.Orm.Logging.NLog.Tests
{
  [TestFixture]
  public class TestFixture
  {
    [Test]
    public void LogManagerTest()
    {
      var logManager = LogManager.Default;
      logManager.Initialize(new LogProvider());
      var logger = LogManager.Default.GetLog("Xtensive.Orm");
      Assert.IsInstanceOf<Log>(logger);
    }
  }
}
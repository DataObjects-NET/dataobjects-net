// Copyright (C) 2014 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kulakov
// Created:    2014.02.20

using NUnit.Framework;


namespace Xtensive.Orm.Logging.log4net.Tests
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

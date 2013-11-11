// Copyright (C) 2013 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kulakov
// Created:    2013.10.14

using NUnit.Framework;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Logging;

namespace Xtensive.Orm.Tests.Core.Logging
{
  [TestFixture]
  public class InternalLogProviderTests
  {
    [Test]
    public void DefaultInitializationTest()
    {
      var logProvider = new InternalLogProvider();
      var logger = logProvider.GetLog("someLogNameWhichDoesNotContainsInLogList");
      Assert.That(logger, Is.InstanceOf<NullLog>());
    }

    [Test]
    public void ParameterizedInitializationTest()
    {
      var configuration = LoggingConfiguration.Load("LoggingConfiguration");
      var logProvider = new InternalLogProvider(configuration.Logs);
      var logger = logProvider.GetLog("Trash");

      Assert.That(logger.Name, Is.EqualTo("Trash"));
      Assert.That(logger, Is.InstanceOf<InternalLog>());

      logger = logProvider.GetLog("NullLog");
      Assert.That(logger.Name, Is.EqualTo(string.Empty));
      Assert.That(logger, Is.InstanceOf<NullLog>());

      logger = logProvider.GetLog("FileLog");
      Assert.That(logger.Name, Is.EqualTo("FileLog"));
      Assert.That(logger, Is.InstanceOf<InternalLog>());

      logger = logProvider.GetLog("AnotherLogName");
      Assert.That(logger.Name, Is.EqualTo("AnotherLogName"));
      Assert.That(logger, Is.InstanceOf<InternalLog>());

      logger = logProvider.GetLog("LogName");
      Assert.That(logger.Name, Is.EqualTo("LogName"));
      Assert.That(logger, Is.InstanceOf<InternalLog>());

      logger = logProvider.GetLog("SecondLogName");
      Assert.That(logger.Name, Is.EqualTo("SecondLogName"));
      Assert.That(logger, Is.InstanceOf<InternalLog>());

      logger = logProvider.GetLog("FirstLogName");
      Assert.That(logger.Name, Is.EqualTo("FirstLogName"));
      Assert.That(logger, Is.InstanceOf<InternalLog>());

      logger = logProvider.GetLog("SomeLogName");
      Assert.That(logger.Name, Is.EqualTo("SomeLogName"));
      Assert.That(logger, Is.InstanceOf<InternalLog>());

      logger = logProvider.GetLog("LogNameNot in list");
      Assert.That(logger.Name, Is.EqualTo("<default>"));
      Assert.That(logger, Is.InstanceOf<InternalLog>());
    }
  }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Logging;
using Xtensive.Orm.Logging.Internals;

namespace Xtensive.Orm.Tests.Core.Logging
{
  [TestFixture]
  public class InternalLogProviderTests
  {
    [Test]
    public void DefaultInitializationTest()
    {
      var logProvider = new InternalLogProvider();
      var logger = logProvider.GetLog("dflgjljdfhldkfh");
      Assert.That(logger, Is.InstanceOf<NullLog>());
    }

    [Test]
    public void ParameterizedInitializationTest()
    {
      var configuration = LoggingConfiguration.Load("LoggingConfiguration");
      var logProvider = new InternalLogProvider(configuration.Logs);
      var logger = logProvider.GetLog("Trash");

      Assert.That(logger.Name, Is.EqualTo("Trash"));
      Assert.That(logger, Is.InstanceOf<Log>());

      logger = logProvider.GetLog("NullLog");
      Assert.That(logger.Name, Is.EqualTo(string.Empty));
      Assert.That(logger, Is.InstanceOf<NullLog>());

      logger = logProvider.GetLog("FileLog");
      Assert.That(logger.Name, Is.EqualTo("FileLog"));
      Assert.That(logger, Is.InstanceOf<Log>());

      logger = logProvider.GetLog("AnotherLogName");
      Assert.That(logger.Name, Is.EqualTo("AnotherLogName"));
      Assert.That(logger, Is.InstanceOf<Log>());

      logger = logProvider.GetLog("LogName");
      Assert.That(logger.Name, Is.EqualTo("LogName"));
      Assert.That(logger, Is.InstanceOf<Log>());

      logger = logProvider.GetLog("SecondLogName");
      Assert.That(logger.Name, Is.EqualTo("SecondLogName"));
      Assert.That(logger, Is.InstanceOf<Log>());

      logger = logProvider.GetLog("FirstLogName");
      Assert.That(logger.Name, Is.EqualTo("FirstLogName"));
      Assert.That(logger, Is.InstanceOf<Log>());

      logger = logProvider.GetLog("SomeLogName");
      Assert.That(logger.Name, Is.EqualTo("SomeLogName"));
      Assert.That(logger, Is.InstanceOf<Log>());

      logger = logProvider.GetLog("LogNameNot in list");
      Assert.That(logger.Name, Is.EqualTo("DefaultLog"));
      Assert.That(logger, Is.InstanceOf<Log>());
    }
  }
}

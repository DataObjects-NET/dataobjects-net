using System;
using NUnit.Framework;

namespace TestCommon.Tests
{
  [TestFixture]
  public class TestConfigurationTest
  {
     [Test]
     public void Test()
     {
       var storage = TestConfiguration.Instance.Storage;
       Console.WriteLine("storage: {0}", storage);
       var configuration = DomainConfigurationFactory.Create();
       Console.WriteLine("connection: {0}", configuration.ConnectionInfo);
     }
  }
}
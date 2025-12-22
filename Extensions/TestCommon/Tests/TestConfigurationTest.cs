using System;
using NUnit.Framework;
using Xtensive.Orm.Tests;

namespace TestCommon.Tests
{
  [TestFixture]
  public class TestConfigurationTest
  {
     [Test]
     public void Test()
     {
       var storage = TestConfiguration.Instance.Storage;
       Console.WriteLine($"storage: {storage}");
       var configuration = DomainConfigurationFactory.Create();
       Console.WriteLine($"connection: {configuration.ConnectionInfo}");
     }
  }
}
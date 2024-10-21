// Copyright (C) 2011-2024 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Dmitri Maximov
// Created:    2011.06.07

using NUnit.Framework;
using TestCommon;
using Xtensive.Orm.Security.Configuration;
using Xtensive.Orm.Tests;

namespace Xtensive.Orm.Security.Tests
{
  [TestFixture]
  public class ConfigurationTests : HasConfigurationAccessTest
  {
    [Test]
    public void HashingServiceNameTest()
    {
      var section = (ConfigurationSection) Configuration.GetSection("Xtensive.Orm.Security.WithName");
      Assert.That(section, Is.Not.Null);
      Assert.That(section.HashingService, Is.Not.Null);
      Assert.That(section.HashingService.Name, Is.Not.Null);
      Assert.That(section.HashingService.Name, Is.EqualTo("md5"));

      var config = SecurityConfiguration.Load(Configuration, "Xtensive.Orm.Security.WithName");
      Assert.That(config, Is.Not.Null);
      Assert.That(config.HashingServiceName, Is.EqualTo("md5"));
    }

    [Test]
    public void HashingServiceAndAuthenticationServiceNameTest()
    {
      var section = (ConfigurationSection) Configuration.GetSection("Xtensive.Orm.Security.AllDeclared");
      Assert.That(section, Is.Not.Null);
      Assert.That(section.HashingService, Is.Not.Null);
      Assert.That(section.HashingService.Name, Is.Not.Null);
      Assert.That(section.HashingService.Name, Is.EqualTo("sha1"));
      Assert.That(section.AuthenticationService.Name, Is.Not.Null);
      Assert.That(section.AuthenticationService.Name, Is.EqualTo("notdefault"));

      var config = SecurityConfiguration.Load(Configuration, "Xtensive.Orm.Security.AllDeclared");
      Assert.That(config, Is.Not.Null);
      Assert.That(config.HashingServiceName, Is.EqualTo("sha1"));
      Assert.That(config.AuthenticationServiceName, Is.EqualTo("notdefault"));
    }

    [Test]
    public void HashingServiceEmptyTest()
    {
      var section = (ConfigurationSection) Configuration.GetSection("Xtensive.Orm.Security.WithoutName");
      Assert.That(section, Is.Not.Null);
      Assert.That(section.HashingService, Is.Not.Null);
      Assert.That(section.HashingService.Name, Is.Null.Or.Empty);

      var config = SecurityConfiguration.Load(Configuration, "Xtensive.Orm.Security.WithoutName");
      Assert.That(config, Is.Not.Null);
      Assert.That(config.HashingServiceName, Is.EqualTo("plain"));
    }

    [Test]
    public void HashingServiceAbsentTest()
    {
      var section = (ConfigurationSection) Configuration.GetSection("Xtensive.Orm.Security.Empty");
      Assert.That(section, Is.Not.Null);
      Assert.That(section.HashingService, Is.Not.Null);
      Assert.That(section.HashingService.Name, Is.Null.Or.Empty);

      var config = SecurityConfiguration.Load(Configuration, "Xtensive.Orm.Security.Empty");
      Assert.That(config, Is.Not.Null);
      Assert.That(config.HashingServiceName, Is.EqualTo("plain"));
    }

    [Test]
    public void HashingServiceNoConfigTest()
    {
      var section = (ConfigurationSection) Configuration.GetSection("Xtensive.Orm.Security.XXX");
      Assert.That(section, Is.Null);

      var config = SecurityConfiguration.Load(Configuration, "Xtensive.Orm.Security.XXX");
      Assert.That(config, Is.Not.Null);
      Assert.That(config.HashingServiceName, Is.EqualTo("plain"));
    }

    [Test]
    public void HashingServiceDefaultTest()
    {
      var section = (ConfigurationSection) Configuration.GetSection("Xtensive.Orm.Security");
      Assert.That(section, Is.Not.Null);
      Assert.That(section.HashingService, Is.Not.Null);
      Assert.That(section.HashingService.Name, Is.Not.Null.Or.Empty);
      Assert.That(section.HashingService.Name, Is.EqualTo("sha1"));

      var config = SecurityConfiguration.Load(Configuration);
      Assert.That(config, Is.Not.Null);
      Assert.That(config.HashingServiceName, Is.EqualTo("sha1"));
    }
  }
}
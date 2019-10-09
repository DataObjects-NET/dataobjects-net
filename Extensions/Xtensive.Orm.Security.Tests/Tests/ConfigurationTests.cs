// Copyright (C) 2011 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2011.06.07

using NUnit.Framework;
using TestCommon;
using Xtensive.Orm.Security.Configuration;

namespace Xtensive.Orm.Security.Tests
{
  [TestFixture]
  public class ConfigurationTests : HasConfigurationAccessTest
  {
    [Test]
    public void HashingServiceNameTest()
    {
      var section = (Configuration.ConfigurationSection) Configuration.GetSection("Xtensive.Orm.Security.WithName");
      Assert.That(section, Is.Not.Null);
      Assert.That(section.HashingService, Is.Not.Null);
      Assert.That(section.HashingService.Name, Is.Not.Null);
      Assert.That(section.HashingService.Name, Is.EqualTo("md5"));

      var config = SecurityConfiguration.Load(Configuration, "Xtensive.Orm.Security.WithName");
      Assert.That(config, Is.Not.Null);
      Assert.That(config.HashingServiceName, Is.EqualTo("md5"));
    }

    [Test]
    public void HashingServiceEmptyTest()
    {
      var section = (Configuration.ConfigurationSection) Configuration.GetSection("Xtensive.Orm.Security.WithoutName");
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
      var section = (Configuration.ConfigurationSection) Configuration.GetSection("Xtensive.Orm.Security.Empty");
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
      var section = (Configuration.ConfigurationSection) Configuration.GetSection("Xtensive.Orm.Security.XXX");
      Assert.That(section, Is.Null);

      var config = SecurityConfiguration.Load(Configuration, "Xtensive.Orm.Security.XXX");
      Assert.That(config, Is.Not.Null);
      Assert.That(config.HashingServiceName, Is.EqualTo("plain"));
    }

    [Test]
    public void HashingServiceDefaultTest()
    {
      var section = (Configuration.ConfigurationSection) Configuration.GetSection("Xtensive.Orm.Security");
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
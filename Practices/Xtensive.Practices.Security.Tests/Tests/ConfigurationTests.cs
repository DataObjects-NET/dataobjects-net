// Copyright (C) 2011 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2011.06.07

using System;
using System.Configuration;
using NUnit.Framework;
using Xtensive.Practices.Security.Configuration;

namespace Xtensive.Practices.Security.Tests
{
  [TestFixture]
  public class ConfigurationTests
  {
    [Test]
    public void HashingServiceNameTest()
    {
      var section = (Configuration.ConfigurationSection) ConfigurationManager.GetSection("Xtensive.Security.Name");
      Assert.IsNotNull(section);
      Assert.IsNotNull(section.HashingService);
      Assert.IsNotNull(section.HashingService.Name);
      Assert.AreEqual("MD5", section.HashingService.Name);

      var config = SecurityConfiguration.Load("Xtensive.Security.Name");
      Assert.IsNotNull(config);
      Assert.AreEqual("MD5", config.HashingServiceName);
    }

    [Test]
    public void HashingServiceEmptyTest()
    {
      var section = (Configuration.ConfigurationSection) ConfigurationManager.GetSection("Xtensive.Security.Empty");
      Assert.IsNotNull(section);
      Assert.IsNotNull(section.HashingService);
      Assert.IsNullOrEmpty(section.HashingService.Name);

      var config = SecurityConfiguration.Load("Xtensive.Security.Empty");
      Assert.IsNotNull(config);
      Assert.AreEqual("PLAIN", config.HashingServiceName);
    }

    [Test]
    public void HashingServiceDefaultTest()
    {
      var section = (Configuration.ConfigurationSection) ConfigurationManager.GetSection("Xtensive.Security");
      Assert.IsNotNull(section);
      Assert.IsNotNull(section.HashingService);
      Assert.IsNotNullOrEmpty(section.HashingService.Name);
      Assert.AreEqual("SHA1", section.HashingService.Name);

      var config = SecurityConfiguration.Load();
      Assert.IsNotNull(config);
      Assert.AreEqual("SHA1", config.HashingServiceName);
    }
  }
}
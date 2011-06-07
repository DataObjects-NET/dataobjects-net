// Copyright (C) 2011 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2011.06.07

using System;
using System.Configuration;
using NUnit.Framework;

namespace Xtensive.Practices.Security.Tests
{
  [TestFixture]
  public class ConfigurationTests
  {
    [Test]
    public void EncryptionServiceNameTest()
    {
      var config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
      var section = config.GetSection("Xtensive.Security.Name") as Configuration.ConfigurationSection;
      Assert.IsNotNull(section);
      Assert.IsNotNull(section.EncryptionService);
      Assert.IsNullOrEmpty(section.EncryptionService.Type);
      Assert.IsNotNull(section.EncryptionService.Name);
      Assert.AreEqual("Md5EncryptionService", section.EncryptionService.Name);
    }

    [Test]
    public void EncryptionServiceTypeTest()
    {
      var config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
      var section = config.GetSection("Xtensive.Security.Type") as Configuration.ConfigurationSection;
      Assert.IsNotNull(section);
      Assert.IsNotNull(section.EncryptionService);
      Assert.IsNullOrEmpty(section.EncryptionService.Name);
      Assert.IsNotNull(section.EncryptionService.Type);
      Assert.AreEqual("Xtensive.Practices.Security.Encryption.Md5EncryptionService, Xtensive.Practices.Security", section.EncryptionService.Type);
    }

    [Test]
    public void EncryptionServiceEmptyTest()
    {
      var config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
      var section = config.GetSection("Xtensive.Security.Empty") as Configuration.ConfigurationSection;
      Assert.IsNotNull(section);
      Assert.IsNotNull(section.EncryptionService);
      Assert.IsNullOrEmpty(section.EncryptionService.Type);
      Assert.IsNullOrEmpty(section.EncryptionService.Name);
    }

    [Test]
    public void EncryptionServiceBothTest()
    {
      var config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
      var section = config.GetSection("Xtensive.Security.Both") as Configuration.ConfigurationSection;
      Assert.IsNotNull(section);
      Assert.IsNotNull(section.EncryptionService);
      Assert.IsNotNull(section.EncryptionService.Type);
      Assert.IsNotNull(section.EncryptionService.Name);
      Assert.AreEqual("Md5EncryptionService", section.EncryptionService.Name);
      Assert.AreEqual("Xtensive.Practices.Security.Encryption.PlainEncryptionService, Xtensive.Practices.Security", section.EncryptionService.Type);
    }
  }
}
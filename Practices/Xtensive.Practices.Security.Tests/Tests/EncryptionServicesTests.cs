// Copyright (C) 2011 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2011.05.30

using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NUnit.Framework;

namespace Xtensive.Practices.Security.Tests
{
  [TestFixture]
  public class EncryptionServicesTests
  {
    private List<string> values;

    [Test]
    public void PlainEncryptionServiceTest()
    {
      var service = new Encryption.PlainEncryptionService();

      foreach (string value in values)
        Assert.AreEqual(value, service.Encrypt(value));
    }

    [Test]
    public void Md5EncryptionServiceTest()
    {
      var service1 = new Encryption.Md5EncryptionService();
      var service2 = new Encryption.Md5EncryptionService();

      foreach (string value in values)
        Assert.AreEqual(service1.Encrypt(value), service2.Encrypt(value));
    }

    public EncryptionServicesTests()
    {
      values = Assembly.GetExecutingAssembly().GetTypes().Select(t => t.Name).ToList();
    }
  }
}
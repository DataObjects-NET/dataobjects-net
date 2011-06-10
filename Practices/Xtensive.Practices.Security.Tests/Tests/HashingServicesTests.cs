// Copyright (C) 2011 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2011.05.30

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using Xtensive.Practices.Security.Cryptography;

namespace Xtensive.Practices.Security.Tests
{
  [TestFixture]
  public class HashingServicesTests : AutoBuildTest
  {
    private List<string> values;

    [Test]
    public void PlainHashingServiceTest()
    {
      var service = new Cryptography.PlainHashingService();

      foreach (string value in values)
        Assert.AreEqual(value, service.ComputeHash(value));
    }

    [Test]
    public void SHA1HashingServiceTest()
    {
      ExecuteTest(new SHA1HashingService(), new SHA1HashingService());
    }

    [Test]
    public void SHA256HashingServiceTest()
    {
      ExecuteTest(new SHA256HashingService(), new SHA256HashingService());
    }

    [Test]
    public void SHA384HashingServiceTest()
    {
      ExecuteTest(new SHA384HashingService(), new SHA384HashingService());
    }

    [Test]
    public void SHA512HashingServiceTest()
    {
      ExecuteTest(new SHA512HashingService(), new SHA512HashingService());
    }

    private void ExecuteTest(IHashingService service1, IHashingService service2)
    {
      foreach (string value in values) {
        var hash = service1.ComputeHash(value);
        Assert.IsNotEmpty(hash);
        Assert.IsTrue(service2.VerifyHash(value, hash));
        Assert.IsFalse(service2.VerifyHash(value, Convert.ToBase64String(new byte[] {33, 32,23,23,23,23,23,23,23,23,2,32,3,23,23,23})));
      }
    }

    [Test]
    public void InitializationTest()
    {
      var s = Domain.Services.Get<IHashingService>("MD5");
      Assert.IsNotNull(s);
      Assert.IsInstanceOf<MD5HashingService>(s);

      s = Domain.Services.Get<IHashingService>("SHA1");
      Assert.IsNotNull(s);
      Assert.IsInstanceOf<SHA1HashingService>(s);

      s = Domain.Services.Get<IHashingService>("SHA256");
      Assert.IsNotNull(s);
      Assert.IsInstanceOf<SHA256HashingService>(s);

      s = Domain.Services.Get<IHashingService>("SHA384");
      Assert.IsNotNull(s);
      Assert.IsInstanceOf<SHA384HashingService>(s);

      s = Domain.Services.Get<IHashingService>("SHA512");
      Assert.IsNotNull(s);
      Assert.IsInstanceOf<SHA512HashingService>(s);

      s = Domain.Services.Get<IHashingService>("PLAIN");
      Assert.IsNotNull(s);
      Assert.IsInstanceOf<PlainHashingService>(s);
    }

    public HashingServicesTests()
    {
      values = Assembly.GetExecutingAssembly().GetTypes().Select(t => t.Name).ToList();
    }
  }
}
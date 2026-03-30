// Copyright (C) 2011-2021 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Dmitri Maximov
// Created:    2011.05.30

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using Xtensive.Orm.Security.Cryptography;

namespace Xtensive.Orm.Security.Tests
{
  [TestFixture]
  public class HashingServicesTests : SecurityTestBase
  {
    private readonly List<string> values;

    [Test]
    public void PlainHashingServiceTest()
    {
      var service = new Cryptography.PlainHashingService();

      foreach (string value in values)
        Assert.That(service.ComputeHash(value), Is.EqualTo(value));
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
        Assert.That(hash, Is.Not.Empty);
        Assert.That(service2.VerifyHash(value, hash), Is.True);
        Assert.That(service2.VerifyHash(value, Convert.ToBase64String(new byte[] {33, 32,23,23,23,23,23,23,23,23,2,32,3,23,23,23})), Is.False);
      }
    }

    [Test]
    public void InitializationTest()
    {
      var s = Domain.Services.Get<IHashingService>("md5");
      Assert.That(s, Is.Not.Null);
      Assert.That(s, Is.InstanceOf<MD5HashingService>());

      s = Domain.Services.Get<IHashingService>("sha1");
      Assert.That(s, Is.Not.Null);
      Assert.That(s, Is.InstanceOf<SHA1HashingService>());

      s = Domain.Services.Get<IHashingService>("sha256");
      Assert.That(s, Is.Not.Null);
      Assert.That(s, Is.InstanceOf<SHA256HashingService>());

      s = Domain.Services.Get<IHashingService>("sha384");
      Assert.That(s, Is.Not.Null);
      Assert.That(s, Is.InstanceOf<SHA384HashingService>());

      s = Domain.Services.Get<IHashingService>("sha512");
      Assert.That(s, Is.Not.Null);
      Assert.That(s, Is.InstanceOf<SHA512HashingService>());

      s = Domain.Services.Get<IHashingService>("plain");
      Assert.That(s, Is.Not.Null);
      Assert.That(s, Is.InstanceOf<PlainHashingService>());
    }

    public HashingServicesTests()
    {
      values = Assembly.GetExecutingAssembly().GetTypes().Select(t => t.Name).ToList();
    }
  }
}
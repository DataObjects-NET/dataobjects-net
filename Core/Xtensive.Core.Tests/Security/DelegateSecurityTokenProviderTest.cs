// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Ivan Galkin
// Created:    2009.03.19

using System;
using System.Diagnostics;
using NUnit.Framework;
using Xtensive.Security;
using Xtensive.Serialization.Binary;

namespace Xtensive.Tests.Security
{
  [TestFixture]
  public class DelegateSecurityTokenProviderTest
  {
    [Test]
    public void GetSecurityTokenTest()
    {
      var userName = "aaa";
      var password = "123";
      var provider = new DelegateSecurityTokenProvider((un, p) => un + ";" + p);
      Assert.AreEqual("aaa;123", provider.GetSecurityToken(userName, password));
    }

    [Test]
    public void SerializationTest()
    {
      var userName = "aaa";
      var password = "123";
      var original = new DelegateSecurityTokenProvider((un, p) => un + ";" + p);
      var deserialized = new LegacyBinarySerializer().Clone(original) as ISecurityTokenProvider;
      Assert.AreEqual(original.GetSecurityToken(userName, password), deserialized.GetSecurityToken(userName, password));
    }
  }
}
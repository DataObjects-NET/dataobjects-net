// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Ivan Galkin
// Created:    2009.03.19

using NUnit.Framework;
using Xtensive.Security;
using Xtensive.Serialization.Binary;

namespace Xtensive.Tests.Security
{
  [TestFixture]
  public class DelegateAuthenticationProviderTest
  {
    [Test]
    public void AuthenticateTest()
    {
      var userName = "123";
      var password1 = "123";
      var password2 = "321";

      var provider = new DelegateAuthenticationProvider((un, p) => un==p);
      Assert.IsTrue(provider.Authenticate(userName, password1));
      Assert.IsFalse(provider.Authenticate(userName, password2));
    }

    [Test]
    public void SerializationTest()
    {
      var userName = "123";
      var password1 = "123";
      var password2 = "321";

      var original = new DelegateAuthenticationProvider((un, p) => un == p);
      var deserialized = new LegacyBinarySerializer().Clone(original) as IAuthenticationProvider;
      Assert.AreEqual(original.Authenticate(userName, password1), deserialized.Authenticate(userName, password1));
      Assert.AreEqual(original.Authenticate(userName, password2), deserialized.Authenticate(userName, password2));
    }
  }
}
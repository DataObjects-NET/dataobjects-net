// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: 
// Created:    2007.07.18

using NUnit.Framework;
namespace Xtensive.Core.Tests.Utility
{
  [TestFixture]
  public class UrlInfoTest
  {
    [Test]
    public void TestHashcode()
    {
      UrlInfo infoA = new UrlInfo("tcp://user:password@someHost:1000/someUrl/someUrl?someParameter=someValue&someParameter2=someValue2");
      UrlInfo infoA2 = new UrlInfo("tcp://user" + ":password@someHost:1000" + "/someUrl/someUrl?someParameter=someValue&someParameter2=someValue2");
      UrlInfo infoA3 = new UrlInfo("tcp://user" + ":password@someHost:1000" + "/someUrl/someUrl?someParameter2=someValue2&someParameter=someValue");
      UrlInfo infoC = new UrlInfo("tcp://user:password@someHost:1000/someUrl/someUrl");

      Assert.IsTrue(infoA.GetHashCode()==infoA2.GetHashCode());
      Assert.IsTrue(infoA.GetHashCode()==infoA3.GetHashCode());
      Assert.IsTrue(infoA.GetHashCode()!=infoC.GetHashCode());

      Assert.IsTrue(infoA.Equals(infoA2));
      Assert.IsFalse(infoA.Equals(infoC));
    }
  }
}
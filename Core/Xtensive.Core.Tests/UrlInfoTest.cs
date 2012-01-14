// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexander Ilyin
// Created:    2007.07.18

using NUnit.Framework;
using Xtensive.Core;

namespace Xtensive.Tests
{
  [TestFixture]
  public class UrlInfoTest
  {
    [Test]
    public void CombinedTest()
    {
      UrlInfo a1 = UrlInfo.Parse("tcp://user:password@someHost:1000/someUrl/someUrl?someParameter=someValue&someParameter2=someValue2");
      UrlInfo a2 = UrlInfo.Parse("tcp://user:password@someHost:1000/someUrl/someUrl?someParameter=someValue&someParameter2=someValue2");
      UrlInfo aX = UrlInfo.Parse("tcp://user:password@someHost:1000/someUrl/someUrl?someParameter2=someValue2&someParameter=someValue");
      UrlInfo b  = UrlInfo.Parse("tcp://user:password@someHost:1000/someUrl/someUrl");

      Assert.IsTrue(a1.GetHashCode()==a2.GetHashCode());
      Assert.IsTrue(a1.GetHashCode()!=aX.GetHashCode());
      Assert.IsTrue(a1.GetHashCode()!=b.GetHashCode());

      Assert.IsTrue(a1.Equals(a2));
      Assert.IsFalse(a1.Equals(aX));
      Assert.IsFalse(a1.Equals(b));
    }
  }
}
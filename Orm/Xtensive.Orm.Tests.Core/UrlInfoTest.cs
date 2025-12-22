// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexander Ilyin
// Created:    2007.07.18

using NUnit.Framework;

namespace Xtensive.Orm.Tests.Core
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

      Assert.That(a1.GetHashCode()==a2.GetHashCode(), Is.True);
      Assert.That(a1.GetHashCode()!=aX.GetHashCode(), Is.True);
      Assert.That(a1.GetHashCode()!=b.GetHashCode(), Is.True);

      Assert.That(a1.Equals(a2), Is.True);
      Assert.That(a1.Equals(aX), Is.False);
      Assert.That(a1.Equals(b), Is.False);
    }
  }
}
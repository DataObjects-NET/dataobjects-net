// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexander Ilyin
// Created:    2007.07.18

using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using NUnit.Framework;

namespace Xtensive.Orm.Tests.Core
{
  [TestFixture]
  public class UrlInfoTest
  {
    private const string TestUrl1 = "tcp://user:password@someHost:1000/someUrl/someUrl?someParameter=someValue&someParameter2=someValue2";
    private const string TestUrl2 = "tcp://user:password@someHost:1000/someUrl/someUrl?someParameter2=someValue2&someParameter=someValue";
    private const string TestUrl3 = "tcp://user:password@someHost:1000/someUrl/someUrl";

    [Test]
    public void CombinedTest()
    {
      UrlInfo a1 = UrlInfo.Parse(TestUrl1);
      UrlInfo a2 = UrlInfo.Parse(TestUrl1);
      UrlInfo aX = UrlInfo.Parse(TestUrl2);
      UrlInfo b  = UrlInfo.Parse(TestUrl3);

      Assert.That(a1.GetHashCode()==a2.GetHashCode(), Is.True);
      Assert.That(a1.GetHashCode()!=aX.GetHashCode(), Is.True);
      Assert.That(a1.GetHashCode()!=b.GetHashCode(), Is.True);

      Assert.That(a1.Equals(a2), Is.True);
      Assert.That(a1.Equals(aX), Is.False);
      Assert.That(a1.Equals(b), Is.False);
    }
  }
}
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

    [Test]
    public void SerializationTest()
    {
      UrlInfo a = UrlInfo.Parse(TestUrl1);
      var clonedAXml = Cloner.CloneViaXmlSerialization<UrlInfo>(a, new DataContractSerializerSettings());
      Assert.That(clonedAXml.Protocol, Is.EqualTo(a.Protocol));
      Assert.That(clonedAXml.Secure, Is.EqualTo(a.Secure));
      Assert.That(clonedAXml.Host, Is.EqualTo(a.Host));
      Assert.That(clonedAXml.Port, Is.EqualTo(a.Port));
      Assert.That(clonedAXml.User, Is.EqualTo(a.User));
      Assert.That(clonedAXml.Password, Is.EqualTo(a.Password));
      Assert.That(clonedAXml.Resource, Is.EqualTo(a.Resource));
      Assert.That(clonedAXml.Params.SequenceEqual(a.Params), Is.True);

      var clonedAJson1 = Cloner.CloneViaJsonSerialization(a, useDataContract: true);
      Assert.That(clonedAJson1.Protocol, Is.EqualTo(a.Protocol));
      Assert.That(clonedAJson1.Secure, Is.EqualTo(a.Secure));
      Assert.That(clonedAJson1.Host, Is.EqualTo(a.Host));
      Assert.That(clonedAJson1.Port, Is.EqualTo(a.Port));
      Assert.That(clonedAJson1.User, Is.EqualTo(a.User));
      Assert.That(clonedAJson1.Password, Is.EqualTo(a.Password));
      Assert.That(clonedAJson1.Resource, Is.EqualTo(a.Resource));
      Assert.That(clonedAJson1.Params.SequenceEqual(a.Params), Is.True);

      var clonedAJson2 = Cloner.CloneViaJsonSerialization(a, useDataContract: false);
      Assert.That(clonedAJson2.Protocol, Is.EqualTo(a.Protocol));
      Assert.That(clonedAJson2.Secure, Is.EqualTo(a.Secure));
      Assert.That(clonedAJson2.Host, Is.EqualTo(a.Host));
      Assert.That(clonedAJson2.Port, Is.EqualTo(a.Port));
      Assert.That(clonedAJson2.User, Is.EqualTo(a.User));
      Assert.That(clonedAJson2.Password, Is.EqualTo(a.Password));
      Assert.That(clonedAJson2.Resource, Is.EqualTo(a.Resource));
      Assert.That(clonedAJson2.Params.SequenceEqual(a.Params), Is.True);


      UrlInfo b = UrlInfo.Parse(TestUrl2);
      var clonedBXml = Cloner.CloneViaXmlSerialization<UrlInfo>(b, new DataContractSerializerSettings());
      Assert.That(clonedBXml.Protocol, Is.EqualTo(b.Protocol));
      Assert.That(clonedBXml.Secure, Is.EqualTo(b.Secure));
      Assert.That(clonedBXml.Host, Is.EqualTo(b.Host));
      Assert.That(clonedBXml.Port, Is.EqualTo(b.Port));
      Assert.That(clonedBXml.User, Is.EqualTo(b.User));
      Assert.That(clonedBXml.Password, Is.EqualTo(b.Password));
      Assert.That(clonedBXml.Resource, Is.EqualTo(b.Resource));
      Assert.That(clonedBXml.Params.SequenceEqual(b.Params), Is.True);

      var clonedBJson1 = Cloner.CloneViaJsonSerialization(b, useDataContract: true);
      Assert.That(clonedBJson1.Protocol, Is.EqualTo(b.Protocol));
      Assert.That(clonedBJson1.Secure, Is.EqualTo(b.Secure));
      Assert.That(clonedBJson1.Host, Is.EqualTo(b.Host));
      Assert.That(clonedBJson1.Port, Is.EqualTo(b.Port));
      Assert.That(clonedBJson1.User, Is.EqualTo(b.User));
      Assert.That(clonedBJson1.Password, Is.EqualTo(b.Password));
      Assert.That(clonedBJson1.Resource, Is.EqualTo(b.Resource));
      Assert.That(clonedBJson1.Params.SequenceEqual(b.Params), Is.True);

      var clonedBJson2 = Cloner.CloneViaJsonSerialization(b, useDataContract: false);
      Assert.That(clonedBJson2.Protocol, Is.EqualTo(b.Protocol));
      Assert.That(clonedBJson2.Secure, Is.EqualTo(b.Secure));
      Assert.That(clonedBJson2.Host, Is.EqualTo(b.Host));
      Assert.That(clonedBJson2.Port, Is.EqualTo(b.Port));
      Assert.That(clonedBJson2.User, Is.EqualTo(b.User));
      Assert.That(clonedBJson2.Password, Is.EqualTo(b.Password));
      Assert.That(clonedBJson2.Resource, Is.EqualTo(b.Resource));
      Assert.That(clonedBJson2.Params.SequenceEqual(b.Params), Is.True);

      UrlInfo c = UrlInfo.Parse(TestUrl3);
      var clonedCXml = Cloner.CloneViaXmlSerialization<UrlInfo>(c, new DataContractSerializerSettings());
      Assert.That(clonedCXml.Protocol, Is.EqualTo(c.Protocol));
      Assert.That(clonedCXml.Secure, Is.EqualTo(c.Secure));
      Assert.That(clonedCXml.Host, Is.EqualTo(c.Host));
      Assert.That(clonedCXml.Port, Is.EqualTo(c.Port));
      Assert.That(clonedCXml.User, Is.EqualTo(c.User));
      Assert.That(clonedCXml.Password, Is.EqualTo(c.Password));
      Assert.That(clonedCXml.Resource, Is.EqualTo(c.Resource));
      Assert.That(clonedCXml.Params.SequenceEqual(c.Params), Is.True);

      var clonedCJson1 = Cloner.CloneViaJsonSerialization(c, useDataContract: true);
      Assert.That(clonedCJson1.Protocol, Is.EqualTo(c.Protocol));
      Assert.That(clonedCJson1.Secure, Is.EqualTo(c.Secure));
      Assert.That(clonedCJson1.Host, Is.EqualTo(c.Host));
      Assert.That(clonedCJson1.Port, Is.EqualTo(c.Port));
      Assert.That(clonedCJson1.User, Is.EqualTo(c.User));
      Assert.That(clonedCJson1.Password, Is.EqualTo(c.Password));
      Assert.That(clonedCJson1.Resource, Is.EqualTo(c.Resource));
      Assert.That(clonedCJson1.Params.SequenceEqual(c.Params), Is.True);

      var clonedCJson2 = Cloner.CloneViaJsonSerialization(c, useDataContract: false);
      Assert.That(clonedCJson2.Protocol, Is.EqualTo(c.Protocol));
      Assert.That(clonedCJson2.Secure, Is.EqualTo(c.Secure));
      Assert.That(clonedCJson2.Host, Is.EqualTo(c.Host));
      Assert.That(clonedCJson2.Port, Is.EqualTo(c.Port));
      Assert.That(clonedCJson2.User, Is.EqualTo(c.User));
      Assert.That(clonedCJson2.Password, Is.EqualTo(c.Password));
      Assert.That(clonedCJson2.Resource, Is.EqualTo(c.Resource));
      Assert.That(clonedCJson2.Params.SequenceEqual(c.Params), Is.True);
    }
  }
}
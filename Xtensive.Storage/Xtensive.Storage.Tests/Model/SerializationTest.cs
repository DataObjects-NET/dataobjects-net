// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.05.21

using System.IO;
using System.Xml.Serialization;
using NUnit.Framework;
using Xtensive.Storage.Configuration;
using Xtensive.Storage.Model;
using Xtensive.Storage.Model.Stored;
using Xtensive.Storage.Tests.ObjectModel.NorthwindDO;

namespace Xtensive.Storage.Tests.Model
{
  [TestFixture]
  public class SerializationTest : AutoBuildTest
  {
    protected override DomainConfiguration BuildConfiguration()
    {
      var configuration = base.BuildConfiguration();
      configuration.Types.Register(typeof (Customer).Assembly, typeof (Customer).Namespace);
      return configuration;
    }

    [Test]
    public void MainTest()
    {
      var stream = new MemoryStream();
      var serializer = new XmlSerializer(typeof(StoredDomainModel));
      serializer.Serialize(stream, Domain.Model.ToStoredModel());
      stream.Seek(0, SeekOrigin.Begin);
      var result = (StoredDomainModel) serializer.Deserialize(stream);
      result.UpdateReferences();
      stream.Close();
    }

    [Test]
    [Explicit]
    public void SaveTest()
    {
      var serializer = new XmlSerializer(typeof(StoredDomainModel));
      using (var stream = new FileStream("C:\\test.xml", FileMode.Create))
        serializer.Serialize(stream, Domain.Model.ToStoredModel());
    }
  }
}
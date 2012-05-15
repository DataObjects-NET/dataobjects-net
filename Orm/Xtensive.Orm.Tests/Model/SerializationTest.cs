// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.05.21

using System.IO;
using NUnit.Framework;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Model;
using Xtensive.Orm.Model.Stored;
using Xtensive.Orm.Tests.ObjectModel.NorthwindDO;

namespace Xtensive.Orm.Tests.Model
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
      var model = Domain.Model.ToStoredModel();
      var serialized = model.Serialize();
      var result = StoredDomainModel.Deserialize(serialized);
      result.UpdateReferences();
    }
  }
}
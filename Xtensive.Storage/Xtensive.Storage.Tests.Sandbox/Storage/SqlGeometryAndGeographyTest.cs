// Copyright (C) 2011 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2011.09.15

using System;
using System.Linq;
using Microsoft.SqlServer.Types;
using NUnit.Framework;
using Xtensive.Storage;
using Xtensive.Storage.Configuration;
using Xtensive.Storage.Tests.Storage.SqlGeometryAndGeographyTestModel;

namespace Xtensive.Storage.Tests.Storage.SqlGeometryAndGeographyTestModel
{
  [HierarchyRoot]
  public class Container : Entity
  {
    [Field, Key]
    public int Id { get; private set; }

    [Field]
    public SqlGeometry Geometry { get; set; }

    [Field]
    public SqlGeography Geography { get; set; }
  }
}

namespace Xtensive.Storage.Tests.Storage
{
  public class SqlGeometryAndGeographyTest : AutoBuildTest
  {
    protected override DomainConfiguration BuildConfiguration()
    {
      var config = base.BuildConfiguration();
      config.Types.Register(typeof (Container).Assembly, typeof (Container).Namespace);
      return config;
    }

    [Test]
    public void MainTest()
    {
      using (Session.Open(Domain)) {
        using (var t = Transaction.Open()) {
          
          new Container
            {
              Geometry = new SqlGeometry(),
              Geography = new SqlGeography()
            };
          t.Complete();
        }
        using (var t = Transaction.Open()) {

          var c = Query.All<Container>().First();
          Assert.IsNotNull(c.Geometry);
          Assert.IsNotNull(c.Geography);
          c = Query.All<Container>().Where(i => i.Geometry != null).First();
          Assert.IsNotNull(c.Geometry);
          c = Query.All<Container>().Where(i => i.Geography!= null).First();
          Assert.IsNotNull(c.Geography);
          t.Complete();
        }
      }
    }
  }
}
// Copyright (C) 2011 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2011.09.15

using System;
using System.Linq;
using Microsoft.SqlServer.Types;
using NUnit.Framework;
using Xtensive.Orm;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Tests;
using Xtensive.Orm.Tests.Storage.SqlGeometryAndGeographyTestModel;

namespace Xtensive.Orm.Tests.Storage.SqlGeometryAndGeographyTestModel
{
  [HierarchyRoot]
  public class Container : Entity
  {
    [Field, Key]
    public int Id { get; private set; }

    [Field(Indexed = true)]
    public SqlGeometry Geometry { get; set; }

    [Field(Indexed=true)]
    public SqlGeography Geography { get; set; }
  }
}

namespace Xtensive.Orm.Tests.Storage
{
  public class SqlGeometryAndGeographyTest : AutoBuildTest
  {
    protected override DomainConfiguration BuildConfiguration()
    {
      var config = base.BuildConfiguration();
      config.Types.Register(typeof (Container).Assembly, typeof (Container).Namespace);
      return config;
    }

    protected override void CheckRequirements()
    {
      Require.ProviderIs(StorageProvider.SqlServer);
      Require.ProviderVersionAtLeast(new Version(10, 0));
    }

    [Test]
    public void MainTest()
    {
      using (Domain.OpenSession()) {
        using (var t = Session.Current.OpenTransaction()) {
          
          new Container
            {
              Geometry = new SqlGeometry(),
              Geography = new SqlGeography()
            };
          new Container
            {
              Geometry = null,
              Geography = null
            };
          t.Complete();
        }
        using (var t = Session.Current.OpenTransaction()) {

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
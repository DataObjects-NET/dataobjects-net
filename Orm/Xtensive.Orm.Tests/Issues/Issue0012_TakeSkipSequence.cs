// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.11.20

using System;
using System.Linq;
using NUnit.Framework;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Providers;
using Xtensive.Orm.Rse;
using Xtensive.Orm.Tests.Issues.Issue0012_Model;

namespace Xtensive.Orm.Tests.Issues.Issue0012_Model
{
  [Serializable]
  [HierarchyRoot]
  public class MyEntity : Entity
  {
    [Field, Key]
    public int Id { get; private set; }

    [Field]
    public int Field1 { get; set; }

    [Field]
    public int Field2 { get; set; }

    [Field]
    public int Field3 { get; set; }
  }
}

namespace Xtensive.Orm.Tests.Issues
{
  public class Issue0012_TakeSkipSequence : AutoBuildTest
  {
    protected override void CheckRequirements()
    {
      base.CheckRequirements();
      Require.AnyFeatureSupported(ProviderFeatures.RowNumber | ProviderFeatures.NativePaging);
    }

    protected override DomainConfiguration BuildConfiguration()
    {
      var config = base.BuildConfiguration();
      config.Types.Register(typeof (MyEntity).Assembly, typeof (MyEntity).Namespace);
      return config;
    }

    [Test]
    public void MainTest()
    {
      Assert.AreEqual(0, new[] {1, 2}.Take(1).Skip(1).Count());

      using (var session = Domain.OpenSession())
      using (var trs = session.OpenTransaction()) {
        var e1 = new MyEntity();
        var e2 = new MyEntity();
        RecordQuery rsMyEntities = Domain.Model.Types[typeof (MyEntity)]
          .Indexes.PrimaryIndex.ToRecordQuery()
          .Filter(t => t.GetValue<int>(0) == e1.Id || t.GetValue<int>(0) == e2.Id);

        Assert.AreEqual(2, rsMyEntities.Count(Session.Current));

        Assert.AreEqual(0, rsMyEntities.Take(1).Skip(1).Count(Session.Current));
        Assert.AreEqual(1, rsMyEntities.Skip(1).Take(1).Count(Session.Current));
        Assert.AreEqual(1, rsMyEntities.Take(1).Take(2).Count(Session.Current));
        Assert.AreEqual(0, rsMyEntities.Skip(1).Skip(1).Count(Session.Current));
        trs.Complete();
      }
    }
  }
}
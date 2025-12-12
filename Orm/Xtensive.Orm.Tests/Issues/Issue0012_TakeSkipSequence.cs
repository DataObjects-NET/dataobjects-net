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
using Xtensive.Orm.Rse.Providers;
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
      config.Types.RegisterCaching(typeof (MyEntity).Assembly, typeof (MyEntity).Namespace);
      return config;
    }

    [Test]
    public void MainTest()
    {
      Assert.That(new[] {1, 2}.Take(1).Skip(1).Count(), Is.EqualTo(0));

      using (var session = Domain.OpenSession())
      using (var trs = session.OpenTransaction()) {
        var e1 = new MyEntity();
        var e2 = new MyEntity();
        CompilableProvider rsMyEntities = Domain.Model.Types[typeof(MyEntity)]
          .Indexes.PrimaryIndex.GetQuery()
          .Filter(t => t.GetValue<int>(0) == e1.Id || t.GetValue<int>(0) == e2.Id);

        Assert.That(rsMyEntities.Count(Session.Current), Is.EqualTo(2));

        Assert.That(rsMyEntities.Take(1).Skip(1).Count(Session.Current), Is.EqualTo(0));
        Assert.That(rsMyEntities.Skip(1).Take(1).Count(Session.Current), Is.EqualTo(1));
        Assert.That(rsMyEntities.Take(1).Take(2).Count(Session.Current), Is.EqualTo(1));
        Assert.That(rsMyEntities.Skip(1).Skip(1).Count(Session.Current), Is.EqualTo(0));
        trs.Complete();
      }
    }
  }
}
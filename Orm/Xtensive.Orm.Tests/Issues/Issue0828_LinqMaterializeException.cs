// Copyright (C) 2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2010.10.11

using System;
using System.Diagnostics;
using System.Linq;
using NUnit.Framework;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Providers;
using Xtensive.Orm.Tests.Issues_Issue0828_LinqMaterializeException;

namespace Xtensive.Orm.Tests.Issues_Issue0828_LinqMaterializeException
{
  [Serializable]
  [HierarchyRoot]
  public class MyEntity : Entity
  {
    [Field, Key]
    public int Id { get; private set; }

    [Field]
    public DateTime? Date { get; set; }

    [Field(Length = 100)]
    public string Text { get; set; }

    [Field]
    [Association(PairTo = "Owner")]
    public EntitySet<Info> Infos { get; private set; }
  }

  [HierarchyRoot]
  public class Info : Entity
  {
    [Field, Key]
    public int Id { get; private set; }

    [Field]
    public MyEntity Owner { get; set; }
  }

  public class SuccessInfo : Info
  {
  }

  public class ErrorInfo : Info
  {
  }

  public enum Status
  {
    Unknown,
    Success,
    Error
  }
}

namespace Xtensive.Orm.Tests.Issues
{
  [TestFixture]
  public class Issue0828_LinqMaterializeException : AutoBuildTest
  {
    protected override DomainConfiguration BuildConfiguration()
    {
      var config = base.BuildConfiguration();
      config.Types.Register(typeof (MyEntity).Assembly, typeof (MyEntity).Namespace);
      return config;
    }

    [Test]
    public void MainTest()
    {
      Require.AllFeaturesSupported(ProviderFeatures.ScalarSubqueries);
      using (var session = Domain.OpenSession()) {
        using (var transaction = session.OpenTransaction()) {
          var myEntity = new MyEntity() {
            Date = DateTime.Now,
            Text = "Text"
          };
          var query = session.Query.All<MyEntity>()
            .Select(e => new {
              MyEntity = e,
              Year = (int?) e.Date.Value.Year,
              Status = e.Infos.OfType<SuccessInfo>().Any() ? Status.Success : (e.Infos.OfType<ErrorInfo>().Any() ? Status.Error : Status.Unknown)
            })
            .Select(o => new object[] {o.MyEntity.Id, o.Year, o.Status});
          var result = query.ToList();
        }
      }
    }
  }
}
// Copyright (C) 2012 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2012.02.17

using System.Linq;
using NUnit.Framework;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Tests.Issues.IssueJira0330_MaterializationOfNullReferenceModel;

namespace Xtensive.Orm.Tests.Issues
{
  namespace IssueJira0330_MaterializationOfNullReferenceModel
  {
    [HierarchyRoot]
    public class Referencer : Entity
    {
      [Key, Field]
      public int Id { get; private set; }

      [Field]
      public Referencer Ref { get; set; }
    }
  }

  public class IssueJira0330_MaterializationOfNullReference : AutoBuildTest
  {
    protected override DomainConfiguration BuildConfiguration()
    {
      var configuration = base.BuildConfiguration();
      configuration.Types.Register(typeof (Referencer).Assembly, typeof (Referencer).Namespace);
      return configuration;
    }

    protected override void PopulateData()
    {
      using (var session = Domain.OpenSession())
      using (var t = session.OpenTransaction()) {
        new Referencer();
        t.Complete();
      }
    }

    [Test]
    public void MainTest()
    {
      using (var session = Domain.OpenSession())
      using (var t = session.OpenTransaction()) {
        var query = Query.Execute(() => Query.All<Referencer>().Where(r => r.Id > 0).Select(r => r.Ref));
        var result = query.ToList();
        Assert.That(result.Count, Is.EqualTo(1));
        Assert.That(result[0], Is.Null);
      }
    }
  }
}
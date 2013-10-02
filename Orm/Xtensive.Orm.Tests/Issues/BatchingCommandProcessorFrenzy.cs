// Copyright (C) 2012 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2012.02.20

using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Tests.Issues.BatchingCommandProcessorFrenzyModel;

namespace Xtensive.Orm.Tests.Issues
{
  namespace BatchingCommandProcessorFrenzyModel
  {
    [HierarchyRoot]
    public class FrenzyEntity : Entity
    {
      [Field, Key]
      public int Id { get; private set; }
    }
  }

  public class BatchingCommandProcessorFrenzy : AutoBuildTest
  {
    protected override DomainConfiguration BuildConfiguration()
    {
      var configuration = base.BuildConfiguration();
      configuration.Types.Register(typeof (FrenzyEntity).Assembly, typeof (FrenzyEntity).Namespace);
      return configuration;
    }

    private int GetParameter()
    {
      var e1 = Query.All<FrenzyEntity>().FirstOrDefault();
      var e2 = Query.All<FrenzyEntity>().FirstOrDefault();
      return 0;
    }

    [Test]
    public void Test()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var x = Query.All<FrenzyEntity>().Count(e => e.Id==GetParameter());
      }
    }
  }
}
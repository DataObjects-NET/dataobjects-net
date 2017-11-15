// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Kofman
// Created:    2009.10.29

using System;
using System.Linq;
using NUnit.Framework;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Tests.Issues.Issue0457_EntityStateToStringModel;

namespace Xtensive.Orm.Tests.Issues.Issue0457_EntityStateToStringModel
{
  [Serializable]
  [HierarchyRoot]
  public class BuggyEntity : Entity
  {
    [Key, Field]
    public int Id { get; private set; }
  }
}

namespace Xtensive.Orm.Tests.Issues
{
  public class Issue0457_EntityStateToString : AutoBuildTest
  {
    protected override DomainConfiguration BuildConfiguration()
    {
      var config = base.BuildConfiguration();
      config.Types.Register(typeof (BuggyEntity).Assembly, typeof (BuggyEntity).Namespace);
      return config;
    }

    [Test]
    public void MainTest()
    {
      using (var session = Domain.OpenSession()) {
        using (var transactionScope = session.OpenTransaction()) {

          session.Query.SingleOrDefault<BuggyEntity>(1001);
          var entityState = session.EntityStateCache.FirstOrDefault();

          if (entityState!=null)
            Console.WriteLine(entityState.ToString()); // NullReferenceException is thrown here
        }
      }
    }
  }
}
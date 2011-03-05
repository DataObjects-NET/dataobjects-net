// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2009.08.24

using System;
using NUnit.Framework;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Tests.Issues.Issue0351_NameBuilderProblem_Model;

namespace Xtensive.Orm.Tests.Issues.Issue0351_NameBuilderProblem_Model
{
  [Serializable]
  [HierarchyRoot]
  public class Master : Entity
  {
    [Field, Key]
    public int Id { get; private set; }

    [Field]
    public EntitySet<Slave> Slaves { get; private set; }
  }

  [Serializable]
  [HierarchyRoot]
  public class Slave : Entity
  {
    [Field, Key]
    public int Id { get; private set; }
  }
}

namespace Xtensive.Orm.Tests.Issues
{
  public class Issue0351_NameBuilderProblem : AutoBuildTest
  {
    protected override DomainConfiguration BuildConfiguration()
    {
      var config = base.BuildConfiguration();
      config.Types.Register(typeof (Master).Assembly, typeof (Master).Namespace);
      config.NamingConvention.NamespacePolicy = NamespacePolicy.AsIs;
      return config;
    }

    [Test]
    public void MainTest()
    {
      using (var session = Domain.OpenSession()) {
        using (var t = session.OpenTransaction()) {
          

          // Rollback
        }
      }
    }
  }
}
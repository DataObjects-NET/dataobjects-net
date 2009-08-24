// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2009.08.24

using NUnit.Framework;
using Xtensive.Storage.Configuration;
using Xtensive.Storage.Tests.Issues.Issue0351_NameBuilderProblem_Model;

namespace Xtensive.Storage.Tests.Issues.Issue0351_NameBuilderProblem_Model
{
  [HierarchyRoot]
  public class Master : Entity
  {
    [Field, Key]
    public int Id { get; private set; }

    [Field]
    public EntitySet<Slave> Slaves { get; }
  }

  [HierarchyRoot]
  public class Slave : Entity
  {
    [Field, Key]
    public int Id { get; private set; }
  }
}

namespace Xtensive.Storage.Tests.Issues
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
      using (Session.Open(Domain)) {
        using (var t = Transaction.Open()) {
          

          // Rollback
        }
      }
    }
  }
}
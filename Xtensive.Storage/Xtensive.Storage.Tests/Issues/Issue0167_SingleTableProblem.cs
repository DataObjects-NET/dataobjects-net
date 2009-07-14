// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Ivan Galkin
// Created:    2009.06.09

using NUnit.Framework;
using Xtensive.Storage.Configuration;
using Xtensive.Storage.Model;
using Xtensive.Storage.Tests.Issues.Issue0167_SingleTableProblem_Model;

namespace Xtensive.Storage.Tests.Issues.Issue0167_SingleTableProblem_Model
{
  [HierarchyRoot(InheritanceSchema = InheritanceSchema.SingleTable)]
  public class Ancestor : Entity
  {
    [Field, Key]
    public int Id { get; private set; }
  }

  public class Descendant : Ancestor
  {
    [Field]
    public int NotNullableField { get; set; }
  }
}

namespace Xtensive.Storage.Tests.Issues
{
  [TestFixture]
  public class Issue0167_SingleTableProblem : AutoBuildTest
  {
    protected override DomainConfiguration BuildConfiguration()
    {
      var config = base.BuildConfiguration();
      config.Types.Register(typeof (Ancestor).Assembly, typeof (Ancestor).Namespace);
      return config;
    }

    [Test]
    public void MainTest()
    {
      using (Session.Open(Domain)) {
        using (var t = Transaction.Open()) {
          new Ancestor();
          t.Complete();
        }
      }
    }
  }
}
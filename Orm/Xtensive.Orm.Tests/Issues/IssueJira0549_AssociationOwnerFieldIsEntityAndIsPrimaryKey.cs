// Copyright (C) 2014 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kulakov
// Created:    2014.08.19

using NUnit.Framework;
using Xtensive.Orm.Tests.Issues.IssueJira0549_AssociationOwnerFieldIsEntityAndIsPrimaryKeyModel;

namespace Xtensive.Orm.Tests.Issues.IssueJira0549_AssociationOwnerFieldIsEntityAndIsPrimaryKeyModel
{
  [HierarchyRoot]
  [KeyGenerator(KeyGeneratorKind.None)]
  public  class Person : Entity
  {
    [Key, Field]
    [Association]
    public User Entity { get; private set; }

    public Person(Session session, User user)
      : base(session)
    {
      Entity = user;
    }
  }

  [HierarchyRoot]
  public class User : Entity
  {
    [Key,Field]
    public int Id { get; private set; }

    [Field]
    public string Name { get; set; }
  }
}

namespace Xtensive.Orm.Tests.Issues
{
  [TestFixture]
  public class IssueJira0549_AssociationOwnerFieldIsEntityAndIsPrimaryKey : AutoBuildTest
  {
    [Test]
    public void MainTest()
    {
      var domainConfiguration = base.BuildConfiguration();
      domainConfiguration.Types.Register(typeof(Person).Assembly, typeof(Person).Namespace);
      
      Assert.DoesNotThrow(()=> {
        var domain = BuildDomain(domainConfiguration);
        domain.Dispose();
      });
    }
  }
}

// Copyright (C) 2014 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kulakov
// Created:    2014.06.17

using NUnit.Framework;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Model;
using Xtensive.Orm.Tests.Issues.IssueJira0541_IncorrectMappingNameInSingleTableModel;

namespace Xtensive.Orm.Tests.Issues.IssueJira0541_IncorrectMappingNameInSingleTableModel
{
  [HierarchyRoot(InheritanceSchema.SingleTable)]
  public abstract class BaseClass : Entity
  {
    [Field, Key]
    public int Id { get; private set; }
  }

  public class FirstChild : BaseClass
  {
    [Field]
    public string Name { get; set; }

    [Field]
    public string SomeFirstChildField { get; set; }
  }

  public class SecondChild : BaseClass
  {
    [Field]
    public string Name { get; set; }

    [Field]
    public string SomeSecondChildField { get; set; }
  }
}

namespace Xtensive.Orm.Tests.Issues
{
  public class IssueJira0541_IncorrectMappingNameInSingleTable : AutoBuildTest
  {
    [Test]
    public void MainTest()
    {
      Assert.AreEqual(Domain.Model.Types[typeof (FirstChild)].Fields["Name"].MappingName, "Name");
      Assert.AreEqual(Domain.Model.Types[typeof (SecondChild)].Fields["Name"].MappingName, "SecondChild.Name");
    }

    protected override DomainConfiguration BuildConfiguration()
    {
      var configuration = base.BuildConfiguration();
      configuration.Types.Register(typeof (BaseClass).Assembly, typeof (BaseClass).Namespace);
      configuration.UpgradeMode = DomainUpgradeMode.Recreate;
      return configuration;
    }
  }
}

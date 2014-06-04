// Copyright (C) 2014 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kulakov
// Created:    2014.06.04

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Xtensive.Orm.Upgrade;
using model1 = Xtensive.Orm.Tests.Issues.IssueJira0540_IncorrectBehaviorOnDomainUpgradeWithRecycledTypeBugModel1;
using model2 = Xtensive.Orm.Tests.Issues.IssueJira0540_IncorrectBehaviorOnDomainUpgradeWithRecycledTypeBugModel2;

namespace Xtensive.Orm.Tests.Issues.IssueJira0540_IncorrectBehaviorOnDomainUpgradeWithRecycledTypeBugModel1
{
  [HierarchyRoot]
  [Recycled]
  public class Recycled : Entity
  {
    [Key]
    [Field(Nullable = false)]
    public Guid Id { get; set; }

    [Field]
    public string Name { get; set; }
  }

  [HierarchyRoot]
  public class Test : Entity
  {
    [Key]
    [Field(Nullable = false)]
    public Guid Id { get; set; }

    [Field]
    public string Name { get; set; }

    [Field]
    [Recycled]
    public Recycled Link { get; set; }
  }
}

namespace Xtensive.Orm.Tests.Issues.IssueJira0540_IncorrectBehaviorOnDomainUpgradeWithRecycledTypeBugModel2
{
  [HierarchyRoot]
  [Recycled("Some.Namespace.SomeType")]
  public class Recycled : Entity
  {
    [Key]
    [Field(Nullable = false)]
    public Guid Id { get; set; }

    [Field]
    public string Name { get; set; }
  }

  [HierarchyRoot]
  public class Test : Entity
  {
    [Key]
    [Field(Nullable = false)]
    public Guid Id { get; set; }

    [Field]
    public string Name { get; set; }

    [Field]
    [Recycled]
    public Recycled Link { get; set; }
  }
}

namespace Xtensive.Orm.Tests.Issues
{
  [TestFixture]
  public class IssueJira0540_IncorrectBehaviorOnDomainUpgradeWithRecycledTypeBug : AutoBuildTest
  {
    [Test]
    public void OldTypeDoesNotDefinedInAttributeTest()
    {
      var domainConfiguration = DomainConfigurationFactory.Create();
      domainConfiguration.Types.Register(typeof (model1.Recycled).Assembly, typeof (model1.Recycled).Namespace);
      domainConfiguration.UpgradeMode = DomainUpgradeMode.Recreate;

      using (var domain = Domain.Build(domainConfiguration)) { }

      domainConfiguration = domainConfiguration.Clone();
      domainConfiguration.UpgradeMode = DomainUpgradeMode.PerformSafely;

      Assert.DoesNotThrow(()=>Domain.Build(domainConfiguration));
    }

    [Test]
    public void OldTypeDefinedInAttributeTest()
    {
      var domainConfiguration = DomainConfigurationFactory.Create();
      domainConfiguration.Types.Register(typeof (model2.Recycled).Assembly, typeof (model2.Recycled).Namespace);
      domainConfiguration.UpgradeMode = DomainUpgradeMode.Recreate;

      using (var domain = Domain.Build(domainConfiguration)) { }

      domainConfiguration = domainConfiguration.Clone();
      domainConfiguration.UpgradeMode = DomainUpgradeMode.PerformSafely;
      Assert.Throws<InvalidOperationException>(() => Domain.Build(domainConfiguration));
    }
  }
}

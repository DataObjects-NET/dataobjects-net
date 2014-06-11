﻿// Copyright (C) 2014 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kulakov
// Created:    2014.05.29

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Xtensive.Orm.Upgrade;
using model1 = Xtensive.Orm.Tests.Upgrade.DisableAutoResolveNamespaceConflictsTestModel1;
using model2 = Xtensive.Orm.Tests.Upgrade.DisableAutoResolveNamespaceConflictsTestModel2;
using model3 = Xtensive.Orm.Tests.Upgrade.DisableAutoResolveNamespaceConflictsTestModel3;

namespace Xtensive.Orm.Tests.Upgrade.DisableAutoResolveNamespaceConflictsTestModel1
{
  [HierarchyRoot]
  public class Foo : Entity
  {
    [Key, Field]
    public int ID { get; private set; }

    [Field]
    public string NotEmpty { get; set; }
  }
}

namespace Xtensive.Orm.Tests.Upgrade.DisableAutoResolveNamespaceConflictsTestModel2
{
  [HierarchyRoot]
  public class Foo : Entity
  {
    [Key, Field]
    public int ID { get; private set; }

    [Field]
    public string NotEmpty { get; set; }
  }
}

namespace Xtensive.Orm.Tests.Upgrade.DisableAutoResolveNamespaceConflictsTestModel3
{
  [HierarchyRoot]
  public class Foo : Entity
  {
    [Key, Field]
    public int ID { get; private set; }
  }

  public class Upgrader : UpgradeHandler
  {
    public override bool CanUpgradeFrom(string oldVersion)
    {
      return true;
    }

    public override bool AutodetectTypesMovements
    {
      get { return false; }
    }
  }
}


namespace Xtensive.Orm.Tests.Upgrade
{
  [TestFixture]
  public class DisableAutoResolveNamespaceConflictsTest : AutoBuildTest
  {
    [Test]
    public void UpgradeWhenAutoResolveIsEnabled()
    {
      var firstModelConfiguration = DomainConfigurationFactory.Create();
      firstModelConfiguration.Types.Register(typeof (model1.Foo));
      firstModelConfiguration.UpgradeMode = DomainUpgradeMode.Recreate;
      var domain = BuildDomain(firstModelConfiguration);
      domain.Dispose();

      var secondModelConfiguration = DomainConfigurationFactory.Create();
      secondModelConfiguration.Types.Register(typeof (model2.Foo));
      secondModelConfiguration.UpgradeMode = DomainUpgradeMode.PerformSafely;
      
      Assert.DoesNotThrow(()=>BuildDomain(secondModelConfiguration));
    }

    [Test]
    public void UpgradeWhenAutoResolveIsDisabled()
    {
      var firstModelConfiguration = DomainConfigurationFactory.Create();
      firstModelConfiguration.Types.Register(typeof (model1.Foo));
      firstModelConfiguration.UpgradeMode = DomainUpgradeMode.Recreate;
      var domain = BuildDomain(firstModelConfiguration);
      domain.Dispose();

      var secondModelConfiguration = DomainConfigurationFactory.Create();
      secondModelConfiguration.Types.Register(typeof (model3.Foo));
      secondModelConfiguration.Types.Register(typeof (model3.Upgrader));
      secondModelConfiguration.UpgradeMode = DomainUpgradeMode.PerformSafely;

      Assert.Throws<SchemaSynchronizationException>(() => BuildDomain(secondModelConfiguration));
    }

  }
}

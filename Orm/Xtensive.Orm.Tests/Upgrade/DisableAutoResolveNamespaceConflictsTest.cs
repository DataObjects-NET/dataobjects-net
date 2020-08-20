// Copyright (C) 2014-2020 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Alexey Kulakov
// Created:    2014.05.29

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using Xtensive.Orm.Configuration;
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
    public override void OnBeforeStage()
    {
      TypesMovementsAutoDetection = false;
    }
  }
}


namespace Xtensive.Orm.Tests.Upgrade
{
  [TestFixture]
  public class DisableAutoResolveNamespaceConflictsTest
  {
    [SetUp]
    public void SetUp()
    {
      var firstModelConfiguration = DomainConfigurationFactory.Create();
      firstModelConfiguration.Types.Register(typeof(model1.Foo));
      firstModelConfiguration.UpgradeMode = DomainUpgradeMode.Recreate;
      BuildDomain(firstModelConfiguration).Dispose();
    }

    [Test]
    public void UpgradeWhenAutoResolveIsEnabledTest()
    {
      var secondModelConfiguration = DomainConfigurationFactory.Create();
      secondModelConfiguration.Types.Register(typeof(model2.Foo));
      secondModelConfiguration.UpgradeMode = DomainUpgradeMode.PerformSafely;
      Assert.DoesNotThrow(()=> BuildDomain(secondModelConfiguration));
    }

    [Test]
    public void UpgradeWhenAutoResolveIsEnabledAsyncTest()
    {
      var secondModelConfiguration = DomainConfigurationFactory.Create();
      secondModelConfiguration.Types.Register(typeof(model2.Foo));
      secondModelConfiguration.UpgradeMode = DomainUpgradeMode.PerformSafely;
      Assert.DoesNotThrowAsync(async () => await BuildDomainAsync(secondModelConfiguration));
    }

    [Test]
    public void UpgradeWhenAutoResolveIsDisabledTest()
    {
      var secondModelConfiguration = DomainConfigurationFactory.Create();
      secondModelConfiguration.Types.Register(typeof(model3.Foo));
      secondModelConfiguration.Types.Register(typeof(model3.Upgrader));
      secondModelConfiguration.UpgradeMode = DomainUpgradeMode.PerformSafely;

      _ = Assert.Throws<SchemaSynchronizationException>(() => BuildDomain(secondModelConfiguration));
    }

    [Test]
    public void UpgradeWhenAutoResolveIsDisabledAsyncTest()
    {
      var secondModelConfiguration = DomainConfigurationFactory.Create();
      secondModelConfiguration.Types.Register(typeof(model3.Foo));
      secondModelConfiguration.Types.Register(typeof(model3.Upgrader));
      secondModelConfiguration.UpgradeMode = DomainUpgradeMode.PerformSafely;

      _ = Assert.ThrowsAsync<SchemaSynchronizationException>(async () => await BuildDomainAsync(secondModelConfiguration));
    }

    private Domain BuildDomain(DomainConfiguration configuration) => Domain.Build(configuration);

    private Task<Domain> BuildDomainAsync(DomainConfiguration configuration) => Domain.BuildAsync(configuration);
  }
}

using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using Xtensive.Orm.Model;
using Before = Xtensive.Orm.Tests.Upgrade.WeirdSequenceErrorModel.Before;
using After = Xtensive.Orm.Tests.Upgrade.WeirdSequenceErrorModel.After;

namespace Xtensive.Orm.Tests.Upgrade.WeirdSequenceErrorModel
{
  namespace Before
  {
    [HierarchyRoot(InheritanceSchema.ConcreteTable)]
    public class VisitDriveTime : Entity
    {
      [Key, Field]
      public long Id { get; set; }

      public VisitDriveTime(Session session) : base(session)
      {
      }
    }

    [TableMapping("VisitDriveTime")]
    [HierarchyRoot]
    public class VisitTripInfo : Entity
    {
      [Key, Field]
      public long Id { get; private set; }

      public VisitTripInfo(Session session) : base(session)
      {
      }
    }
  }

  namespace After
  {
    [HierarchyRoot(InheritanceSchema.ConcreteTable)]
    public class VisitDriveTime : Entity
    {
      [Key, Field]
      public long Id { get; set; }

      public VisitDriveTime(Session session) : base(session)
      {
      }
    }

    [TableMapping("VisitDriveTime")]
    [HierarchyRoot]
    //[Index(nameof(Id), Name = "PK_VisitDriveTime")]
    public class VisitTripInfo : Entity
    {
      [Key, Field]
      public long Id { get; private set; }

      public VisitTripInfo(Session session) : base(session)
      {
      }
    }
  }
}

namespace Xtensive.Orm.Tests.Upgrade
{
  [TestFixture]
  public class WeirdSequenceError
  {
    [Test]
    public void OnlyNamespaceChangeTest()
    {
      var initConfig = DomainConfigurationFactory.Create();
      initConfig.Types.Register(typeof(Before.VisitDriveTime));
      initConfig.UpgradeMode = DomainUpgradeMode.Recreate;

      using (var domain = Domain.Build(initConfig)) { }

      var upgradeConfig = DomainConfigurationFactory.Create();
      upgradeConfig.Types.Register(typeof(After.VisitDriveTime));
      upgradeConfig.UpgradeMode = DomainUpgradeMode.Default;

      using (var domain = Domain.Build(upgradeConfig)) { }
    }

    [Mute]
    [Test]
    public void MainDomainTest()
    {
      var initConfig = DomainConfigurationFactory.Create();
      initConfig.Types.Register(typeof(Before.VisitDriveTime));
      initConfig.UpgradeMode = DomainUpgradeMode.Recreate;

      using (var domain = Domain.Build(initConfig)) { }

      var upgradeConfig = DomainConfigurationFactory.Create();
      upgradeConfig.Types.Register(typeof(After.VisitTripInfo));
      upgradeConfig.UpgradeMode = DomainUpgradeMode.Default;

      using (var domain = Domain.Build(upgradeConfig)) { }

    }
  }
}

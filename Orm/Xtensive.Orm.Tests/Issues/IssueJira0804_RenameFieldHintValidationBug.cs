// Copyright (C) 2024 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.

using System;
using System.Collections.Generic;
using NUnit.Framework;
using Xtensive.Orm.Upgrade;
using Xtensive.Orm.Tests.Issues.IssueJira0804_RenameFieldHintValidationBugModel;


namespace Xtensive.Orm.Tests.Issues.IssueJira0804_RenameFieldHintValidationBugModel
{
  [Serializable]
  [HierarchyRoot]
  public class Person : Entity
  {
    [Key, Field]
    public long Id { get; private set; }
  }

  [Serializable]
  [HierarchyRoot]
  public class Address : Entity
  {
    [Key, Field]
    public long Id { get; private set; }
    [Field]
    public Person Person { get; set; }
    [Field]
    public bool NewName { get; set; }
  }

  public class Upgrader : UpgradeHandler
  {
    public override bool CanUpgradeFrom(string oldVersion) => true;

    protected override void AddUpgradeHints(ISet<UpgradeHint> hints)
    {
      base.AddUpgradeHints(hints);
      _ = hints.Add(
        new RenameFieldHint(typeof(Address), "FieldToRename", "NewName")
      );
    }
  }
}

namespace Xtensive.Orm.Tests.Issues
{
  [TestFixture]
  public sealed class IssueJira0804_RenameFieldHintValidationBug
  {
    [Test]
    public void MainTest()
    {
      var initConfig = DomainConfigurationFactory.Create();
      initConfig.Types.Register(typeof(Person));
      // I don't include this type for it to not exist in database before upgrade
      // this type is delcared in the hint
      //initConfig.Types.Register(typeof(Address));
      initConfig.UpgradeMode = DomainUpgradeMode.Recreate;

      using (var domain = Domain.Build(initConfig)) { }

      var upgradeConfig = DomainConfigurationFactory.Create();
      upgradeConfig.Types.Register(typeof(Person));
      upgradeConfig.Types.Register(typeof(Address));
      upgradeConfig.Types.Register(typeof(Upgrader));
      upgradeConfig.UpgradeMode = DomainUpgradeMode.PerformSafely;

      _ = Assert.Throws<InvalidOperationException>(() => {
        using var domain = Domain.Build(upgradeConfig);
      });
    }
  }
}

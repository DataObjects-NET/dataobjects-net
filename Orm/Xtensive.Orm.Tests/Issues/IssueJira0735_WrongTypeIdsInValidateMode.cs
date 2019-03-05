// Copyright (C) 2018 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kulakov
// Created:    2018.06.17

using System;
using NUnit.Framework;
using Xtensive.Orm.Configuration;
using V1 = Xtensive.Orm.Tests.Issues.IssueJira0735_WrongTypeIdsInValidateModeModel.V1;
using V2 = Xtensive.Orm.Tests.Issues.IssueJira0735_WrongTypeIdsInValidateModeModel.V2;

namespace Xtensive.Orm.Tests.Issues
{
  public sealed class IssueJira0735_WrongTypeIdsInValidateMode
  {
    [Test]
    public void DefaultBehaviorTest()
    {
      using (var domain = Domain.Build(BuildInitialConfiguration())) { }

      var exception = Assert.Throws<SchemaSynchronizationException>(() => Domain.Build(BuildUpgradeConfiguration()));
      Assert.That(exception.ComparisonResult, Is.Null);
    }

    private DomainConfiguration BuildInitialConfiguration()
    {
      var configuration = DomainConfigurationFactory.Create();
      configuration.UpgradeMode = DomainUpgradeMode.Recreate;
      configuration.Types.Register(typeof (V1.MaterialLot));
      configuration.Types.Register(typeof (V1.CustomMaterialLot));
      return configuration;
    }

    private DomainConfiguration BuildUpgradeConfiguration()
    {
      var configuration = DomainConfigurationFactory.Create();
      configuration.UpgradeMode = DomainUpgradeMode.Validate;
      configuration.Types.Register(typeof (V1.MaterialLot));
      configuration.Types.Register(typeof (V2.CustomMaterialLot));
      return configuration;
    }
  }
}

namespace Xtensive.Orm.Tests.Issues.IssueJira0735_WrongTypeIdsInValidateModeModel
{
  namespace V1
  {
    [HierarchyRoot]
    public class MaterialLot : Entity
    {
      [Field, Key]
      public long Id { get; set; }

      [Field]
      public String LotNumber { get; set; }
    }

    public class CustomMaterialLot : MaterialLot
    {
      [Field]
      public String CustomField { get; set; }
    }
  }

  namespace V2
  {
    public class CustomMaterialLot : V1.MaterialLot
    {
      [Field]
      public String CustomField { get; set; }
    }
  }
}

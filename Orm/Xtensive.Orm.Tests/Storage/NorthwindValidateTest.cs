// Copyright (C) 2012 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2012.06.08

using NUnit.Framework;
using Xtensive.Orm.Tests.ObjectModel.NorthwindDO;

namespace Xtensive.Orm.Tests.Storage
{
  [TestFixture]
  public class NorthwindValidateTest
  {
    [Test]
    public void MainTest()
    {
      var initialDomain = BuildDomain(DomainUpgradeMode.Recreate);
      initialDomain.Dispose();
      var validatedDomain = BuildDomain(DomainUpgradeMode.Validate);
      validatedDomain.Dispose();
    }

    public Domain BuildDomain(DomainUpgradeMode mode)
    {
      var configuration = DomainConfigurationFactory.Create();
      configuration.UpgradeMode = mode;
      configuration.Types.Register(typeof (Supplier).Assembly, typeof (Supplier).Namespace);
      return Domain.Build(configuration);
    }
  }
}
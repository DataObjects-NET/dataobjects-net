// Copyright (C) 2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2010.06.21

using System;
using System.Linq;
using NUnit.Framework;

namespace Xtensive.Orm.Tests.Issues.InconsistentDefaultDateTimeValuesModel1
{
  [HierarchyRoot]
  public class MyEntity : Entity
  {
    [Key, Field]
    public long Id { get; private set; }
  }
}

namespace Xtensive.Orm.Tests.Issues.InconsistentDefaultDateTimeValuesModel2
{
  [HierarchyRoot]
  public class MyEntity : Entity
  {
    [Key, Field]
    public long Id { get; private set; }

    [Field]
    public DateTime Value { get; private set; }
  }
}

namespace Xtensive.Orm.Tests.Issues
{
  public class Issue0713_InconsistentDefaultDateTimeValues
  {
    [Test]
    public void MainTest()
    {
      var configuration1 = DomainConfigurationFactory.Create();
      configuration1.Types.Register(typeof(InconsistentDefaultDateTimeValuesModel1.MyEntity));
      configuration1.UpgradeMode = DomainUpgradeMode.Recreate;

      using (var domain1 = Domain.Build(configuration1))
      using (var session = domain1.OpenSession())
      using (var ts = session.OpenTransaction()) {
        new InconsistentDefaultDateTimeValuesModel1.MyEntity();
        ts.Complete();
      }

      var configuration2 = DomainConfigurationFactory.Create();
      configuration2.Types.Register(typeof(InconsistentDefaultDateTimeValuesModel2.MyEntity));
      configuration2.UpgradeMode = DomainUpgradeMode.Perform;

      using (var domain2 = Domain.Build(configuration2))
      using (var session = domain2.OpenSession())
      using (var ts = session.OpenTransaction()) {
        session.Query.All<InconsistentDefaultDateTimeValuesModel2.MyEntity>().First(entity => entity.Value==DateTime.MinValue);
        ts.Complete();
      }
    }
  }
}
// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Elena Vakhtina
// Created:    2008.11.26

using System;
using NUnit.Framework;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Tests.Issue0004_Model;

namespace Xtensive.Orm.Tests.Issue0004_Model
{
  [HierarchyRoot]
  public class User : Entity
  {
    [Field, Key]
    public int ID { get; private set; }

    [Field]
    public string Name { get; set; }

    [Field, Association(PairTo = "User")]
    public EntitySet<Notification> Notifications { get; set; }
  }

  [HierarchyRoot]
  public class Notification : Entity
  {
    [Field, Key]
    public int ID { get; private set; }

    [Field]
    public string Description { get; set; }

    [Field, Association(PairTo = "Notifications")]
    public User User { get; set; }
  }
}

namespace Xtensive.Orm.Tests.Issues
{
  public class Issue0004_PairAttributeMisusageIsNotHandled
  {
    [Test]
    public void DomainBuildTest()
    {
      var configuration = BuildConfiguration();
      Assert.DoesNotThrow(() => Domain.Build(configuration).Dispose());
    }

    [Test]
    public void DomainBuildAsyncTest()
    {
      var configuration = BuildConfiguration();
      Assert.DoesNotThrowAsync(async () =>  (await Domain.BuildAsync(configuration)).Dispose());
    }

    private static DomainConfiguration BuildConfiguration()
    {
      var config = DomainConfigurationFactory.Create();
      config.Types.Register(typeof(User));
      config.Types.Register(typeof(Notification));
      return config;
    }

  }
}



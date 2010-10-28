// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Elena Vakhtina
// Created:    2008.11.26

using System;
using System.Reflection;
using NUnit.Framework;
using Xtensive.Core;
using Xtensive.Testing;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Tests.Issue0004_Model;

namespace Xtensive.Orm.Tests.Issue0004_Model
{
  [Serializable]
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

  [Serializable]
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
  [Ignore("No more actual")]
  public class Issue0004_PairAttributeMisusageIsNotHandled : AutoBuildTest
  {
    protected override DomainConfiguration BuildConfiguration()
    {
      var config = base.BuildConfiguration();
      config.Types.Register(Assembly.GetExecutingAssembly(), typeof(User).Namespace);
      return config;
    }

    protected override Domain BuildDomain(DomainConfiguration configuration)
    {
      Domain result = null;
      AssertEx.Throws<DomainBuilderException>(() => result = base.BuildDomain(configuration));
      return result;
    }
  }
}



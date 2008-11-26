// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Elena Vakhtina
// Created:    2008.11.26

using System.Reflection;
using NUnit.Framework;
using Xtensive.Core;
using Xtensive.Core.Testing;
using Xtensive.Storage.Attributes;
using Xtensive.Storage.Configuration;
using Xtensive.Storage.Tests.Bug0004_Model;

namespace Xtensive.Storage.Tests.Bug0004_Model
{
  [HierarchyRoot(typeof(KeyGenerator), "ID")]
  public class User : Entity
  {
    [Field]
    public int ID { get; private set; }

    [Field]
    public string Name { get; set; }

    [Field(PairTo = "User")]
    public EntitySet<Notification> Notifications { get; set; }
  }

  [HierarchyRoot(typeof(KeyGenerator), "ID")]
  public class Notification : Entity
  {
    [Field]
    public int ID { get; private set; }

    [Field]
    public string Description { get; set; }

    [Field(PairTo = "Notifications")]
    public User User { get; set; }
  }
}

namespace Xtensive.Storage.Tests.BugReports
{
  public class Bug0004_PairAttributeMisusageIsNotHandled : AutoBuildTest
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
      AssertEx.Throws<AggregateException>(() => result = base.BuildDomain(configuration));
      return result;
    }
  }
}



// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Elena Vakhtina
// Created:    2008.11.26

using System.Reflection;
using Xtensive.Core;
using Xtensive.Core.Testing;
using Xtensive.Storage.Configuration;
using Xtensive.Storage.Tests.Issue0004_Model;

namespace Xtensive.Storage.Tests.Issue0004_Model
{
  [HierarchyRoot]
  public class User : Entity
  {
    [Field, Key]
    public int ID { get; private set; }

    [Field]
    public string Name { get; set; }

    [Field(PairTo = "User")]
    public EntitySet<Notification> Notifications { get; set; }
  }

  [HierarchyRoot]
  public class Notification : Entity
  {
    [Field, Key]
    public int ID { get; private set; }

    [Field]
    public string Description { get; set; }

    [Field(PairTo = "Notifications")]
    public User User { get; set; }
  }
}

namespace Xtensive.Storage.Tests.Issues
{
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



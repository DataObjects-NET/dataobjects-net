// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2009.12.16

using System.Configuration;
using NUnit.Framework;
using Xtensive.IoC;
using Xtensive.Orm.Configuration;
using ConfigurationSection=Xtensive.IoC.Configuration.ConfigurationSection;

namespace Xtensive.Orm.Tests.Storage.IoC
{
  #region Service containers for this test

  public class DomainServiceContainer : ProxyContainerBase
  {
    public DomainServiceContainer(object configuration, IServiceContainer parent)
      : base(configuration, parent)
    {
      var section = (ConfigurationSection) 
        ConfigurationManager.GetSection("NativeTest");
      RealContainer = ServiceContainer.Create(section, "domain");
    }
  }

  public class SessionServiceContainer : ProxyContainerBase
  {
    public SessionServiceContainer(object configuration, IServiceContainer parent)
      : base(configuration, parent)
    {
      var section = (ConfigurationSection) 
        ConfigurationManager.GetSection("NativeTest");
      RealContainer = ServiceContainer.Create(section, "session");
    }
  }

  #endregion

  [TestFixture]
  public class NativeTest : ServiceTestBase
  {
    protected override Domain BuildDomain(DomainConfiguration configuration)
    {
      configuration.ServiceContainerType = 
        typeof (DomainServiceContainer);
      configuration.Sessions.Add(new SessionConfiguration(WellKnown.Sessions.Default));
      configuration.Sessions.Default.ServiceContainerType = 
        typeof (SessionServiceContainer);
      return base.BuildDomain(configuration);
    }
  }
}
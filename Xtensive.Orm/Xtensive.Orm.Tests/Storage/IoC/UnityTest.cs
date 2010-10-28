// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2009.12.16

using System;
using System.Collections.Generic;
using System.Configuration;
using Microsoft.Practices.ObjectBuilder2;
using Microsoft.Practices.Unity;
using Microsoft.Practices.Unity.Configuration;
using NUnit.Framework;
using Xtensive.IoC;
using Xtensive.Orm.Configuration;

namespace Xtensive.Orm.Tests.Storage.IoC
{
  #region Service containers for this test

  public class UnityServiceContainer : ServiceContainerBase
  {
    public UnityContainer UnityContainer { get; private set; }

    protected override IEnumerable<object> HandleGetAll(Type serviceType)
    {
      return UnityContainer.ResolveAll(serviceType);
    }

    protected override object HandleGet(Type serviceType, string name)
    {
      try {
        return UnityContainer.Resolve(serviceType, name);
      }
      catch (ResolutionFailedException) {
        return null;
      }
    }

    public UnityServiceContainer(UnityContainer unityContainer, IServiceContainer parent)
      : base(parent)
    {
      UnityContainer = unityContainer;
    }
  }

  public class UnityDomainServiceContainer : UnityServiceContainer
  {
    private static UnityContainer GetUnityContainer()
    {
      var section = (UnityConfigurationSection) ConfigurationManager.GetSection("UnityTest");
      var container = new UnityContainer();
      section.Containers["domain"].Configure(container);
      return container;
    }

    public UnityDomainServiceContainer(object configuration, IServiceContainer parent)
      : base(GetUnityContainer(), parent)
    {
    }
  }

  public class UnitySessionServiceContainer : UnityServiceContainer
  {
    private static UnityContainer GetUnityContainer()
    {
      var section = (UnityConfigurationSection) ConfigurationManager.GetSection("UnityTest");
      var container = new UnityContainer();
      section.Containers["session"].Configure(container);
      return container;
    }

    public UnitySessionServiceContainer(object configuration, IServiceContainer parent)
      : base(GetUnityContainer(), parent)
    {
    }
  }

  #endregion

  [TestFixture]
  public class UnityTest : ServiceTestBase
  {
    protected override Domain BuildDomain(DomainConfiguration configuration)
    {
      configuration.ServiceContainerType = 
        typeof (UnityDomainServiceContainer);
      configuration.Sessions.Add(new SessionConfiguration(WellKnown.Sessions.Default));
      configuration.Sessions.Default.ServiceContainerType = 
        typeof (UnitySessionServiceContainer);
      return base.BuildDomain(configuration);
    }
  }
}
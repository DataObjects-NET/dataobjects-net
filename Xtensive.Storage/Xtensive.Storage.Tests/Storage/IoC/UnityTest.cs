// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2009.12.16

using System.Configuration;
using Microsoft.Practices.Unity;
using Microsoft.Practices.Unity.Configuration;
using NUnit.Framework;
using Xtensive.Storage.Configuration;


namespace Xtensive.Storage.Tests.Storage.IoC
{
  [TestFixture]
  public class UnityTest : ServiceTestBase
  {
    protected override Domain BuildDomain(DomainConfiguration configuration)
    {
      var domain = base.BuildDomain(configuration);

      // Setting domain-container
      var domainContainer = BuildContainer("domain");
      // TODO: Fix this
      // domain.Services.SetLocatorProvider(() => new Microsoft.Practices.Unity.ServiceLocatorAdapter.UnityServiceLocator(domainContainer));

      // Singing up to SessionOpen event to set and configure session-level container
      domain.SessionOpen += OnSessionOpen;
      return domain;
    }

    private void OnSessionOpen(object sender, SessionEventArgs e)
    {
      var sessionContainer = BuildContainer("session");
      // TODO: Fix this
      // e.Session.Services.SetLocatorProvider(() => new Microsoft.Practices.Unity.ServiceLocatorAdapter.UnityServiceLocator(sessionContainer));
    }

    private static UnityContainer BuildContainer(string containerName)
    {
      var section = (UnityConfigurationSection) ConfigurationManager.GetSection("UnityTest");
      var result = new UnityContainer();
      section.Containers[containerName].Configure(result);

      return result;
    }
  }
}
// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2009.12.16

using System.Configuration;
using NUnit.Framework;
using Xtensive.Core.IoC;
using Xtensive.Storage.Configuration;

namespace Xtensive.Storage.Tests.Storage.IoC
{
  [TestFixture]
  public class NativeTest : ServiceTestBase
  {
    protected override Domain BuildDomain(DomainConfiguration configuration)
    {
      var domain = base.BuildDomain(configuration);

      // Setting domain-container
      var domainContainer = BuildContainer("domain");
      domain.Services.SetLocatorProvider(() => new ServiceLocatorAdapter(domainContainer));

      // Singing up to SessionOpen event to set and configure session-level container
      domain.SessionOpen += OnSessionOpen;
      return domain;
    }

    private void OnSessionOpen(object sender, SessionEventArgs e)
    {
      var sessionContainer = BuildContainer("session");
      e.Session.Services.SetLocatorProvider(() => new ServiceLocatorAdapter(sessionContainer));
    }

    private static ServiceContainer BuildContainer(string containerName)
    {
      var section = (Core.IoC.Configuration.ConfigurationSection) ConfigurationManager.GetSection("NativeTest");
      var result = new ServiceContainer();
      result.Configure(section.Containers[containerName]);

      return result;
    }
  }
}
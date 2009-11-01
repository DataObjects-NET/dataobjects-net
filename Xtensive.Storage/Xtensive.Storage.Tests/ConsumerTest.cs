// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2007.08.29

using System.Reflection;
using NUnit.Framework;
using Xtensive.Storage.Configuration;

namespace Xtensive.Storage.Tests
{
  [TestFixture]
  public class ConsumerTest
  {
    [Test]
    public void SessionScopeTest()
    {
      DomainConfiguration domainConfiguration = new DomainConfiguration(@"memory://localhost/DefaultPlacement");
      domainConfiguration.Types.Register(Assembly.GetExecutingAssembly(), "Xtensive.Storage.Tests.Storage.Library");

      Domain domain = Domain.Build(domainConfiguration);

      SessionConfiguration sessionConfig = new SessionConfiguration();
      using (domain.OpenSession(sessionConfig)) {
        // Some actions with Entities
      }
    }
  }
}
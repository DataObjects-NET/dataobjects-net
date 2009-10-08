// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.07.31

using System.Configuration;
using Microsoft.Practices.Unity;
using Microsoft.Practices.Unity.Configuration;
using NUnit.Framework;
using System;
using Xtensive.Core.Disposing;
using Xtensive.Core.IoC;
using Xtensive.Core.Reflection;
using Xtensive.Storage.Configuration;
using Xtensive.Storage.Providers;

namespace Xtensive.Storage.Tests
{
  [TestFixture]
  public abstract class AutoBuildTest
  {
    private string protocolName;
    private StorageProtocol protocol;
    private DisposableSet disposables;
    private static UnityContainer container;

    protected ProviderInfo ProviderInfo { get; private set; }
    protected Domain Domain { get; private set; }
    
    [TestFixtureSetUp]
    public virtual void TestFixtureSetUp()
    {
      var config = BuildConfiguration();
      SelectProtocol(config);
      CheckRequirements();
      Domain = BuildDomain(config);
      if (Domain!=null)
        ProviderInfo = Domain.StorageProviderInfo;
    }

    [TestFixtureTearDown]
    public virtual void TestFixtureTearDown()
    {
      disposables.DisposeSafely();
      Domain.DisposeSafely();
    }

    protected virtual DomainConfiguration BuildConfiguration()
    {
      return DomainConfigurationFactory.Create();
    }

    protected virtual void CheckRequirements()
    {
    }

    protected void EnsureProtocolIs(StorageProtocol allowedProtocols)
    {
      if ((protocol & allowedProtocols)==0)
        throw new IgnoreException(
          string.Format("This test is not suitable for '{0}' protocol", protocolName));
    }

    protected void EnsureProtocolIsNot(StorageProtocol disallowedProtocols)
    {
      EnsureProtocolIs(~disallowedProtocols);
    }

    protected void CreateSessionAndTransaction()
    {
      try {
        disposables = new DisposableSet();
        var session = Session.Open(Domain);
        disposables.Add(session);
        var transaction = Transaction.Open(session);
        disposables.Add(transaction);
      }
      catch {
        disposables.DisposeSafely();
        disposables = null;
        throw;
      }
    }

    protected virtual Domain BuildDomain(DomainConfiguration configuration)
    {
      try {
        return Domain.Build(configuration);
      }
      catch (Exception e) {
        Log.Error(GetType().GetFullName());
        Log.Error(e);
        throw;
      }
    }

    private void SelectProtocol(DomainConfiguration config)
    {
      protocolName = config.ConnectionInfo.Protocol;
      switch (protocolName) {
      case WellKnown.Protocol.Memory:
        protocol = StorageProtocol.Memory;
        break;
      case WellKnown.Protocol.SqlServer:
        protocol = StorageProtocol.SqlServer;
        break;
      case WellKnown.Protocol.PostgreSql:
        protocol = StorageProtocol.PostgreSql;
        break;
      case WellKnown.Protocol.Oracle:
        protocol = StorageProtocol.Oracle;
        break;
      default:
        throw new ArgumentOutOfRangeException();
      }
    }

    static AutoBuildTest()
    {
      // Global logging configuration
      var section = (UnityConfigurationSection) ConfigurationManager.GetSection("Unity");
      container = new UnityContainer();
      section.Containers.Default.Configure(container);
      ServiceLocator.SetLocatorProvider(() => new Microsoft.Practices.Unity.ServiceLocatorAdapter.UnityServiceLocator(container));
    }
  }
}

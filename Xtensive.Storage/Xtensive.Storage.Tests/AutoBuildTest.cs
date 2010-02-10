// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.07.31

using Microsoft.Practices.Unity;
using NUnit.Framework;
using System;
using Xtensive.Core.Disposing;
using Xtensive.Core.Reflection;
using Xtensive.Storage.Configuration;
using Xtensive.Storage.Providers;

namespace Xtensive.Storage.Tests
{
  [TestFixture]
  public abstract class AutoBuildTest
  {
    private string protocolName;
    private StorageProvider provider;
    private DisposableSet disposables;
    private static UnityContainer container;

    protected ProviderInfo ProviderInfo { get; set; }
    protected Domain Domain { get; set; }
    
    [TestFixtureSetUp]
    public virtual void TestFixtureSetUp()
    {
      var config = BuildConfiguration();
      provider = StorageTestHelper.ParseProvider(config.ConnectionInfo.Provider);
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

    protected void EnsureProviderIs(StorageProvider allowedProviders)
    {
      StorageTestHelper.EnsureProviderIs(provider, allowedProviders);
    }

    protected void EnsureProviderIsNot(StorageProvider disallowedProviders)
    {
      StorageTestHelper.EnsureProviderIs(provider, ~disallowedProviders);
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
  }
}

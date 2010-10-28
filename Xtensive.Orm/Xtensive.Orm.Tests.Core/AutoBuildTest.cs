// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.07.31

using Microsoft.Practices.Unity;
using NUnit.Framework;
using System;
using Xtensive.Disposing;
using Xtensive.Core;
using Xtensive.Reflection;
using Xtensive.Orm.Configuration;
using Xtensive.Storage.Providers;

namespace Xtensive.Orm.Tests
{
  [TestFixture]
  public abstract class AutoBuildTest
  {
    private DisposableSet disposables;
    private static UnityContainer container;

    protected ProviderInfo ProviderInfo { get; set; }
    protected Domain Domain { get; set; }
    
    [TestFixtureSetUp]
    public virtual void TestFixtureSetUp()
    {
      CheckRequirements();
      var config = BuildConfiguration();
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

    protected void CreateSessionAndTransaction()
    {
      try {
        disposables = new DisposableSet();
        var session = Domain.OpenSession();
        disposables.Add(session);
        var transaction = session.OpenTransaction();
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

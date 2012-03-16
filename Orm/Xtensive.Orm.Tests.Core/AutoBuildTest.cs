// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.07.31

using System.Diagnostics;
using NUnit.Framework;
using System;
using Xtensive.Disposing;
using Xtensive.Core;
using Xtensive.Reflection;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Providers;

namespace Xtensive.Orm.Tests
{
  [TestFixture]
  public abstract class AutoBuildTest
  {
    private const string ErrorInTestFixtureSetup = "Error in TestFixtureSetUp:\r\n{0}";
    private DisposableSet disposables;

    protected ProviderInfo ProviderInfo { get; set; }
    protected Domain Domain { get; set; }
    
    [TestFixtureSetUp]
    public virtual void TestFixtureSetUp()
    {
      try {
        CheckRequirements();
        RebuildDomain();
      }
      catch (IgnoreException) {
        throw;
      }
      catch (Exception e) {
        Debug.WriteLine(ErrorInTestFixtureSetup.FormatWith(e));
        throw;
      }
    }

    [TestFixtureTearDown]
    public virtual void TestFixtureTearDown()
    {
      disposables.DisposeSafely(true);
      Domain.DisposeSafely(true);
    }

    protected virtual DomainConfiguration BuildConfiguration()
    {
      return DomainConfigurationFactory.Create();
    }

    protected void RebuildDomain()
    {
      disposables.DisposeSafely(true);
      Domain.DisposeSafely(true);

      var config = BuildConfiguration();
      Domain = BuildDomain(config);
      PopulateData();

      if (Domain!=null)
        ProviderInfo = Domain.StorageProviderInfo;
    }

    protected virtual void PopulateData()
    {
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

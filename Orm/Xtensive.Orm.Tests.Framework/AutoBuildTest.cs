// Copyright (C) 2008-2023 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Dmitri Maximov
// Created:    2008.07.31

using System.Diagnostics;
using NUnit.Framework;
using System;
using Xtensive.Core;
using Xtensive.Reflection;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Providers;

namespace Xtensive.Orm.Tests
{
  [TestFixture]
  public abstract class AutoBuildTest : HasConfigurationAccessTest
  {
    private const string ErrorInTestFixtureSetup = "Error in TestFixtureSetUp:\r\n{0}";
    private DisposableSet disposables;

    protected ProviderInfo ProviderInfo { get; set; }
    protected Domain Domain { get; set; }

    [OneTimeSetUp]
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
        Debug.WriteLine(ErrorInTestFixtureSetup, e);
        throw;
      }
    }

    [OneTimeTearDown]
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

    protected (Session, TransactionScope) CreateSessionAndTransaction()
    {
      try {
        disposables = new DisposableSet();
        var session = Domain.OpenSession();
        var transaction = session.OpenTransaction();
        _ = disposables.Add(session);
        _ = disposables.Add(transaction);
        return (session, transaction);
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
        TestLog.Error(GetType().GetFullName());
        TestLog.Error(e);
        throw;
      }
    }
  }
}

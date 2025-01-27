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
    private const string ErrorNotInitializedGlobalSession = "Set InitGlobalSession to true";

    private DisposableSet disposables;
    private (Session session, TransactionScope transaction) globalSessionAndTransaction;

    /// <summary>
    /// If set to <see langword="true"/>, global session and transaction will be opened
    /// to use one session accross all test methods.
    /// </summary>
    protected virtual bool InitGlobalSession => false;

    protected ProviderInfo ProviderInfo { get; set; }
    protected Domain Domain { get; set; }

    // Use these two for read-only tests only, don't change them, they are controlled by AutoBuildTest.
    // If there is need to change Session/Transactionscope or add/modify/remove entities
    // then open dedicated Session/TransactionScope within test
    protected Session GlobalSession => InitGlobalSession ? globalSessionAndTransaction.session : throw new Exception(ErrorNotInitializedGlobalSession);
    protected TransactionScope GlobalTransaction => InitGlobalSession ? globalSessionAndTransaction.transaction : throw new Exception(ErrorNotInitializedGlobalSession);

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
      if (Domain != null) {
        ProviderInfo = Domain.StorageProviderInfo;
        if (InitGlobalSession) {
          globalSessionAndTransaction = CreateSessionAndTransaction();
        }
      }
      else {
        ProviderInfo = StorageProviderInfo.Instance.Info;
      }
      PopulateData();
    }

    protected virtual void PopulateData()
    {
    }

    protected virtual void CheckRequirements()
    {
    }

    protected (Session, TransactionScope) CreateSessionAndTransaction()
    {
      var initDisposable = disposables is null;
      try {
        if (initDisposable)
          disposables = new DisposableSet();
        var session = Domain.OpenSession();
        var transaction = session.OpenTransaction();
        _ = disposables.Add(session);
        _ = disposables.Add(transaction);
        return (session, transaction);
      }
      catch {
        if (initDisposable) {
          disposables.DisposeSafely();
          disposables = null;
        }
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

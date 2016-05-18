// Copyright (C) 2016 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kulakov
// Created:    2016.03.18

using System;
using System.Diagnostics;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Tests.Storage.ReadOnlySupport.ReadOnlyStorageBaseModel;

namespace Xtensive.Orm.Tests.Storage.ReadOnlySupport.ReadOnlyStorageBaseModel
{
  [HierarchyRoot]
  public class TestEntity : Entity
  {
    [Field, Key]
    public long Id { get; set; }

    [Field(Length = 300)]
    public string TextData { get; set; }
  }
}

namespace Xtensive.Orm.Tests.Storage.ReadOnlySupport
{
  [TestFixture]
  public abstract class ReadOnlyStorageTestBase
  {
    private const string ReadOnlyUserName = "readonlydotest";
    private const string ReadOnlyUserPassword = "readonlydotest";

    [Test]
    public void DomainBuildTest()
    {
      BuildReadOnlyDomain();
      Assert.DoesNotThrow(()=>BuildReadOnlyDomain());
    }

    [Test]
    public void QueryTest()
    {
      Domain domain = null;
      Assert.DoesNotThrow(()=>domain = BuildReadOnlyDomain());
      Assert.That(domain, Is.Not.Null);
      using (domain)
      using (var session = domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var queryResult = session.Query.All<TestEntity>().Where(t => t.Id > 150).ToList();
        Assert.That(queryResult.Count, Is.GreaterThan(0));
      }
    }

    [Test]
    public void CreateTest()
    {
      Domain domain = null;
      Assert.DoesNotThrow(() => domain = BuildReadOnlyDomain());
      Assert.That(domain, Is.Not.Null);
      using (domain)
      using (var session = domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        Assert.Throws(GetExceptionType(), () => new TestEntity());
      }
    }

    [Test]
    public void UpdateTest()
    {
      Domain domain = null;
      Assert.DoesNotThrow(() => domain = BuildReadOnlyDomain());
      Assert.That(domain, Is.Not.Null);
      using (domain)
      using (var session = domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var queryResult = session.Query.All<TestEntity>().Where(t => t.Id > 150).ToList();
        Assert.That(queryResult.Count, Is.EqualTo(150));
        foreach (var testEntity in queryResult) {
          testEntity.TextData += 1;
        }
        Assert.Throws(GetExceptionType(), session.SaveChanges);
      }
    }

    [Test]
    public void DeleteTest()
    {
      Domain domain = null;
      Assert.DoesNotThrow(() => domain = BuildReadOnlyDomain());
      Assert.That(domain, Is.Not.Null);
      using (domain)
      using (var session = domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var queryResult = session.Query.All<TestEntity>().Where(t => t.Id > 150).ToList();
        Assert.That(queryResult.Count, Is.EqualTo(150));
        foreach (var testEntity in queryResult) {
          testEntity.Remove();
        }
        Assert.Throws(GetExceptionType(), session.SaveChanges);
      }
    }

    [TestFixtureSetUp]
    public void TestFixtureSetUp()
    {
      try {
        CheckRequirements();
        using (var domain = BuildInitialDomain())
          PopulateData(domain);
      }
      catch (IgnoreException) {
        throw;
      }
      catch (Exception e) {
        Debug.WriteLine("Error in TestFixureSetUp: {0}", e);
        throw;
      }
    }

    protected virtual Type GetExceptionType()
    {
      return typeof (StorageException);
    }

    protected virtual void CheckRequirements()
    {
      Require.ProviderIsNot(StorageProvider.Sqlite | StorageProvider.MySql | StorageProvider.Firebird);
    }

    protected virtual void PopulateData(Domain domain)
    {
      using (var session = domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        for (int i = 0; i < 300; i++)
          new TestEntity {TextData = i.ToString()};

        transaction.Complete();
      }
    }

    protected virtual DomainConfiguration BuildUpgradeConfiguration()
    {
      var configuration = DomainConfigurationFactory.Create();
      configuration.UpgradeMode = DomainUpgradeMode.Recreate;
      configuration.Types.Register(typeof (TestEntity));
      return configuration;
    }

    protected virtual DomainConfiguration BuildReadOnlyConfiguration()
    {
      var configuration = DomainConfigurationFactory.Create();
      configuration.ConnectionInfo = new ConnectionInfo(BuildReadOnlyConnectionString(configuration.ConnectionInfo));
      configuration.Types.Register(typeof(TestEntity));
      configuration.UpgradeMode = DomainUpgradeMode.Skip;
      return configuration;
    }

    private Domain BuildInitialDomain()
    {
      return BuildDomain(BuildUpgradeConfiguration());
    }

    private Domain BuildReadOnlyDomain()
    {
      return BuildDomain(BuildReadOnlyConfiguration());
    }

    private Domain BuildDomain(DomainConfiguration configuration)
    {
      return Domain.Build(configuration);
    }

    private string BuildReadOnlyConnectionString(ConnectionInfo connectionInfo)
    {
      var connectionStringTemplate = "{0}://{1}{2}/{3}{4}";
      var loginInfoTemplate = "{0}:{1}@";
      var hostTemplate = "{0}:{1}";
      var parameterTemplate = "{0}={1}";

      var protocol = connectionInfo.ConnectionUrl.Protocol;
      var loginInfo = string.Format(loginInfoTemplate, ReadOnlyUserName, ReadOnlyUserPassword);
      var server = (connectionInfo.ConnectionUrl.Port > 0)
        ? string.Format(hostTemplate, connectionInfo.ConnectionUrl.Host, connectionInfo.ConnectionUrl.Port)
        : connectionInfo.ConnectionUrl.Host;
      var database = connectionInfo.ConnectionUrl.Resource;
      var parameters = string.Empty;
      if (connectionInfo.ConnectionUrl.Params.Count > 0) {
        var stringBuilder = new StringBuilder("?");
        foreach (var parameter in connectionInfo.ConnectionUrl.Params) {
          stringBuilder.Append(string.Format(parameterTemplate, parameter.Key, parameter.Value));
        }
      }

      return string.Format(connectionStringTemplate, protocol, loginInfo, server, database, parameters);
    }
  }
}

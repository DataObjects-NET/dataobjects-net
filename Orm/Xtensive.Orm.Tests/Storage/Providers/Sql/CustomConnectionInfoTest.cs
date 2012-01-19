// Copyright (C) 2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2010.06.10

using System.Linq;
using NUnit.Framework;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Metadata;

namespace Xtensive.Orm.Tests.Storage.Providers.Sql
{
  [TestFixture]
  public class CustomConnectionInfoTest 
  {
    [Test]
    public void CombinedTest()
    {
      Require.ProviderIs(StorageProvider.SqlServer);
      var configuration = DomainConfiguration.Load(
        "AppConfigTest", "DomainWithCustomConnectionInfo");

      using (var domain = Domain.Build(configuration)) {
        using (var session = domain.OpenSession()) {
          Assert.IsNull(session.Configuration.ConnectionInfo);
        }
        using (var session = domain.OpenSession(configuration.Sessions["constr"])) {
          Assert.IsNotNull(session.Configuration.ConnectionInfo);
          Assert.IsNotNull(session.Configuration.ConnectionInfo.ConnectionString);
        }
        using (var session = domain.OpenSession(configuration.Sessions["conurl"])) {
          Assert.IsNotNull(session.Configuration.ConnectionInfo);
          Assert.IsNotNull(session.Configuration.ConnectionInfo.ConnectionUrl);
        }
      }
    }
  }
}
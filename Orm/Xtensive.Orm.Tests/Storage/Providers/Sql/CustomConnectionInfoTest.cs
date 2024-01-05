// Copyright (C) 2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2010.06.10

using NUnit.Framework;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Metadata;

namespace Xtensive.Orm.Tests.Storage.Providers.Sql
{
  [TestFixture]
  public class CustomConnectionInfoTest : HasConfigurationAccessTest
  {
    [Test]
    public void CombinedTest()
    {
      var domainConfiguration = LoadDomainConfiguration("AppConfigTest", "DomainWithCustomConnectionInfo");
      var session = domainConfiguration.Sessions["constr"];
      Assert.IsNotNull(session.ConnectionInfo);
      Assert.IsNotNull(session.ConnectionInfo.ConnectionString);
      session = domainConfiguration.Sessions["conurl"];
      Assert.IsNotNull(session.ConnectionInfo);
      Assert.IsNotNull(session.ConnectionInfo.ConnectionUrl);
    }
  }
}
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
      var domainConfiguration = DomainConfiguration.Load("AppConfigTest", "DomainWithCustomConnectionInfo");
      var session = domainConfiguration.Sessions["constr"];
      Assert.IsNotNull(session.ConnectionInfo);
      Assert.IsNotNull(session.ConnectionInfo.ConnectionString);
      session = domainConfiguration.Sessions["conurl"];
      Assert.IsNotNull(session.ConnectionInfo);
      Assert.IsNotNull(session.ConnectionInfo.ConnectionUrl);
    }
  }
}
// Copyright (C) 2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2010.06.08

using System;
using NUnit.Framework;
using Xtensive.Core.Testing;
using Xtensive.Storage.Configuration;

namespace Xtensive.Storage.Tests.Issues
{
  [TestFixture]
  public class Issue0689_WeirdExceptionWhenMissingConnectionInfo
  {
    [Test]
    public void MissingConnectionInfoInCodeTest()
    {
      var configuration = DomainConfigurationFactory.Create();
      configuration.ConnectionInfo = null;
      AssertEx.Throws<ArgumentNullException>(
        () => Domain.Build(configuration));
    }

    [Test]
    public void MissingConnectionInfoInAppConfigTest()
    {
      Require.ProviderIs(StorageProvider.SqlServer);
      AssertEx.Throws<ArgumentNullException>(
        () => Domain.Build(DomainConfiguration.Load("AppConfigTest", "DomainWithWrongConnectionInfo")));
    }
  }
}
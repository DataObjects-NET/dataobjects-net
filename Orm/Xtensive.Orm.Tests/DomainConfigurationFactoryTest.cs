// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2009.05.19

using System;
using NUnit.Framework;
using Xtensive.Orm.Building.Builders;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Providers;
using Xtensive.Sql;

namespace Xtensive.Orm.Tests
{
  [TestFixture]
  public class DomainConfigurationFactoryTest
  {
    // ReSharper disable LocalizableElement

    [Test]
    public void MainTest()
    {
      var urlConnectionInfo = DomainConfigurationFactory.Create().ConnectionInfo;
      Console.WriteLine("ConnectionURL: " + urlConnectionInfo);

      var stringConnectionInfo = DomainConfigurationFactory.Create(true).ConnectionInfo;
      Console.WriteLine("ConnectionString: " + stringConnectionInfo);

      var providerDescriptor = ProviderDescriptor.Get(urlConnectionInfo.Provider);
      var driverFactory = (SqlDriverFactory) Activator.CreateInstance(providerDescriptor.DriverFactory);
      var driver = driverFactory.GetDriver(urlConnectionInfo);
      var providerInfo = ProviderInfoBuilder.Build(urlConnectionInfo.Provider, driver);

      Console.WriteLine("Features: " + providerInfo.ProviderFeatures);
    }

    // ReSharper restore LocalizableElement
  }
}
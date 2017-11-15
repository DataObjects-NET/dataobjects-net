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
      Console.WriteLine("Is64BitProcess: {0}", Environment.Is64BitProcess);
      Console.WriteLine();

      var urlConnectionInfo = DomainConfigurationFactory.Create().ConnectionInfo;
      Console.WriteLine("ConnectionURL: {0}", urlConnectionInfo);
      Console.WriteLine();

      var stringConnectionInfo = DomainConfigurationFactory.CreateForConnectionStringTest().ConnectionInfo;
      Console.WriteLine("ConnectionString: {0}", stringConnectionInfo);
      Console.WriteLine();

      var driver = TestSqlDriver.Create(urlConnectionInfo);
      var providerInfo = ProviderInfoBuilder.Build(urlConnectionInfo.Provider, driver);

      Console.WriteLine("Features: {0}", providerInfo.ProviderFeatures);
    }

    // ReSharper restore LocalizableElement
  }
}
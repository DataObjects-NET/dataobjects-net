// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.12.17

using System;
using System.Linq;
using Xtensive.Core;
using Xtensive.Core.Reflection;

namespace Xtensive.Sql
{
  partial class SqlDriver
  {
    private const string DriverAssemblyFormat = "Xtensive.Sql.{0}";

    /// <summary>
    /// Creates the driver from the specified connection url.
    /// </summary>
    /// <param name="url">The connection url.</param>
    /// <returns>Created driver.</returns>
    public static SqlDriver Create(UrlInfo url)
    {
      ArgumentValidator.EnsureArgumentNotNull(url, "url");
      return BuildDriver(url);
    }

    /// <summary>
    /// Creates the driver from the specified connection url.
    /// </summary>
    /// <param name="url">The connection url.</param>
    /// <returns>Created driver.</returns>
    public static SqlDriver Create(string url)
    {
      ArgumentValidator.EnsureArgumentNotNullOrEmpty(url, "url");
      return BuildDriver(UrlInfo.Parse(url));
    }

    private static SqlDriver BuildDriver(UrlInfo url)
    {
      var assembly = AssemblyHelper.LoadExtensionAssembly(string.Format(DriverAssemblyFormat, url.Protocol));
      var factoryType = assembly.GetTypes()
        .Single(type => type.IsPublicNonAbstractInheritorOf(typeof (SqlDriverFactory)));
      var factory = (SqlDriverFactory) Activator.CreateInstance(factoryType);
      var driver = factory.CreateDriver(url);
      driver.Initialize();
      return driver;
    }
  }
}
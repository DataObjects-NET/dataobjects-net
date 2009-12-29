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
    /// Creates the driver from the specified connection URL.
    /// </summary>
    /// <param name="url">The connection url.</param>
    /// <returns>Created driver.</returns>
    public static SqlDriver Create(UrlInfo url)
    {
      ArgumentValidator.EnsureArgumentNotNull(url, "url");
      return BuildDriver(new ConnectionInfo(url));
    }

    /// <summary>
    /// Creates the driver from the specified connection URL.
    /// </summary>
    /// <param name="url">The connection url.</param>
    /// <returns>Created driver.</returns>
    public static SqlDriver Create(string url)
    {
      ArgumentValidator.EnsureArgumentNotNullOrEmpty(url, "url");
      return BuildDriver(new ConnectionInfo(UrlInfo.Parse(url)));
    }

    /// <summary>
    /// Creates the driver from the specified connection string and driver name.
    /// </summary>
    /// <param name="provider">The provider.</param>
    /// <param name="connectionString">The connection string.</param>
    /// <returns>Created driver.</returns>
    public static SqlDriver Create(string provider, string connectionString)
    {
      ArgumentValidator.EnsureArgumentNotNullOrEmpty(provider, "provider");
      ArgumentValidator.EnsureArgumentNotNullOrEmpty(connectionString, "connectionString");
      return BuildDriver(new ConnectionInfo(provider, connectionString));
    }

    /// <summary>
    /// Creates the driver from the specified connection string and driver name.
    /// </summary>
    /// <param name="connectionInfo">The connection info.</param>
    /// <returns>Created driver.</returns>
    public static SqlDriver Create(ConnectionInfo connectionInfo)
    {
      ArgumentValidator.EnsureArgumentNotNull(connectionInfo, "connectionInfo");
      return BuildDriver(connectionInfo);
    }

    private static SqlDriver BuildDriver(ConnectionInfo connectionInfo)
    {
      var assembly = AssemblyHelper.LoadExtensionAssembly(string.Format(DriverAssemblyFormat, connectionInfo.Provider));
      var factoryType = assembly.GetTypes()
        .Single(type => type.IsPublicNonAbstractInheritorOf(typeof (SqlDriverFactory)));
      var factory = (SqlDriverFactory) Activator.CreateInstance(factoryType);
      var driver = factory.CreateDriver(connectionInfo);
      driver.Initialize();
      return driver;
    }
  }
}
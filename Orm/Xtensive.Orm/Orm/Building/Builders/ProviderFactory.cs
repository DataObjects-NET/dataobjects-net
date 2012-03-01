// Copyright (C) 2012 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2012.03.01

using System;
using System.Linq;
using System.Reflection;
using Xtensive.Orm.Providers;
using Xtensive.Reflection;
using Xtensive.Sql;

namespace Xtensive.Orm.Building.Builders
{
  /// <summary>
  /// Provider search result.
  /// </summary>
  public sealed class ProviderFactory
  {
    private readonly Type handlerFactory;
    private readonly Type driverFactory;

    /// <summary>
    /// Creates <see cref="SqlDriverFactory"/> for this provider.
    /// </summary>
    /// <returns></returns>
    public SqlDriverFactory CreateDriverFactory()
    {
      return (SqlDriverFactory) Activator.CreateInstance(driverFactory);
    }

    /// <summary>
    /// Creates <see cref="Providers.HandlerFactory"/> for this provider.
    /// </summary>
    /// <returns></returns>
    public HandlerFactory CreateHandlerFactory()
    {
      return (HandlerFactory) Activator.CreateInstance(handlerFactory);
    }

    /// <summary>
    /// Gets <see cref="ProviderFactory"/> for the specified <paramref name="providerName"/>.
    /// </summary>
    /// <param name="providerName">Name of the storage provider.</param>
    /// <returns>Provider factory for <paramref name="providerName"/>.</returns>
    public static ProviderFactory Get(string providerName)
    {
      var providerAssembly = GetProviderAssembly(providerName);

      var entry = providerAssembly.GetTypes()
        .Where(type => type.IsPublicNonAbstractInheritorOf(typeof (HandlerFactory)))
        .Where(type => type.IsDefined(typeof (ProviderAttribute), false))
        .Select(type => new {HandlerFactory = type, Attribute = type.GetAttribute<ProviderAttribute>()})
        .SingleOrDefault(e => e.Attribute.Name==providerName);

      if (entry==null)
        throw new DomainBuilderException(string.Format(
          Strings.ExStorageProviderXIsNotFound, providerName));

      return new ProviderFactory(entry.Attribute.DriverFactory, entry.HandlerFactory);
    }

    private static Assembly GetProviderAssembly(string providerName)
    {
      const string assemblyFormat = "Xtensive.Orm.{0}";

      switch (providerName) {
      case WellKnown.Provider.SqlServer:
        return typeof (Providers.SqlServer.HandlerFactory).Assembly;
      case WellKnown.Provider.SqlServerCe:
      case WellKnown.Provider.PostgreSql:
      case WellKnown.Provider.Oracle:
      case WellKnown.Provider.Firebird:
      case WellKnown.Provider.MySql:
        return AssemblyHelper.LoadExtensionAssembly(string.Format(assemblyFormat, providerName));
      default:
        throw new NotSupportedException(
          string.Format(Strings.ExProviderXIsNotSupportedUseOneOfTheFollowingY, providerName, WellKnown.Provider.All));
      }
    }

    // Constructors

    private ProviderFactory(Type driverFactory, Type handlerFactory)
    {
      this.driverFactory = driverFactory;
      this.handlerFactory = handlerFactory;
    }
  }
}
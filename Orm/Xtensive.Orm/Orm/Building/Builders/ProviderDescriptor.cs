// Copyright (C) 2012 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2012.03.01

using System;
using System.Linq;
using System.Reflection;
using Xtensive.Core;
using Xtensive.Orm.Providers;
using Xtensive.Reflection;
using Xtensive.Sql;

namespace Xtensive.Orm.Building.Builders
{
  /// <summary>
  /// Provider descriptor.
  /// </summary>
  internal sealed class ProviderDescriptor
  {
    /// <summary>
    /// Gets <see cref="HandlerFactory"/> type.
    /// </summary>
    public Type HandlerFactory { get; private set; }

    /// <summary>
    /// Gets <see cref="SqlDriverFactory"/> type.
    /// </summary>
    public Type DriverFactory { get; private set; }

    /// <summary>
    /// Gets <see cref="ProviderDescriptor"/> for the specified <paramref name="providerName"/>.
    /// </summary>
    /// <param name="providerName">Name of the storage provider.</param>
    /// <returns>Provider factory for <paramref name="providerName"/>.</returns>
    public static ProviderDescriptor Get(string providerName)
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

      return new ProviderDescriptor(entry.Attribute.DriverFactory, entry.HandlerFactory);
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
      case WellKnown.Provider.Sqlite:
        return LoadExtensionAssembly(string.Format(assemblyFormat, providerName));
      default:
        throw new NotSupportedException(
          string.Format(Strings.ExProviderXIsNotSupportedUseOneOfTheFollowingY, providerName, WellKnown.Provider.All));
      }
    }

    private static Assembly LoadExtensionAssembly(string extensionAssemblyName)
    {
      var mainAssembly = typeof (ProviderDescriptor).Assembly;
      var mainAssemblyRef = mainAssembly.GetName();
      var extensionAssemblyFullName = mainAssemblyRef.FullName.Replace(mainAssemblyRef.Name, extensionAssemblyName);
      var extensionAssembly = Assembly.Load(extensionAssemblyFullName);
      var mainAssemblyVersion = GetInformationalVersion(mainAssembly);
      var extensionAssemblyVersion = GetInformationalVersion(extensionAssembly);
      if (mainAssemblyVersion!=extensionAssemblyVersion)
        throw new InvalidOperationException(string.Format(
          Strings.ExAssemblyVersionMismatchMainAssemblyXYExtensionsAssemblyAB,
          mainAssemblyRef.Name, mainAssemblyVersion,
          extensionAssemblyName, extensionAssemblyVersion));
      return extensionAssembly;
    }

    private static string GetInformationalVersion(Assembly assembly)
    {
      var attributes = assembly.GetCustomAttributes(typeof (AssemblyInformationalVersionAttribute), false);
      if (attributes.Length < 1)
        return null;
      return ((AssemblyInformationalVersionAttribute) attributes[0]).InformationalVersion;
    }

    // Constructors

    private ProviderDescriptor(Type driverFactory, Type handlerFactory)
    {
      DriverFactory = driverFactory;
      HandlerFactory = handlerFactory;
    }
  }
}
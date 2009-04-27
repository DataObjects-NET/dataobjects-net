// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Kofman
// Created:    2009.04.23

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Xtensive.Storage.Building.Internals;
using Xtensive.Storage.Configuration;
using Xtensive.Storage.Internals;
using Xtensive.Storage.Resources;

namespace Xtensive.Storage.Building.Builders
{
  /// <summary>
  /// Builds domain in extended modes.
  /// </summary>
  public static class ExtendedDomainBuilder
  {
    internal static void BuildForUpgrade(DomainConfiguration configuration, Action dataUpgrader)
    {
      DomainBuilder.BuildDomain(configuration,
        SchemaUpgradeMode.Upgrade, 
        dataUpgrader);
    }

    private static Domain BuildBlockUpgrade(DomainConfiguration configuration)
    {
      Domain systemDomain = BuildSystemDomain(configuration);
      using (systemDomain.OpenSession()) {
        using (Transaction.Open()) {
          SchemaVersionHelper.CheckSchemaVersionIsActual(configuration.Types);
        }
      }
      return DomainBuilder.BuildDomain(configuration,
        SchemaUpgradeMode.Validate,
        () => { });
    }
   
    private static Domain BuildRecreate(DomainConfiguration configuration)
    {
      return DomainBuilder.BuildDomain(configuration,
        SchemaUpgradeMode.Recreate,
        () => SchemaVersionHelper.SetInitialSchemaVersion(configuration.Types));
    }

    private static Domain BuildPerformStrict(DomainConfiguration configuration)
    {
      UpgradeData(configuration);
      return BuildBlockUpgrade(configuration);
    }

    private static Domain BuildSystemDomain(DomainConfiguration configuration)
    {
      DomainConfiguration domainConfiguration = (DomainConfiguration) configuration.Clone();      
      return DomainBuilder.BuildDomain(domainConfiguration, 
        SchemaUpgradeMode.Validate, 
        () => { },
        type => false);
    }

    /// <summary>
    /// Builds the new <see cref="Domain"/> according to specified configuration.
    /// </summary>
    /// <param name="configuration">The storage configuration.</param>
    /// <returns>Newly created <see cref="Domain"/>.</returns>
    /// <exception cref="ArgumentNullException">Parameter <paramref name="configuration"/> is null.</exception>
    /// <exception cref="DomainBuilderException">At least one error have been occurred 
    /// during storage building process.</exception>
    public static Domain Build(DomainConfiguration configuration)
    {
      switch (configuration.BuildMode) {
        case DomainBuildMode.Recreate: 
          return BuildRecreate(configuration);
        case DomainBuildMode.BlockUpgrade:
          return BuildBlockUpgrade(configuration);
        case DomainBuildMode.PerformStrict:
          return BuildPerformStrict(configuration);

        default:
          return BuildRecreate(configuration);
      }
    }

    public static void UpgradeData(DomainConfiguration domainConfiguration)
    {
      IEnumerable<Assembly> assemblies = AssemblyHelper.GetAssemblies(domainConfiguration.Types);
      Dictionary<Assembly, string> schemaVersions = SchemaVersionHelper.GetSchemaVersions(assemblies);

      // TODO: Sort assemblies topologically
      // TODO: Use upgraders from different assemblies, but same version together

      foreach (Assembly assembly in assemblies) {
        string assemblyVersion = assembly.GetName(false).Version.ToString();
        if (assemblyVersion==schemaVersions[assembly])
          continue;

        var upgraders = GetUpgraders(assembly);

        while (assemblyVersion!=schemaVersions[assembly]) {

          var schemaVersion = schemaVersions[assembly];
          IUpgrader upgrader = 
            upgraders.Where(u => u.CanUpgradeFrom(schemaVersion)).FirstOrDefault();
            
          if (upgrader==null)
            break;

          var configuration = (DomainConfiguration) domainConfiguration.Clone();
          upgrader.RegisterRecycledTypes(configuration.Types);

          BuildForUpgrade(configuration,() => ExecuteUpgrader(upgrader));
        }
      }
    }

    private static void ExecuteUpgrader(IUpgrader upgrader)
    {
      string schemaVersion = SchemaVersionAccessor.GetSchemaVersion(upgrader.AssemblyName);
      if (schemaVersion!=upgrader.SourceVersion)
        throw new InvalidOperationException(Strings.ExInvalidUpgraderVersion);
      upgrader.Upgrade();
      SchemaVersionAccessor.SetSchemaVersion(upgrader.AssemblyName, upgrader.ResultVersion);
    }

    private static IEnumerable<IUpgrader> GetUpgraders(Assembly assembly)
    {
      return assembly.GetTypes()
        .Where(type => (typeof (IUpgrader)).IsAssignableFrom(type))
        .Select(type => type.TypeInitializer.Invoke(null))
        .Cast<IUpgrader>();
    }
  }
}
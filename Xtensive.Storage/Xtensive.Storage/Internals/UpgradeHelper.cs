// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Kofman
// Created:    2009.04.08

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Xtensive.Core;
using Xtensive.Storage.Building.Builders;
using Xtensive.Storage.Building.Internals;
using Xtensive.Storage.Configuration;
using Xtensive.Storage.Resources;

namespace Xtensive.Storage.Internals
{
  internal class UpgradeHelper
  {
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

          AutoBuilder.BuildForUpgrade(configuration,() => ExecuteUpgrader(upgrader));
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
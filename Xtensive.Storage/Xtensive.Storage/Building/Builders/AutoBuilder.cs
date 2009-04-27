// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Kofman
// Created:    2009.04.23

using System;
using System.Linq;
using Xtensive.Storage.Configuration;
using Xtensive.Storage.Internals;

namespace Xtensive.Storage.Building.Builders
{
  
  public static class AutoBuilder
  {
    internal static void BuildForUpgrade(DomainConfiguration configuration, Action dataUpgrader)
    {
      DomainBuilder.Build(configuration, 
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
      return DomainBuilder.Build(configuration,
        SchemaUpgradeMode.Validate,
        () => { });
    }
   
    private static Domain BuildRecreate(DomainConfiguration configuration)
    {
      return DomainBuilder.Build(configuration,
        SchemaUpgradeMode.Recreate,
        () => SchemaVersionHelper.SetInitialSchemaVersion(configuration.Types));
    }  

    private static Domain BuildPerformStrict(DomainConfiguration configuration)
    {
      UpgradeHelper.UpgradeData(configuration);
      return BuildBlockUpgrade(configuration);
    }


    private static Domain BuildSystemDomain(DomainConfiguration configuration)
    {
      var systemDomainConfiguration = (DomainConfiguration) configuration.Clone();
      // systemDomainConfiguration.Types.Clear();
      return DomainBuilder.Build(systemDomainConfiguration, SchemaUpgradeMode.Validate, () => { });
    }        
    

    private static void BuildTypeIds()
    {
      BuildingContext context = BuildingContext.Current;
      TypeIdBuilder.BuildSystemTypeIds();
      bool systemTypesOnly = context.Model.Types.All(t => t.IsSystem);
      if (systemTypesOnly)
        return;
      Domain systemDomain = BuildSystemDomain(context.Configuration);
      using (systemDomain.OpenSession()) {
        using (var transactionScope = Transaction.Open()) {
          TypeIdBuilder.ReadTypeIds();
          transactionScope.Complete();
        }
      }
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

  }
}
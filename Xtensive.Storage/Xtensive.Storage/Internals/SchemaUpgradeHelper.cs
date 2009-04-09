// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Kofman
// Created:    2009.04.08

using System;
using System.Collections.Generic;
using System.Linq;
using Xtensive.Core;
using Xtensive.Storage.Configuration;
using Xtensive.Storage.Resources;
using Assembly=Xtensive.Storage.Metadata.Assembly;

namespace Xtensive.Storage.Internals
{
  internal class SchemaUpgradeHelper
  {
    private readonly DomainConfiguration domainConfiguratuion;
    private readonly IModelAssembliesManager modelAssembliesManager;

    public bool IsSchemaActual(DomainConfiguration configuration)
    {
      var modelAssemblies = modelAssembliesManager.GetModelAssemblies(configuration.Types);
      if (modelAssemblies.Count > 0) {

        // New configuration with system types only
        var systemDomainConfiguration = (DomainConfiguration) configuration.Clone();
        systemDomainConfiguration.Types.Clear(); 

        // Build domain to read schema versions from storage
        Domain systemDomain = Domain.Build(systemDomainConfiguration);
        using (systemDomain.OpenSystemSession()) {
          using (Transaction.Open()) {
            foreach (var modelAssembly in modelAssemblies) {
              string schemaVersion = GetSchemaVersion(modelAssembly.AssemblyName);
              if (schemaVersion!=modelAssembly.ModelVersion)
                return false;
            }
          }
        }
      }
      return true;
    }

    public void UpgradeSchema(DomainConfiguration configuration)
    {
      List<IModelAssembly> modelAssemblies = GetModelAssemblies();
      
      // TODO: Sort assemblies topologically
      // TODO: Use upgraders from different assemblies, but same version together
      
      var schemaVersions = GetSchemaVersions(modelAssemblies);

      foreach (IModelAssembly assembly in modelAssemblies) {
        if (assembly.ModelVersion==schemaVersions[assembly])
          continue;
        var upgraders = assembly.GetUpgraders();

        while (assembly.ModelVersion!=schemaVersions[assembly]) {
          var schemaVersion = schemaVersions[assembly];
          ISchemaUpgrader upgrader = GetSuitableUpgrader(assembly.AssemblyName, schemaVersion, upgraders);
          if (upgrader==null)
            break;
          UpgradeSchema(upgrader);
        }
      }
    }

    private void UpgradeSchema(ISchemaUpgrader upgrader)
    {
      var configuration = (DomainConfiguration) domainConfiguratuion.Clone();
      upgrader.RegisterRecycledTypes(configuration.Types);
      Domain domain = Domain.Build(configuration);
      using (domain.OpenSystemSession()) {
        using (var transactionScope = Transaction.Open()) {
          string schemaVersion = GetSchemaVersion(upgrader.AssemblyName);
          if (schemaVersion!=upgrader.SourceVersion)
            throw new InvalidOperationException(Strings.ExInvalidUpgraderVersion);
          upgrader.RunUpgradeScript();
          SetSchemaVersion(upgrader.AssemblyName, upgrader.ResultVersion);
          transactionScope.Complete();
        }
      }
    }

    private List<IModelAssembly> GetModelAssemblies()
    {
      return modelAssembliesManager.GetModelAssemblies(domainConfiguratuion.Types);
    }

    private static Dictionary<IModelAssembly, string> GetSchemaVersions(IEnumerable<IModelAssembly> assemblies)
    {
      var result = new Dictionary<IModelAssembly, string>();
      foreach (var assembly in assemblies)
        result[assembly] = GetSchemaVersion(assembly.AssemblyName);
      return result;
    }

    private static ISchemaUpgrader GetSuitableUpgrader(string assemblyName, string schemaVersion, IEnumerable<ISchemaUpgrader> upgraders)
    {
      return upgraders
        .Where(upgrader => 
          upgrader.AssemblyName==assemblyName &&
          upgrader.SourceVersion==schemaVersion)
        .First();
    }

    #region Access to meta objects

    private static void SetSchemaVersion(string assemblyName, string version)
    {
      Assembly assembly = GetAssemblyObject(assemblyName);
      if (assembly==null)
        assembly = new Assembly {AssemblyName = assemblyName, Version = version};
      assembly.Version = version;
    }

    private static string GetSchemaVersion(string assemblyName)
    {
      Assembly assembly = GetAssemblyObject(assemblyName);
      if (assembly==null)
        throw new InvalidOperationException("Schema version is not found.");
      return assembly.Version;
    }

    private static Assembly GetAssemblyObject(string assemblyName)
    {
      return Enumerable.Where(Query<Assembly>.All, a => a.AssemblyName==assemblyName).First();
    }

    #endregion

    private static IModelAssembliesManager CreateModelAssembliesManager(Type managerType)
    {
      ArgumentValidator.EnsureArgumentNotNull(managerType, "managerType");
      if (!typeof(IModelAssembliesManager).IsAssignableFrom(managerType))
        throw new InvalidOperationException(string.Format(
          Strings.ExTypeXDoesNotImplementYInterface, managerType, typeof(IModelAssembliesManager)));

      return (IModelAssembliesManager) managerType.TypeInitializer.Invoke(null);
    }


    // Constructors

    public SchemaUpgradeHelper(DomainConfiguration domainConfiguration)
    {
      ArgumentValidator.EnsureArgumentNotNull(domainConfiguration, "domainConfiguration");
      this.domainConfiguratuion = domainConfiguration;      
      this.modelAssembliesManager = 
        CreateModelAssembliesManager(domainConfiguration.ModelAssembliesManagerType);;
    }
  }
}
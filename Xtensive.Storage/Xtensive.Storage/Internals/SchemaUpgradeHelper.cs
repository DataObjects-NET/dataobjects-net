// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Kofman
// Created:    2009.04.08

using System;
using System.Collections.Generic;
using System.Linq;
using Xtensive.Core;
using Xtensive.Core.Tuples;
using Xtensive.Storage.Configuration;
using Xtensive.Storage.Resources;
using Assembly=Xtensive.Storage.Metadata.Assembly;

namespace Xtensive.Storage.Internals
{
  internal class SchemaUpgradeHelper
  {
    private readonly DomainConfiguration domainConfiguratuion;
    private readonly IModelAssembliesManager modelAssembliesManager;

    

    public static void CheckSchemaIsActual()
    {
      
    }

    public bool IsSchemaActual()
    {
      var modelAssemblies = modelAssembliesManager.GetModelAssemblies(domainConfiguratuion.Types);
      if (modelAssemblies.Count > 0) {

        // New configuration with system types only
        var configuration = (DomainConfiguration) domainConfiguratuion.Clone();
        configuration.Types.Clear(); 

        // Build domain to read schema versions from storage
        Domain domain = Domain.Build(configuration);
        using (domain.OpenSystemSession()) {
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

    public void UpgradeSchema()
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

    public static void SetInitialSchemaVersion()
    {
//      List<IModelAssembly> modelAssemblies = GetModelAssemblies();
//      foreach (var assembly in modelAssemblies) {
//        SetSchemaVersion(assembly.AssemblyName, assembly.ModelVersion);
//      }
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

    public static void SetSchemaVersion(string assemblyName, string version)
    {
      Assembly assembly = GetAssemblyObject(assemblyName);
      if (assembly==null)
        assembly = new Assembly (assemblyName) {Version = version};
      assembly.Version = version;
    }

    public static string GetSchemaVersion(string assemblyName)
    {
      Assembly assembly = GetAssemblyObject(assemblyName);  
      if (assembly==null)
        throw new InvalidOperationException("Schema version is not found.");
      return assembly.Version;
    }

    private static Assembly GetAssemblyObject(string assemblyName)
    {
      Key assemblyKey = Key.Create(typeof(Assembly), Tuple.Create(assemblyName));
      return assemblyKey.Resolve<Assembly>();
    }

    #endregion

    private static IModelAssembliesManager CreateModelAssembliesManager(Type managerType)
    {
      ArgumentValidator.EnsureArgumentNotNull(managerType, "managerType");
      if (!typeof(IModelAssembliesManager).IsAssignableFrom(managerType))
        throw new InvalidOperationException(string.Format(
          Strings.ExTypeXDoesNotImplementYInterface, managerType, typeof(IModelAssembliesManager)));

      return (IModelAssembliesManager) System.Activator.CreateInstance(managerType);
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
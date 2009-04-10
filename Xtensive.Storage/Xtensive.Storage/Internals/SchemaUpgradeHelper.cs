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
using Xtensive.Storage.Building.Builders;
using Xtensive.Storage.Configuration;
using Xtensive.Storage.Resources;
using Assembly=Xtensive.Storage.Metadata.Assembly;

namespace Xtensive.Storage.Internals
{
  internal class SchemaUpgradeHelper
  {
    private readonly DomainConfiguration domainConfiguratuion;
    private readonly IModelAssembliesManager modelAssembliesManager;

    public void CheckSchemaIsActual()
    {
      var modelAssemblies = modelAssembliesManager.GetModelAssemblies(domainConfiguratuion.Types);
      foreach (var assembly in modelAssemblies) {
        string schemaVersion = GetSchemaVersion(assembly.AssemblyName);
        string modelVersion = assembly.ModelVersion;
        if (schemaVersion!=modelVersion)
          throw new InvalidOperationException(string.Format(
            "Actual schema version of assembly '{0}' is expected to be '{1}', but currently is '{2}'.", 
            assembly.AssemblyName, modelVersion, schemaVersion));
      }
    }


    public Dictionary<IModelAssembly, string> ReadSchemaVersions(IEnumerable<IModelAssembly> assemblies)
    {
      DomainConfiguration configuration = (DomainConfiguration) domainConfiguratuion.Clone();
      configuration.Types.Clear();
      VersionsReader reader = new VersionsReader(assemblies);
      DomainBuilder.BuildForAccessMetadata(configuration, reader.ReadData);
      return reader.Versions;
    }

    private class VersionsReader
    {
      public Dictionary<IModelAssembly, string> Versions { get; private set;}

      private readonly IEnumerable<IModelAssembly> assemblies;

      public void ReadData()
      {
        Versions = GetSchemaVersions(assemblies);
      }

      public VersionsReader(IEnumerable<IModelAssembly> assemblies)
      {
        this.assemblies = assemblies;
      }
    }

    public void UpgradeData()
    {
      List<IModelAssembly> assemblies = GetModelAssemblies();
      var schemaVersions = ReadSchemaVersions(assemblies);

      // TODO: Sort assemblies topologically
      // TODO: Use upgraders from different assemblies, but same version together
      
      foreach (IModelAssembly assembly in assemblies) {
        if (assembly.ModelVersion==schemaVersions[assembly])
          continue;
        var upgraders = assembly.GetUpgraders();

        while (assembly.ModelVersion!=schemaVersions[assembly]) {
          var schemaVersion = schemaVersions[assembly];
          ISchemaUpgrader upgrader = GetSuitableUpgrader(assembly.AssemblyName, schemaVersion, upgraders);
          if (upgrader==null)
            break;

          var configuration = (DomainConfiguration) domainConfiguratuion.Clone();
          upgrader.RegisterRecycledTypes(configuration.Types);

          DomainBuilder.BuildForUpgrade(configuration,() => UpgradeData(upgrader));
        }
      }
    }

    private void UpgradeData(ISchemaUpgrader upgrader)
    {
      string schemaVersion = GetSchemaVersion(upgrader.AssemblyName);
      if (schemaVersion!=upgrader.SourceVersion)
        throw new InvalidOperationException(Strings.ExInvalidUpgraderVersion);
      upgrader.RunUpgradeScript();
      SetSchemaVersion(upgrader.AssemblyName, upgrader.ResultVersion);
    }

    public void SetInitialSchemaVersion()
    {
      var modelAssemblies = modelAssembliesManager.GetModelAssemblies(domainConfiguratuion.Types);
      foreach (var assembly in modelAssemblies) {
        SetSchemaVersion(assembly.AssemblyName, assembly.ModelVersion);
      }
    }

    private List<IModelAssembly> GetModelAssemblies()
    {
      return modelAssembliesManager.GetModelAssemblies(domainConfiguratuion.Types);
    }

    private Dictionary<IModelAssembly, string> GetSchemaVersions()
    {
      var assemblies = modelAssembliesManager.GetModelAssemblies(domainConfiguratuion.Types);
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

    private static Dictionary<IModelAssembly, string> GetSchemaVersions(IEnumerable<IModelAssembly> assemblies)
    {
      var result = new Dictionary<IModelAssembly, string>();
      foreach (var assembly in assemblies)
        result[assembly] = GetSchemaVersion(assembly.AssemblyName);
      return result;
    }

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
// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Kofman
// Created:    2009.04.08

using System;
using System.Linq;
using Xtensive.Storage.Building.Builders;
using Xtensive.Storage.Configuration;
using Xtensive.Storage.Configuration.TypeRegistry;
using Xtensive.Storage.Metadata;

namespace Xtensive.Storage
{
  /// <summary>
  /// Base class for schema upgraders - classes responsible for schema upgrade.
  /// </summary>
  public abstract class SchemaUpgrader
  {
    /// <summary>
    /// Gets the name of the assembly.
    /// </summary>
    protected abstract string AssemblyName { get; }

    /// <summary>
    /// Gets the source version, i.e. schema version this upgrader can upgrade from.
    /// </summary>
    protected abstract string SourceVersion { get; }

    /// <summary>
    /// Gets the result version, i.e. schema version this upgrader can upgrade to.
    /// </summary>
    protected abstract string ResultVersion { get; }

    /// <summary>
    /// Determines whether this instance can upgrade schema from specified version.
    /// </summary>
    /// <param name="assemblyName">Name of the assembly with model.</param>
    /// <param name="version">The version to upgrade schema from.</param>
    /// <returns>
    /// <see langword="true"/> if this upgrade is possible; otherwise, <see langword="false"/>.
    /// </returns>
    public bool CanUpgradeFrom(string assemblyName, string version)
    {
      return 
        AssemblyName==assemblyName && 
        SourceVersion==version;
    }

    /// <summary>
    /// Performs the schema upgrade.
    /// </summary>
    /// <param name="configuration">The configuration.</param>
    public void PerformUpgrade(DomainConfiguration configuration)
    {
      DomainConfiguration newConfiguration = (DomainConfiguration) configuration.Clone();      
      RegisterRecycledTypes(newConfiguration.Types);
      Domain domain = DomainBuilder.Build(newConfiguration);      

      using (domain.Handler.OpenSession(SessionType.System)) {
        using (var transactionScope = Transaction.Open()) {
          string schemaVersion = GetSchemaVersion(AssemblyName);
          if (schemaVersion!=SourceVersion)
            throw new InvalidOperationException("Invalid upgrader version");
          RunUpgradeScript();
          SetSchemaVersion(AssemblyName, ResultVersion);
          transactionScope.Complete();
        }
      }
    }

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
      return Query<Assembly>.All.Where(a => a.AssemblyName==assemblyName).First();
    }

    /// <summary>
    /// Runs the upgrade script.
    /// </summary>
    /// <remarks>
    /// Override this method to perform some actions when schema is upgrading.
    /// </remarks>
    protected virtual void RunUpgradeScript()
    {
    }

    /// <summary>
    /// Registers the recycled types.
    /// </summary>
    /// <param name="typeRegistry">The type registry to register types in.</param>
    /// <remarks>
    /// Override this method to register recycled classes required for schema upgrade.
    /// </remarks>
    protected virtual void RegisterRecycledTypes(Registry typeRegistry)
    {
    }
  }
}
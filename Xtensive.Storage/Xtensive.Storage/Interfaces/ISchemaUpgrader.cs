// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Kofman
// Created:    2009.04.08

using System;
using Xtensive.Storage.Building.Builders;
using Xtensive.Storage.Configuration;
using Xtensive.Storage.Configuration.TypeRegistry;
using Xtensive.Storage.Internals;

namespace Xtensive.Storage
{
  /// <summary>
  /// Base class for schema upgraders - classes responsible for schema upgrade.
  /// </summary>
  public interface ISchemaUpgrader
  {
    /// <summary>
    /// Gets the name of the assembly.
    /// </summary>
    string AssemblyName { get; }

    /// <summary>
    /// Gets the source version, i.e. schema version this upgrader can upgrade from.
    /// </summary>
    string SourceVersion { get; }

    /// <summary>
    /// Gets the result version, i.e. schema version this upgrader can upgrade to.
    /// </summary>
    string ResultVersion { get; }

    /// <summary>
    /// Runs the upgrade script.
    /// </summary>
    /// <remarks>
    /// Implement this method to perform some actions when schema is upgrading.
    /// </remarks>
    void RunUpgradeScript();

    /// <summary>
    /// Registers the recycled types.
    /// </summary>
    /// <param name="typeRegistry">The type registry to register types in.</param>
    /// <remarks>
    /// Implement this method to register recycled classes required for schema upgrade.
    /// </remarks>
    void RegisterRecycledTypes(Registry typeRegistry);
  }
}
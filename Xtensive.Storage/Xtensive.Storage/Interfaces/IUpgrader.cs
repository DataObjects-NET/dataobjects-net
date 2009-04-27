// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Kofman
// Created:    2009.04.08

using System;
using Xtensive.Storage.Building.Builders;
using Xtensive.Storage.Configuration;
using Xtensive.Storage.Configuration;
using Xtensive.Storage.Internals;

namespace Xtensive.Storage
{
  /// <summary>
  /// Base class for schema upgraders - classes responsible for schema upgrade.
  /// </summary>
  public interface IUpgrader
  {
    /// <summary>
    /// Gets the name of the assembly.
    /// </summary>
    string AssemblyName { get; }

    /// <summary>
    /// Determines whether this upgrade can upgrade schema from the specified version.
    /// </summary>
    /// <param name="schemaVersion">The schema version to upgrade from.</param>
    /// <returns>
    /// <see langword="true"/> if this upgrader can upgrade schema from the specified version; otherwise, <see langword="false"/>.
    /// </returns>
    bool CanUpgradeFrom(string schemaVersion);

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
    void Upgrade();

    /// <summary>
    /// Registers the recycled types.
    /// </summary>
    /// <param name="typeRegistry">The type registry to register types in.</param>
    /// <remarks>
    /// Implement this method to register recycled classes required for schema upgrade.
    /// </remarks>
    void RegisterRecycledTypes(TypeRegistry typeRegistry);
  }
}
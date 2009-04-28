// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Kofman
// Created:    2009.04.08

using System;

namespace Xtensive.Storage
{
  /// <summary>
  /// Schema upgrader - class responsible for schema upgrade.
  /// </summary>
  public interface IUpgrader
  {
    /// <summary>
    /// Gets the name of the assembly.
    /// </summary>
    string AssemblyName { get; }

    /// <summary>
    /// Determines whether this upgrader can upgrade schema from the specified version.
    /// </summary>
    /// <param name="schemaVersion">The schema version to upgrade from.</param>
    /// <returns>
    /// <see langword="true"/> if this upgrader can upgrade schema from the specified version; otherwise, <see langword="false"/>.
    /// </returns>
    bool CanUpgradeFrom(string schemaVersion);

    /// <summary>
    /// Gets the result version, i.e. schema version this upgrader can upgrade to.
    /// </summary>
    string ResultVersion { get; }

    /// <summary>
    /// Gets the schema upgrade hints.
    /// </summary>
    /// <param name="hints">The hint registry.</param>
    void GetRenameHints(HintRegistry hints);

    /// <summary>
    /// Perform some upgrade actions with the data.
    /// </summary>
    void ProcessData();

    /// <summary>
    /// Determines whether specified persistent type should be included in upgrade model or not.
    /// </summary>
    /// <param name="type">The type to filter.</param>
    /// <returns>
    /// <see langword="true" /> if type should be included in upgrade model, otherwise <see langword="false" />
    /// </returns>
    bool PersistentTypeFilter(Type type);
  }
}
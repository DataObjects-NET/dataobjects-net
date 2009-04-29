// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Kofman
// Created:    2009.04.08

using System;

namespace Xtensive.Storage.Upgrade
{
  /// <summary>
  /// Assembly upgrade handler contract.
  /// </summary>
  public interface IUpgrader
  {
    /// <summary>
    /// Gets the name of the assembly.
    /// </summary>
    /// <returns>Assembly name.</returns>
    string GetAssemblyName();

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
    /// Adds hints to current <see cref="UpgradeContext"/>.
    /// </summary>
    void OnBeforeUpgrade();
    
    /// <summary>
    /// Migrates data.
    /// </summary>
    void OnUpgrade();

    /// <summary>
    /// Determines whether specified persistent type should be included in upgrade model or not.
    /// </summary>
    /// <param name="type">The type to filter.</param>
    /// <returns>
    /// <see langword="true" /> if type should be included in upgrade model, otherwise <see langword="false" />
    /// </returns>
    bool IsAvailable(Type type);
  }
}
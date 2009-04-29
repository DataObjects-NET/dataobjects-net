// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Kofman
// Created:    2009.04.08

using System;

namespace Xtensive.Storage
{
  // TODO: -> Xtensive.Storage.Upgarde (+ move related public types available for user)
  // TODO: -> IAssemblyUpgradeHandler
  // TODO: Implement default (use if not provided)
  // TODO: Implement for Xtensive.Storage
  
  /// <summary>
  /// Assembly upgrade handler contract.
  /// </summary>
  public interface IUpgrader
  {
    // TODO: ->GetAssemblyName()
    // TODO: + string GetTypeName(Type type)
    // TODO: + int GetTypeId(Type type)
    // TODO: * Visibility GetTypeVisibility(Type type) (см. ниже)

    /// <summary>
    /// Gets the name of the assembly.
    /// </summary>
    string AssemblyName { get; }

    // TODO: -> schemaVersion -> assemblyVersion

    /// <summary>
    /// Determines whether this upgrader can upgrade schema from the specified version.
    /// </summary>
    /// <param name="schemaVersion">The schema version to upgrade from.</param>
    /// <returns>
    /// <see langword="true"/> if this upgrader can upgrade schema from the specified version; otherwise, <see langword="false"/>.
    /// </returns>
    bool CanUpgradeFrom(string schemaVersion);

    // TODO: -> GetAssemblyVersion()

    /// <summary>
    /// Gets the result version, i.e. schema version this upgrader can upgrade to.
    /// </summary>
    string ResultVersion { get; }
    
    // TODO: -> OnBeforeUpgrade() (adds hints to UpgradeContext.Current)
    // И забудь про RenameHint - че за детство? Со временемих будет дофига.
    
    /// <summary>
    /// Gets the schema upgrade hints.
    /// </summary>
    /// <param name="hints">The hint registry.</param>
    void GetRenameHints(HintRegistry hints);
    
    // TODO: -> OnUpgrade() (migrates data). Ахуеть у тебя название.
    
    /// <summary>
    /// Perform some upgrade actions with the data.
    /// </summary>
    void ProcessData();

    // TODO: -> Visibility GetTypeVisibility(Type type)
    // Visibility: enum [Flags]: Runtime = 1, UpgradeOnly = 2, Full = 3, System = 1+2+4
    
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
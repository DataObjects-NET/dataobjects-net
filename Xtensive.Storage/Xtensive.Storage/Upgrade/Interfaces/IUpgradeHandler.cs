// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Kofman
// Created:    2009.04.08

using System;
using System.Reflection;

namespace Xtensive.Storage.Upgrade
{
  /// <summary>
  /// A handler responsible for upgrading a specific assembly or its part.
  /// </summary>
  public interface IUpgradeHandler
  {
    /// <summary>
    /// Gets a value indicating whether this handler is enabled.
    /// </summary>
    bool IsEnabled { get; }

    /// <summary>
    /// Gets the assembly this handler is made for.
    /// </summary>
    Assembly Assembly { get; }

    /// <summary>
    /// Gets the name of the assembly described by this handler.
    /// </summary>
    string AssemblyName { get; }

    /// <summary>
    /// Gets the version of the assembly described by this handler.
    /// </summary>
    string AssemblyVersion { get; }
    
    /// <summary>
    /// Override this method to modify the <see cref="Storage.Upgrade.UpgradeContext.Configuration"/>
    /// before starting the next upgrade stage.
    /// </summary>
    void OnBeforeStage();

    /// <summary>
    /// Override this method to handle "at schema ready" event. 
    /// The both extracted schema and target schema are ready at this moment.
    /// </summary>
    void OnSchemaReady();

    /// <summary>
    /// Override this method to handle "at upgrade stage" event.
    /// </summary>
    void OnStage();

    /// <summary>
    /// Determines whether this handler can upgrade the assembly
    /// from the specified version of it.
    /// </summary>
    /// <param name="oldVersion">The old assembly version.</param>
    /// <returns>
    /// <see langword="true"/> if this instance can upgrade 
    /// from the specified version of an old assembly; 
    /// otherwise, <see langword="false"/>.
    /// </returns>
    bool CanUpgradeFrom(string oldVersion);

    /// <summary>
    /// Determines whether specified persistent type should be included into the model
    /// in the specified <paramref name="upgradeStage"/>, or not.
    /// </summary>
    /// <param name="type">The type to filter.</param>
    /// <param name="upgradeStage">The upgrade stage to check the availability at.</param>
    /// <returns>
    /// <see langword="true"/> if type should be included into the model in the specified upgrade stage;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    bool IsTypeAvailable(Type type, UpgradeStage upgradeStage);

    /// <summary>
    /// Determines whether specified persistent field (property) should be included into the model
    /// in the specified <paramref name="upgradeStage"/>, or not.
    /// </summary>
    /// <param name="field">The field to filter.</param>
    /// <param name="upgradeStage">The upgrade stage to check the availability at.</param>
    /// <returns>
    ///   <see langword="true"/> if type should be included into the model in the specified upgrade stage;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    bool IsFieldAvailable(PropertyInfo field, UpgradeStage upgradeStage);
  }
}
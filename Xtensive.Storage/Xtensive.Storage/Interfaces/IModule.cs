// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexander Nikolaev
// Created:    2009.07.02

using Xtensive.Storage.Building;
using Xtensive.Storage.Building.Definitions;

namespace Xtensive.Storage
{
  /// <summary>
  /// The contract of extension module.
  /// </summary>
  public interface IModule
  {
    /// <summary>
    /// Called when 'complex' build process is completed.
    /// </summary>
    /// <param name="domain">The built domain.</param>
    void OnBuilt(Domain domain);

    /// <summary>
    /// Called when the build of <see cref="DomainModelDef"/> is completed.
    /// </summary>
    /// <param name="context">The domain building context.</param>
    /// <param name="model">The domain model definition.</param>
    void OnDefinitionsBuilt(BuildingContext context, DomainModelDef model);
  }
}
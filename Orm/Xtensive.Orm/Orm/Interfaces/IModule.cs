// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexander Nikolaev
// Created:    2009.07.02

using Xtensive.Orm.Building;
using Xtensive.Orm.Building.Definitions;

namespace Xtensive.Orm
{
  /// <summary>
  /// <see cref="Domain"/>-level extension module contract.
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
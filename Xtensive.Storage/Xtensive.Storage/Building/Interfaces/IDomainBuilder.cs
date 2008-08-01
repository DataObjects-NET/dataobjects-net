// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2007.09.26

using Xtensive.Storage.Building.Definitions;

namespace Xtensive.Storage.Building
{
  /// <summary>
  /// Builds the <see cref="DomainModelDef"/> object.
  /// </summary>
  public interface IDomainBuilder
  {
    /// <summary>
    /// Builds the <see cref="DomainModelDef"/> object.
    /// </summary>
    /// <param name="context">The domain building context.</param>
    /// <param name="domain">The domain definition object.</param>
    void Build(BuildingContext context, DomainModelDef domain);
  }
}
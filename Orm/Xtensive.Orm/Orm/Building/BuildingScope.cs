// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2007.09.28

using Xtensive.Core;

using Xtensive.IoC;

namespace Xtensive.Orm.Building
{
  /// <summary>
  /// The scope for <see cref="BuildingContext"/>.
  /// </summary>
  public sealed class BuildingScope: Scope<BuildingContext>
  {
    /// <summary>
    /// Gets the context.
    /// </summary>
    public new static BuildingContext Context
    {
      get { return CurrentContext; }
    }


    // Constructors

    internal BuildingScope(BuildingContext buildingContext)
      : base(buildingContext)
    {
    }
  }
}
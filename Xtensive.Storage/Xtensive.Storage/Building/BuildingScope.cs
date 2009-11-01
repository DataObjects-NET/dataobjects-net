// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2007.09.28

using Xtensive.Core;

namespace Xtensive.Storage.Building
{
  public class BuildingScope: Scope<BuildingContext>
  {
    /// <summary>
    /// Gets the context.
    /// </summary>
    public new static BuildingContext Context
    {
      get { return CurrentContext; }
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="BuildingScope"/> class.
    /// </summary>
    /// <param name="buildingContext">The context.</param>
    public BuildingScope(BuildingContext buildingContext)
      : base(buildingContext)
    {
    }
  }
}
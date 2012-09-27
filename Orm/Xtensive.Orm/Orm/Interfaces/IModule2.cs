// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2012.09.27

using System;
using System.Collections.Generic;
using Xtensive.Orm.Building;

namespace Xtensive.Orm
{
  /// <summary>
  /// Extended version of <see cref="IModule"/>.
  /// Consider inheriting from <see cref="Module"/> instead.
  /// </summary>
  public interface IModule2 : IModule
  {
    /// <summary>
    /// Called when automatic generic instances are generated.
    /// </summary>
    /// <param name="context">Current <see cref="BuildingContext"/> instance.</param>
    /// <param name="autoGenerics">Automatic generic instances.</param>
    void OnAutoGenericsBuilt(BuildingContext context, ICollection<Type> autoGenerics);
  }
}
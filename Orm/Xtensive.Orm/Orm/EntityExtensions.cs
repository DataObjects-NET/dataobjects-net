// Copyright (C) 2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2010.03.04

using System;
using System.Diagnostics;

namespace Xtensive.Orm
{
  /// <summary>
  /// <see cref="Entity"/> related extension methods.
  /// </summary>
  public static class EntityExtensions
  {
    /// <summary>
    /// Determines whether the specified entity is removed by safely
    /// checking it for <see langword="null" /> and calling <see cref="IEntity.IsRemoved"/>.
    /// </summary>
    /// <param name="entity">The entity to check.</param>
    /// <returns>
    /// <see langword="true"/> if the specified entity is removed; 
    /// otherwise, <see langword="false"/>.
    /// </returns>
    public static bool IsRemoved(this IEntity entity)
    {
      return entity==null || entity.IsRemoved;
    }
  }
}
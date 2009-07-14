// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Kofman
// Created:    2009.06.22

using System;
using System.Collections.Generic;
using Xtensive.Storage.Model;
using Xtensive.Storage.Internals;
using Xtensive.Storage.Resources;

namespace Xtensive.Storage
{
  /// <summary>
  /// Contains referential-related methods.
  /// </summary>
  public static class ReferentialHelper
  {
    /// <summary>
    /// Finds entities that reference the specified entity.
    /// </summary>
    /// <param name="entity">The entity to find references to.</param>
    /// <returns>The set of found entities.</returns>
    public static IEnumerable<Entity> FindReferencingEntities(Entity entity)
    {
      foreach (AssociationInfo association in entity.Type.GetTargetAssociations())
        foreach (Entity item in association.FindReferencingObjects(entity))
          yield return item;
    }

    /// <summary>
    /// Finds entities that reference specified entity within specified <paramref name="association"/>.
    /// </summary>
    /// <param name="entity">The entity to find references to.</param>
    /// <param name="association">The association.</param>
    /// <returns>The set of found entities.</returns>
    /// <exception cref="InvalidOperationException">Type doesn't participate in the specified association.</exception>
    public static IEnumerable<Entity> FindReferencingEntities(Entity entity, AssociationInfo association)
    {
      if (!association.TargetType.UnderlyingType.IsAssignableFrom(entity.Type.UnderlyingType))
        throw new InvalidOperationException(
          string.Format(Strings.TypeXDoesNotParticipateInTheSpecifiedAssociation, entity.Type.Name));
      return association.FindReferencingObjects(entity);
    }

  }
}
// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2009.09.10

using System;
using System.Collections.Generic;
using System.Linq;
using Xtensive.Collections;
using Xtensive.Core;
using Xtensive.Orm.Model;
using Xtensive.Orm.Resources;

namespace Xtensive.Orm
{
  /// <summary>
  /// Provides a set of referential integrity related methods.
  /// </summary>
  public static class ReferenceFinder
  {
    #region Entity extension methods

    /// <summary>
    /// Finds the referencing objects.
    /// </summary>
    /// <param name="target">The target to find references to.</param>
    /// <returns>The sequence of <see cref="ReferenceInfo"/> objects.</returns>
    public static IEnumerable<ReferenceInfo> FindReferencingObjects(this Entity target)
    {
      return GetReferencesTo(target);
    }

    /// <summary>
    /// Finds the referencing objects.
    /// </summary>
    /// <param name="target">The target to find references to.</param>
    /// <param name="association">The association.</param>
    /// <returns>
    /// The sequence of <see cref="ReferenceInfo"/> objects.
    /// </returns>
    public static IEnumerable<ReferenceInfo> FindReferencingObjects(this Entity target, AssociationInfo association)
    {
      return GetReferencesTo(target, association);
    }

    /// <summary>
    /// Determines whether the specified <see cref="Entity"/> is referenced.
    /// </summary>
    /// <param name="target">The <see cref="Entity"/> to check.</param>
    /// <returns>
    /// <see langword="true"/> if the specified <see cref="Entity"/> is referenced; otherwise, <see langword="false"/>.
    /// </returns>
    public static bool IsReferenced(this Entity target)
    {
      return !GetReferencesTo(target).IsNullOrEmpty();
    }

    #endregion

    /// <summary>
    /// Finds all the entities that reference specified <paramref name="target"/> entity.
    /// </summary>
    /// <param name="target">The entity to find references to.</param>
    /// <returns>
    /// The sequence of <see cref="ReferenceInfo"/> objects.
    /// </returns>
    public static IEnumerable<ReferenceInfo> GetReferencesTo(Entity target)
    {
      var handler = target.Session.Handler;
      return target.TypeInfo.GetTargetAssociations().SelectMany(association => handler.GetReferencesTo(target, association));
    }

    /// <summary>
    /// Finds all the entities that reference <paramref name="target"/> entity 
    /// via specified <paramref name="association"/>.
    /// </summary>
    /// <param name="target">The entity to find references to.</param>
    /// <param name="association">The association to process.</param>
    /// <returns>
    /// The sequence of <see cref="ReferenceInfo"/> objects.
    /// </returns>
    /// <exception cref="InvalidOperationException">Type doesn't participate in the specified association.</exception>
    public static IEnumerable<ReferenceInfo> GetReferencesTo(Entity target, AssociationInfo association)
    {
      var handler = target.Session.Handler;
      if (!association.TargetType.UnderlyingType.IsAssignableFrom(target.TypeInfo.UnderlyingType))
        throw new InvalidOperationException(
          String.Format(Strings.TypeXDoesNotParticipateInTheSpecifiedAssociation, target.TypeInfo.Name));
      return handler.GetReferencesTo(target, association);
    }

    /// <summary>
    /// Gets all the references from the specified <paramref name="source"/> entity
    /// via specified <paramref name="association"/>.
    /// </summary>
    /// <param name="source">The source entity.</param>
    /// <param name="association">The association to process.</param>
    /// <returns>
    /// The sequence of <see cref="ReferenceInfo"/> objects.
    /// </returns>
    public static IEnumerable<ReferenceInfo> GetReferencesFrom(Entity source, AssociationInfo association)
    {
      var handler = source.Session.Handler;
      return handler.GetReferencesFrom(source, association);
    }
  }
}
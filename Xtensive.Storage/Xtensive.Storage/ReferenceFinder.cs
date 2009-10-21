// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2009.09.10

using System;
using System.Collections.Generic;
using System.Linq;
using Xtensive.Core.Collections;
using Xtensive.Core.Tuples;
using Xtensive.Storage.Model;
using Xtensive.Storage.Resources;
using Xtensive.Storage.Rse;

namespace Xtensive.Storage
{
  /// <summary>
  /// Contains referential-related methods.
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
      return FindReferencesTo(target);
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
      return FindReferencesTo(target, association);
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
      return !FindReferencesTo(target).IsNullOrEmpty();
    }

    #endregion

    /// <summary>
    /// Finds entities that reference this entity.
    /// </summary>
    /// <param name="target">The entity to find references to.</param>
    /// <returns>
    /// The sequence of <see cref="ReferenceInfo"/> objects.
    /// </returns>
    public static IEnumerable<ReferenceInfo> FindReferencesTo(Entity target)
    {
      var handler = Session.Demand().Handler;
      foreach (var association in target.Type.GetTargetAssociations())
        foreach (var item in handler.GetReferencesTo(target, association))
          yield return item;
    }

    /// <summary>
    /// Finds entities that reference this entity within specified <paramref name="association"/>.
    /// </summary>
    /// <param name="target">The entity to find references to.</param>
    /// <param name="association">The association.</param>
    /// <returns>
    /// The sequence of <see cref="ReferenceInfo"/> objects.
    /// </returns>
    /// <exception cref="InvalidOperationException">Type doesn't participate in the specified association.</exception>
    public static IEnumerable<ReferenceInfo> FindReferencesTo(Entity target, AssociationInfo association)
    {
      var handler = Session.Demand().Handler;
      if (!association.TargetType.UnderlyingType.IsAssignableFrom(target.Type.UnderlyingType))
        throw new InvalidOperationException(
          String.Format(Strings.TypeXDoesNotParticipateInTheSpecifiedAssociation, target.Type.Name));
      return handler.GetReferencesTo(target, association);
    }

    public static IEnumerable<ReferenceInfo> GetReferencesFrom(Entity target, AssociationInfo association)
    {
      var handler = Session.Demand().Handler;
      return handler.GetReferencesFrom(target, association);
    }
  }
}
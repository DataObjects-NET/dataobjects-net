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
      foreach (var association in target.Type.GetTargetAssociations())
        foreach (var item in GetReferencesTo(target, association))
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
      if (!association.TargetType.UnderlyingType.IsAssignableFrom(target.Type.UnderlyingType))
        throw new InvalidOperationException(
          String.Format(Strings.TypeXDoesNotParticipateInTheSpecifiedAssociation, target.Type.Name));
      return GetReferencesTo(target, association);
    }

    internal static IEnumerable<ReferenceInfo> GetReferencesTo(Entity target, AssociationInfo association)
    {
      IndexInfo index;
      Tuple keyTuple;
      RecordSet recordSet;

      switch (association.Multiplicity) {
        case Multiplicity.ZeroToOne:
        case Multiplicity.ManyToOne:
          index = association.OwnerType.Indexes.GetIndex(association.OwnerField.Name);
          keyTuple = target.Key.Value;
          recordSet = index.ToRecordSet().Range(keyTuple, keyTuple);
          foreach (var item in recordSet.ToEntities(0))
            yield return new ReferenceInfo(item, target, association);
          break;
        case Multiplicity.OneToOne:
        case Multiplicity.OneToMany:
          Key key = target.GetReferenceKey(association.Reversed.OwnerField);
          if (key!=null)
            yield return new ReferenceInfo(Query.SingleOrDefault(key), target, association);
          break;
        case Multiplicity.ZeroToMany:
        case Multiplicity.ManyToMany:
          if (association.IsMaster)
            index = association.AuxiliaryType.Indexes.Where(indexInfo => indexInfo.IsSecondary).First();
          else
            index = association.Master.AuxiliaryType.Indexes.Where(indexInfo => indexInfo.IsSecondary).Skip(1).First();
          keyTuple = target.Key.Value;
          recordSet = index.ToRecordSet().Range(keyTuple, keyTuple);
          foreach (var item in recordSet)
            yield return new ReferenceInfo(Query.SingleOrDefault(Key.Create(association.OwnerType, association.ExtractForeignKey(item))), target, association);
          break;
      }
    }

    internal static IEnumerable<ReferenceInfo> GetReferencesFrom(Entity owner, AssociationInfo association)
    {
      switch (association.Multiplicity) {
        case Multiplicity.ZeroToOne:
        case Multiplicity.OneToOne:
        case Multiplicity.ManyToOne:
          var target = owner.GetFieldValue<Entity>(association.OwnerField);
          if (target != null)
            yield return new ReferenceInfo(owner, target, association);
          break;
        case Multiplicity.ZeroToMany:
        case Multiplicity.OneToMany:
        case Multiplicity.ManyToMany:
          var targets = owner.GetFieldValue<EntitySetBase>(association.OwnerField);
          foreach (var item in targets.Entities)
            yield return new ReferenceInfo(owner, (Entity) item, association);
          break;
      }
    }
  }
}
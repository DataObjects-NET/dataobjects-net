// Copyright (C) 2014 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kulakov
// Created:    2014.09.17

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xtensive.Collections;

namespace Xtensive.Orm.Internals
{
  internal class EntityReferenceChangesRegistry : SessionBound
  {
    private object lockableObject = new object();

    /// <summary>
    /// For removed references and added references:
    /// TKey is entity which referenced;
    /// TValue is entities which referencing or no longer referencing to TKey.
    /// </summary>
    private readonly IDictionary<EntityState, IDictionary<EntityState, EntityState>> removedReferences = new Dictionary<EntityState, IDictionary<EntityState, EntityState>>();
    private readonly IDictionary<EntityState, IDictionary<EntityState, EntityState>> addedReferences = new Dictionary<EntityState, IDictionary<EntityState, EntityState>>();
    
    /// <summary>
    /// Checks that reference from <paramref name="entityStateForSearch"/> was removed.
    /// </summary>
    /// <param name="entityStateForSearch">State of entity for search.</param>
    /// <returns><see langword="false"/> if registry contains information about removed reference, otherwise, <see langword="true"/>.</returns>
    public bool HasReferenceFrom(EntityState target, EntityState entityStateForSearch)
    {
      IDictionary<EntityState, EntityState> foundEntities;
      if (!removedReferences.TryGetValue(target, out foundEntities))
        return true;
      var isExistsInRemoved = foundEntities.ContainsKey(entityStateForSearch);
      return !isExistsInRemoved;
    }

    /// <summary>
    /// Gets all states which no longer referencing to <paramref name="target"/>.
    /// </summary>
    /// <param name="target">Referenced state</param>
    /// <returns>Read-only list of states which no longer referencing to <paramref name="target"/>.</returns>
    public ReadOnlyDictionary<EntityState, EntityState> GetRemovedReferences(EntityState target)
    {
      IDictionary<EntityState, EntityState> removedReferences;
      if(this.removedReferences.TryGetValue(target, out removedReferences))
        return new ReadOnlyDictionary<EntityState, EntityState>(removedReferences);
      var dictionary = new Dictionary<EntityState, EntityState>();
      this.removedReferences.Add(target, dictionary);
      return new ReadOnlyDictionary<EntityState, EntityState>(dictionary);
    }

    /// <summary>
    /// Gets all states which add reference to <paramref name="target"/>.
    /// </summary>
    /// <param name="target">Referenced state.</param>
    /// <returns>Read-only list of states which add reference to <paramref name="target"/>.</returns>
    public ReadOnlyDictionary<EntityState, EntityState> GetAddedReferences(EntityState target)
    {
      IDictionary<EntityState, EntityState> addedReferences;
      if (this.addedReferences.TryGetValue(target, out addedReferences))
        return new ReadOnlyDictionary<EntityState, EntityState>(addedReferences);
      var dictionary = new Dictionary<EntityState, EntityState>();
      this.addedReferences.Add(target, dictionary);
      return new ReadOnlyDictionary<EntityState, EntityState>(dictionary);
    }

    /// <summary>
    /// Registers removing of reference between <paramref name="referencedEntityState"/> and <paramref name="referencedEntityState"/>.
    /// </summary>
    /// <param name="referencedEntityState">Referenced <see cref="EntityState"/>.</param>
    /// <param name="noLongerReferncingEntityState"><see cref="EntityState"/> which no longer referencing to <paramref name="referencedEntityState"/>.</param>
    public void RegisterRemovedReference(EntityState referencedEntityState, EntityState noLongerReferncingEntityState)
    {
      lock (lockableObject) {
        IDictionary<EntityState, EntityState> foundStates;
        if (addedReferences.TryGetValue(referencedEntityState, out foundStates))
          if (foundStates.ContainsKey(noLongerReferncingEntityState)) {
            foundStates.Remove(noLongerReferncingEntityState);
            return;
          }

        if (removedReferences.TryGetValue(referencedEntityState, out foundStates))
          foundStates.Add(noLongerReferncingEntityState, noLongerReferncingEntityState);
        else {
          var dictionary = new Dictionary<EntityState, EntityState>();
          dictionary.Add(noLongerReferncingEntityState, noLongerReferncingEntityState);
          removedReferences.Add(referencedEntityState, dictionary);
        }
      }
    }

    /// <summary>
    /// Registers addition of reference between <paramref name="referencedEntityState"/> and <paramref name="referencedEntityState"/>.
    /// </summary>
    /// <param name="referencedEntityState">Referenced <see cref="EntityState"/>.</param>
    /// <param name="newReferencingEntity">Referencing <see cref="EntityState"/>.</param>
    public void RegisterAddedReference(EntityState referencedEntityState, EntityState newReferencingEntity)
    {
      lock (lockableObject) {
        IDictionary<EntityState, EntityState> foundStates;
        if (removedReferences.TryGetValue(referencedEntityState, out foundStates))
          if (foundStates.ContainsKey(newReferencingEntity)) {
            foundStates.Remove(newReferencingEntity);
            return;
          }
      
        if (addedReferences.TryGetValue(referencedEntityState, out foundStates))
          foundStates.Add(newReferencingEntity,newReferencingEntity);
        else {
          var dictionary = new Dictionary<EntityState, EntityState>();
          dictionary.Add(newReferencingEntity, newReferencingEntity);
          addedReferences.Add(referencedEntityState, dictionary);
        }
      }
    }
    
    /// <summary>
    /// Clears registry.
    /// </summary>
    public void Clear()
    {
      lock (lockableObject) {
        removedReferences.Clear();
        addedReferences.Clear();
      }
    }

    internal EntityReferenceChangesRegistry(Session session)
      :base(session)
    {
    }
  }
}

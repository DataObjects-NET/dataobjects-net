// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.08.19

using System.Collections.Generic;
using System.Linq;
using Xtensive.Core;
using Xtensive.Core.Collections;
using Xtensive.Core.Sorting;
using Xtensive.Storage.Internals;
using Xtensive.Storage.Model;

namespace Xtensive.Storage.Providers
{
  partial class SessionHandler
  {
    protected bool PersistRequiresTopologicalSort;

    /// <summary>
    /// Persists changed entities.
    /// </summary>
    /// <param name="registry">The registry.</param>
    /// <param name="dirtyFlush">if set to <see langword="true"/> dirty flush is allowed.</param>
    public virtual void Persist(EntityChangeRegistry registry, bool dirtyFlush)
    {
      Persist(GetPersistSequence(registry), dirtyFlush);
    }

    /// <summary>
    /// Persists changed entities.
    /// </summary>
    /// <param name="persistActions">The entity states and the corresponding actions.</param>
    /// <param name="dirty">if set to <see langword="true"/> dirty flush is allowed.</param>
    public abstract void Persist(IEnumerable<PersistAction> persistActions, bool dirty);

    private IEnumerable<PersistAction> GetPersistSequence(EntityChangeRegistry registry)
    {
      // Insert
      var entityStates =
        from state in registry.GetItems(PersistenceState.New)
        where !state.IsNotAvailableOrMarkedAsRemoved
        select state;

      if (PersistRequiresTopologicalSort)
        foreach (var action in GetInsertSequence(entityStates))
          yield return action;
      else
        foreach (var state in entityStates)
          yield return new PersistAction(state, PersistActionKind.Insert);

      // Commit state differences, if any
      foreach (var state in entityStates)
        state.CommitDifference();

      // Update
      foreach (var state in registry.GetItems(PersistenceState.Modified)) {
        if (state.IsNotAvailableOrMarkedAsRemoved)
          continue;
        yield return new PersistAction(state, PersistActionKind.Update);
        state.DifferentialTuple.Merge();
      }

      // Delete
      entityStates =
        registry.GetItems(PersistenceState.Removed)
          .Except(registry.GetItems(PersistenceState.New));

      if (PersistRequiresTopologicalSort && entityStates.AtLeast(2))
        foreach (var action in GetDeleteSequence(entityStates))
          yield return action;
      else
        foreach (var state in entityStates)
          yield return new PersistAction(state, PersistActionKind.Remove);
    }

    private static IEnumerable<PersistAction> GetInsertSequence(IEnumerable<EntityState> entityStates)
    {
      // Topological sorting
      List<Triplet<EntityState, FieldInfo, Entity>> loopReferences;
      List<EntityState> sortedEntities;
      List<EntityState> unreferencedEntities;
      SortAndRemoveLoopEdges(entityStates, out sortedEntities, out unreferencedEntities, out loopReferences);

      // Insert 
      sortedEntities.Reverse();
      sortedEntities.AddRange(unreferencedEntities);

      foreach (var state in sortedEntities)
        yield return new PersistAction(state, PersistActionKind.Insert);

      // Restore loop links
      foreach (var restoreGroup in loopReferences.GroupBy(restoreData =>restoreData.First)) {
        var entityState = restoreGroup.Key;
        entityState.PersistenceState = PersistenceState.Synchronized;
        entityState.Entity.PrepareToSetField();
        foreach (var restoreData in restoreGroup)
          Persistent.GetAccessor<Entity>(restoreData.Second).SetValue(restoreData.First.Entity, restoreData.Second, restoreData.Third);
        yield return new PersistAction(entityState, PersistActionKind.Update);
      }
    }

    private static IEnumerable<PersistAction> GetDeleteSequence(IEnumerable<EntityState> entityStates)
    {
      // Rolling back the changes in state to properly sort it
      foreach (var state in entityStates)
        state.RollbackDifference();

      // Topological sorting
      List<Triplet<EntityState, FieldInfo, Entity>> loopReferences;
      List<EntityState> sortedEntities;
      List<EntityState> unreferencedEntities;
      SortAndRemoveLoopEdges(entityStates, out sortedEntities, out unreferencedEntities, out loopReferences);

      // Insert 
      sortedEntities.InsertRange(0, unreferencedEntities);

      // TODO: Group by entity
      // Restore loop links
      foreach (var restoreData in loopReferences) {
        // No necessity to call Entity.PrepareToSetField, since it already is
        Persistent.GetAccessor<Entity>(restoreData.Second).SetValue(restoreData.First.Entity, restoreData.Second, null);
        yield return new PersistAction(restoreData.First, PersistActionKind.Update);
      }

      foreach (var state in sortedEntities)
        yield return new PersistAction(state, PersistActionKind.Remove);
    }

    private static void SortAndRemoveLoopEdges(IEnumerable<EntityState> entityStates,
      out List<EntityState> sortResult, out List<EntityState> unreferencedData,
      out List<Triplet<EntityState, FieldInfo, Entity>> keysToRestore)
    {
      var sortData = new Dictionary<Key, Node<EntityState, AssociationInfo>>();
      unreferencedData = new List<EntityState>();
      foreach (var data in entityStates) {
        if (data.Type.GetTargetAssociations().Count==0 && data.Type.GetOwnerAssociations().Count==0)
          unreferencedData.Add(data);
        else
          sortData.Add(data.Key, new Node<EntityState, AssociationInfo>(data));
      }

      // Add connections
      foreach (var data in sortData) {
        var processingEntityState = data.Value.Item;
        foreach (var association in processingEntityState.Type.GetOwnerAssociations().Where(associationInfo => associationInfo.OwnerField.IsEntity)) {
          Key foreignKey = processingEntityState.Entity.GetReferenceKey(association.OwnerField);
          Node<EntityState, AssociationInfo> destination;
          if (foreignKey!=null && sortData.TryGetValue(foreignKey, out destination))
            if (foreignKey.Equals(data.Value.Item.Key) && processingEntityState.Entity.Type.Hierarchy.Schema == InheritanceSchema.ClassTable) {
              // Check if self-reference with inheritance.
              if (association.OwnerField.ValueType!=processingEntityState.Entity.Type.Hierarchy.Root.UnderlyingType)
                data.Value.AddConnection(destination, true, association);
            }
            else
              data.Value.AddConnection(destination, true, association);
        }
      }

      // Sort
      List<NodeConnection<EntityState, AssociationInfo>> removedEdges;
      sortResult = TopologicalSorter.Sort(sortData.Values, out removedEdges, true);

      // Remove loop links
      keysToRestore = new List<Triplet<EntityState, FieldInfo, Entity>>();
      foreach (var edge in removedEdges) {
        AssociationInfo associationInfo = edge.ConnectionItem;
        keysToRestore.Add(new Triplet<EntityState, FieldInfo, Entity>(edge.Source.Item, associationInfo.OwnerField, edge.Destination.Item.Entity));
        var entity = edge.Source.Item.Entity;
        entity.PrepareToSetField();
        Persistent.GetAccessor<Entity>(associationInfo.OwnerField).SetValue(entity, associationInfo.OwnerField, null);
      }
    }
  }
}
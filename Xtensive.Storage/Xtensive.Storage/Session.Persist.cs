// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2007.08.10

using System;
using System.Collections.Generic;
using System.Linq;
using Xtensive.Core;
using Xtensive.Core.Collections;
using Xtensive.Core.Sorting;
using Xtensive.Core.Tuples;
using Xtensive.Storage.Internals;
using Xtensive.Storage.Model;

namespace Xtensive.Storage
{
  public partial class Session
  {
    /// <summary>
    /// Persists all modified instances immediately.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This method should be called to ensure that all delayed
    /// updates are flushed to the storage. 
    /// </para>
    /// <para>
    /// Note, that this method is called automatically when it's necessary,
    /// e.g. before beginning, committing and rolling back a transaction, performing a
    /// query and so further. So generally you should not worry
    /// about calling this method.
    /// </para>
    /// </remarks>
    /// <exception cref="ObjectDisposedException">Session is already disposed.</exception>
    public void Persist()
    {
      if (IsPersisting)
        return;
      using (CoreServices.OpenSystemLogicOnlyRegion()) {
        IsPersisting = true;
        try {
          EnsureNotDisposed();

          if (EntityChangeRegistry.Count==0)
            return;

          if (IsDebugEventLoggingEnabled)
            Log.Debug("Session '{0}'. Persisting...", this);
          NotifyPersisting();

          try {
            Handler.Persist(GetPersistSequence());
          }
          finally {
            foreach (var item in EntityChangeRegistry.GetItems(PersistenceState.New))
              item.PersistenceState = PersistenceState.Synchronized;
            foreach (var item in EntityChangeRegistry.GetItems(PersistenceState.Modified))
              item.PersistenceState = PersistenceState.Synchronized;
            foreach (var item in EntityChangeRegistry.GetItems(PersistenceState.Removed))
              item.Update(null);
            EntityChangeRegistry.Clear();

            if (IsDebugEventLoggingEnabled)
              Log.Debug("Session '{0}'. Persisted.", this);

            NotifyPersist();
          }
        }
        finally {
          IsPersisting = false;
        }
      }
    }

    private IEnumerable<PersistAction> GetPersistSequence()
    {
      // Insert
      var entityStates =
        from state in EntityChangeRegistry.GetItems(PersistenceState.New)
        where !state.IsNotAvailableOrMarkedAsRemoved
        select state;

      if (persistRequiresTopologicalSort && entityStates.AtLeast(2))
        foreach (var action in GetInsertSequence(entityStates))
          yield return action;
      else
        foreach (var state in entityStates)
          yield return new PersistAction(state, PersistActionKind.Insert);

      // Merging the state into DifferentialTuple.Origin
      foreach (var state in entityStates)
        state.DifferentialTuple.Merge();

      // Update
      foreach (var state in EntityChangeRegistry.GetItems(PersistenceState.Modified)) {
        if (state.IsNotAvailableOrMarkedAsRemoved)
          continue;
        yield return new PersistAction(state, PersistActionKind.Update);
        state.DifferentialTuple.Merge();
      }

      // Delete
      entityStates = 
        EntityChangeRegistry.GetItems(PersistenceState.Removed)
        .Except(EntityChangeRegistry.GetItems(PersistenceState.New));

      if (persistRequiresTopologicalSort && entityStates.AtLeast(2))
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
      foreach (var restoreData in loopReferences) {
        Persistent.GetAccessor<Entity>(restoreData.Second).SetValue(restoreData.First.Entity, restoreData.Second, restoreData.Third);
        yield return new PersistAction(restoreData.First, PersistActionKind.Update);
      }
    }
    
    private static IEnumerable<PersistAction> GetDeleteSequence(IEnumerable<EntityState> entityStates)
    {
      // Rolling back the changes in state to properly sort it
      foreach (var state in entityStates) 
        state.ToOriginal();

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
          if (foreignKey!=null && !foreignKey.Equals(data.Value.Item.Key) && sortData.TryGetValue(foreignKey, out destination))
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
        Persistent.GetAccessor<Entity>(associationInfo.OwnerField).SetValue(edge.Source.Item.Entity, associationInfo.OwnerField, null);
      }
    }
  }
}
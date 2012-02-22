// Copyright (C) 2012 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2012.02.22

using System.Collections.Generic;
using System.Linq;
using Xtensive.Core;
using Xtensive.Orm.Model;
using Xtensive.Sorting;

namespace Xtensive.Orm.Internals
{
  internal class SortingPersistActionGenerator : PersistActionGenerator
  {
    protected override IEnumerable<PersistAction> GetInsertSequence(IEnumerable<EntityState> entityStates)
    {

      // Topological sorting
      List<Triplet<EntityState, FieldInfo, Entity>> refsToRestore;
      List<EntityState> sortedStates;
      List<EntityState> unreferencedStates;
      SortAndRemoveLoopEdges(entityStates, out sortedStates, out unreferencedStates, out refsToRestore);

      // Insert 
      sortedStates.Reverse();
      sortedStates.AddRange(unreferencedStates);

      foreach (var state in sortedStates)
        yield return new PersistAction(state, PersistActionKind.Insert);

      // Restore loop links
      foreach (var tripletGroup in refsToRestore.GroupBy(restoreData =>restoreData.First)) {
        var entityState = tripletGroup.Key;
        entityState.PersistenceState = PersistenceState.Synchronized;
        entityState.Entity.SystemBeforeTupleChange();
        foreach (var triplet in tripletGroup) {
          var entity = triplet.First.Entity;
          entity.GetFieldAccessor(triplet.Second)
            .SetUntypedValue(entity, triplet.Third);
        }
        yield return new PersistAction(entityState, PersistActionKind.Update);
      }
    }

    protected override IEnumerable<PersistAction> GetDeleteSequence(IEnumerable<EntityState> entityStates)
    {
      // Rolling back the changes in state to properly sort it
      foreach (var state in entityStates)
        state.RollbackDifference();

      // Topological sorting
      List<Triplet<EntityState, FieldInfo, Entity>> refsToRestore;
      List<EntityState> sortedStates;
      List<EntityState> unreferencedStates;
      SortAndRemoveLoopEdges(entityStates, out sortedStates, out unreferencedStates, out refsToRestore);

      // Insert 
      sortedStates.InsertRange(0, unreferencedStates);

      // TODO: Group by entity
      // Restore loop links
      foreach (var triplet in refsToRestore) {
        // No necessity to call Entity.SystemBeforeTupleChange, since it already is
        var entity = triplet.First.Entity;
        entity.GetFieldAccessor(triplet.Second)
          .SetUntypedValue(entity, null);
        yield return new PersistAction(triplet.First, PersistActionKind.Update);
      }

      foreach (var state in sortedStates)
        yield return new PersistAction(state, PersistActionKind.Remove);
    }

    private void SortAndRemoveLoopEdges(IEnumerable<EntityState> entityStates,
      out List<EntityState> sortedStates, 
      out List<EntityState> unreferencedStates,
      out List<Triplet<EntityState, FieldInfo, Entity>> refsToRestore)
    {
      var nodes = new Dictionary<Key, Node<EntityState, AssociationInfo>>();
      unreferencedStates = new List<EntityState>();
      foreach (var entityState in entityStates) {
        if (entityState.Type.GetTargetAssociations().Count==0 && entityState.Type.GetOwnerAssociations().Count==0)
          unreferencedStates.Add(entityState);
        else
          nodes.Add(entityState.Key, new Node<EntityState, AssociationInfo>(entityState));
      }

      // Add connections
      foreach (var pair in nodes) {
        var key = pair.Key;
        var node = pair.Value;
        var entityState = node.Item;
        var references =
          from association in entityState.Type.GetOwnerAssociations()
          where association.OwnerField.IsEntity
          select association;

        foreach (var association in references) {
          var ownerField = association.OwnerField;
          var targetKey = entityState.Entity.GetReferenceKey(ownerField);
          Node<EntityState, AssociationInfo> destination;
          if (targetKey!=null && nodes.TryGetValue(targetKey, out destination)) {
            var hierarchy = entityState.Entity.TypeInfo.Hierarchy;
            // If there is self-referencing field of hierarchy root type (important!),
            // we consider there is no dependency, since such insert sequence will pass
            // without any modifications
            if (targetKey.Equals(key))
              if (hierarchy.InheritanceSchema!=InheritanceSchema.ClassTable || ownerField.ValueType==hierarchy.Root.UnderlyingType)
                continue;
            node.AddConnection(destination, association);
          }
        }
      }

      // Sort
      List<NodeConnection<EntityState, AssociationInfo>> removedEdges;
      sortedStates = TopologicalSorter.Sort(nodes.Values, out removedEdges, true);

      // Remove loop links
      refsToRestore = new List<Triplet<EntityState, FieldInfo, Entity>>();
      foreach (var edge in removedEdges) {
        AssociationInfo associationInfo = edge.ConnectionItem;
        refsToRestore.Add(new Triplet<EntityState, FieldInfo, Entity>(edge.Source.Item, associationInfo.OwnerField, edge.Destination.Item.Entity));
        var entity = edge.Source.Item.Entity;
        entity.SystemBeforeTupleChange();
        entity.GetFieldAccessor(associationInfo.OwnerField)
          .SetUntypedValue(entity, null);
      }
    }
  }
}
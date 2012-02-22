// Copyright (C) 2012 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2012.02.22

using System.Collections.Generic;
using System.Linq;
using Xtensive.Core;
using Xtensive.Graphs;
using Xtensive.Orm.Model;

namespace Xtensive.Orm.Internals
{
  internal class SortingPersistActionGenerator : PersistActionGenerator
  {
    private List<Node<EntityState>> sortedNodes;
    private List<EntityState> unreferencedStates;
    private List<Triplet<EntityState, FieldInfo, Entity>> referencesToRestore;

    protected override IEnumerable<PersistAction> GetInsertSequence(IEnumerable<EntityState> entityStates)
    {
      // Topological sorting
      SortAndRemoveLoopEdges(entityStates, false);

      // Insert
      for (int i = sortedNodes.Count - 1; i >= 0; i--)
        yield return new PersistAction(sortedNodes[i].Value, PersistActionKind.Insert);

      foreach (var state in unreferencedStates)
        yield return new PersistAction(state, PersistActionKind.Insert);

      // Restore loop links
      foreach (var tripletGroup in referencesToRestore.GroupBy(restoreData => restoreData.First)) {
        var state = tripletGroup.Key;
        state.PersistenceState = PersistenceState.Synchronized;
        state.Entity.SystemBeforeTupleChange();
        foreach (var triplet in tripletGroup) {
          var entity = triplet.First.Entity;
          entity
            .GetFieldAccessor(triplet.Second)
            .SetUntypedValue(entity, triplet.Third);
        }
        yield return new PersistAction(state, PersistActionKind.Update);
      }
    }

    protected override IEnumerable<PersistAction> GetDeleteSequence(IEnumerable<EntityState> entityStates)
    {
      // Topological sorting
      SortAndRemoveLoopEdges(entityStates, true);

      // Restore loop links
      foreach (var triplet in referencesToRestore) {
        // No necessity to call Entity.SystemBeforeTupleChange, since it already is
        var entity = triplet.First.Entity;
        entity
          .GetFieldAccessor(triplet.Second)
          .SetUntypedValue(entity, null);
        yield return new PersistAction(triplet.First, PersistActionKind.Update);
      }

      foreach (var state in unreferencedStates)
        yield return new PersistAction(state, PersistActionKind.Remove);

      foreach (var node in sortedNodes)
        yield return new PersistAction(node.Value, PersistActionKind.Remove);
    }

    private void SortAndRemoveLoopEdges(IEnumerable<EntityState> entityStates, bool rollbackDifferenceBeforeSort)
    {
      var nodeIndex = new Dictionary<Key, Node<EntityState>>();
      var graph = new Graph<Node<EntityState>, Edge<AssociationInfo>>();

      unreferencedStates = new List<EntityState>();

      foreach (var state in entityStates) {
        if (rollbackDifferenceBeforeSort)
          state.RollbackDifference();
        if (state.Type.GetTargetAssociations().Count==0 && state.Type.GetOwnerAssociations().Count==0)
          unreferencedStates.Add(state);
        else {
          var node = new Node<EntityState>(state);
          graph.Nodes.Add(node);
          nodeIndex.Add(state.Key, node);
        }
      }

      // Add connections
      foreach (var ownerNode in graph.Nodes) {
        var owner = ownerNode.Value;
        var ownerKey = owner.Key;

        var references =
          from association in owner.Type.GetOwnerAssociations()
          where association.OwnerField.IsEntity
          select association;

        foreach (var association in references) {
          var ownerField = association.OwnerField;
          var targetKey = owner.Entity.GetReferenceKey(ownerField);
          Node<EntityState> targetNode;
          if (targetKey==null || !nodeIndex.TryGetValue(targetKey, out targetNode))
            continue;
          var hierarchy = owner.Entity.TypeInfo.Hierarchy;
          // If there is self-referencing field of hierarchy root type (important!),
          // we consider there is no dependency, since such insert sequence will pass
          // without any modifications
          var skipEdge =
            targetKey.Equals(ownerKey)
              && (hierarchy.InheritanceSchema!=InheritanceSchema.ClassTable
                || ownerField.ValueType==hierarchy.Root.UnderlyingType);

          if (skipEdge)
            continue;

          new Edge<AssociationInfo>(ownerNode, targetNode, association).Attach();
        }
      }

      // Sort
      var result = TopologicalSorter.Sort(graph, _ => true);
      sortedNodes = result.SortedNodes;

      // Remove loop links
      referencesToRestore = new List<Triplet<EntityState, FieldInfo, Entity>>();
      foreach (var edge in result.BrokenEdges) {
        var associationInfo = edge.Value;
        var owner = (EntityState) edge.Source.UntypedValue;
        var target = (EntityState) edge.Target.UntypedValue;

        referencesToRestore.Add(new Triplet<EntityState, FieldInfo, Entity>(
          owner, associationInfo.OwnerField, target.Entity));

        var entity = owner.Entity;
        entity.SystemBeforeTupleChange();
        entity
          .GetFieldAccessor(associationInfo.OwnerField)
          .SetUntypedValue(entity, null);
      }
    }
  }
}
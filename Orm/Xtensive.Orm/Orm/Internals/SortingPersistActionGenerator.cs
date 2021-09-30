// Copyright (C) 2012 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2012.02.22

using System;
using System.Collections.Generic;
using System.Linq;
using Xtensive.Collections.Graphs;
using Xtensive.Core;
using Xtensive.Orm.Model;
using Xtensive.Orm.Providers;

namespace Xtensive.Orm.Internals
{
  internal class SortingPersistActionGenerator : PersistActionGenerator
  {
    private List<Node<EntityState>> sortedNodes;
    private List<EntityState> inboundOnlyStates;
    private List<EntityState> outboundOnlyStates;
    private List<(EntityState, FieldInfo, Entity)> referencesToRestore;
    private readonly bool selfReferencingRowRemovalIsError;

    protected override IEnumerable<PersistAction> GetInsertSequence(IEnumerable<EntityState> entityStates)
    {
      // Topological sorting
      SortAndRemoveLoopEdges(entityStates, false);

      // Insert entities that do not reference anything
      foreach (var state in inboundOnlyStates)
        yield return new PersistAction(Node, state, PersistActionKind.Insert);

      // Insert sorted states in reverse order
      for (int i = sortedNodes.Count - 1; i >= 0; i--)
        yield return new PersistAction(Node, sortedNodes[i].Value, PersistActionKind.Insert);

      // Insert entities that are not referenced by anything
      foreach (var state in outboundOnlyStates)
        yield return new PersistAction(Node, state, PersistActionKind.Insert);

      // Restore loop links
      foreach (var tripletGroup in referencesToRestore.GroupBy(restoreData => restoreData.Item1)) {
        var state = tripletGroup.Key;
        state.PersistenceState = PersistenceState.Synchronized;
        state.Entity.SystemBeforeTupleChange();
        foreach (var triplet in tripletGroup) {
          var entity = triplet.Item1.Entity;
          entity
            .GetFieldAccessor(triplet.Item2)
            .SetUntypedValue(entity, triplet.Item3);
        }
        yield return new PersistAction(Node, state, PersistActionKind.Update);
      }
    }

    public override IEnumerable<PersistAction> GetPersistSequence(EntityChangeRegistry registry)
    {
      // Insert
      foreach (var action in GetInsertSequence(GetCreatedStates(registry)))
        yield return action;

      // Commit state differences, if any
      foreach (var state in GetCreatedStates(registry))
        state.CommitDifference();

      // Update
      foreach (var state in registry.GetItems(PersistenceState.Modified))
      {
        if (state.IsNotAvailableOrMarkedAsRemoved)
          continue;
        yield return new PersistAction(Node, state, PersistActionKind.Update);
        state.DifferentialTuple.Merge();
      }

      // Delete
      foreach (var action in GetDeleteSequence(GetRemovedStates(registry)))
        yield return action;
    }

    protected override IEnumerable<PersistAction> GetDeleteSequence(IEnumerable<EntityState> entityStates)
    {
      // Topological sorting
      SortAndRemoveLoopEdges(entityStates, true);

      // Restore loop links
      foreach (var triplet in referencesToRestore) {
        // No necessity to call Entity.SystemBeforeTupleChange, since it already is
        var entity = triplet.Item1.Entity;
        entity
          .GetFieldAccessor(triplet.Item2)
          .SetUntypedValue(entity, null);
        yield return new PersistAction(Node, triplet.Item1, PersistActionKind.Update);
      }

      // Remove entities that are not referenced by anything
      foreach (var state in outboundOnlyStates)
        yield return new PersistAction(Node, state, PersistActionKind.Remove);

      // Remove sorted states in direct order
      foreach (var node in sortedNodes)
        yield return new PersistAction(Node, node.Value, PersistActionKind.Remove);

      // Remove entities that do not reference anything
      foreach (var state in inboundOnlyStates)
        yield return new PersistAction(Node, state, PersistActionKind.Remove);
    }

    private void SortAndRemoveLoopEdges(IEnumerable<EntityState> entityStates, bool rollbackDifferenceBeforeSort)
    {
      var nodeIndex = new Dictionary<Key, Node<EntityState>>();
      var graph = new Graph<Node<EntityState>, Edge<AssociationInfo>>();

      inboundOnlyStates = new List<EntityState>();
      outboundOnlyStates = new List<EntityState>();

      foreach (var state in entityStates) {
        if (rollbackDifferenceBeforeSort)
          state.RollbackDifference();
        var type = state.Type;
        if (type.IsOutboundOnly)
          outboundOnlyStates.Add(state);
        else if (type.IsInboundOnly)
          inboundOnlyStates.Add(state);
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
                || ownerField.ValueType==hierarchy.Root.UnderlyingType)
              && !selfReferencingRowRemovalIsError;

          if (skipEdge)
            continue;

          new Edge<AssociationInfo>(ownerNode, targetNode, association).Attach();
        }
      }
      //In some cases, 
      List<Edge<AssociationInfo>> notBreakedEdges = new List<Edge<AssociationInfo>>();

      //This predicate filter edges where source node value can't be null reference
      Predicate<Edge<AssociationInfo>> edgeBreaker =
        edge => {
          var association = edge.Value;
          if (association.OwnerField.IsNullable)
            return true;
          notBreakedEdges.Add(edge);
          return false;
        };

      // Sort
      var result = TopologicalSorter.Sort(graph, edgeBreaker);

      //Sometimes we have loops after topological sorter.
      //In this case, we add loop nodes in tail of sorted nodes list and broke last edge in loop.
      if (result.HasLoops) {
        sortedNodes = result.SortedNodes.Union(result.LoopNodes).ToList();
        var loopNodes = result.LoopNodes.ToDictionary(el => el as Collections.Graphs.Node);
        for (var i = notBreakedEdges.Count; i-- > 0;) {
          var edge = notBreakedEdges[i];
          if (loopNodes.ContainsKey(edge.Source) && loopNodes.ContainsKey(edge.Target)) {
            result.BrokenEdges.Add(edge);
            break;
          }
        }
      }
      else {
        sortedNodes = result.SortedNodes;
      }

      // Remove loop links
      referencesToRestore = new List<(EntityState, FieldInfo, Entity)>();
      foreach (var edge in result.BrokenEdges) {
        var associationInfo = edge.Value;
        var owner = (EntityState) edge.Source.UntypedValue;
        var target = (EntityState) edge.Target.UntypedValue;

        referencesToRestore.Add((owner, associationInfo.OwnerField, target.Entity));

        var entity = owner.Entity;
        entity.SystemBeforeTupleChange();
        entity
          .GetFieldAccessor(associationInfo.OwnerField)
          .SetUntypedValue(entity, null);
      }
    }

    // Constructors

    public SortingPersistActionGenerator(StorageNode node, bool selfReferencingRowRemovalIsError)
      : base(node)
    {
      this.selfReferencingRowRemovalIsError = selfReferencingRowRemovalIsError;
    }
  }
}
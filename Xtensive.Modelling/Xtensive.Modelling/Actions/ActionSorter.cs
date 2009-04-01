// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2009.03.30

using System;
using System.Collections.Generic;
using Xtensive.Core.Sorting;
using Xtensive.Modelling.Resources;
using SNode=Xtensive.Core.Sorting.Node<Xtensive.Modelling.Actions.NodeAction, object>;


namespace Xtensive.Modelling.Actions
{
  /// <summary>
  /// Sorts <see cref="NodeAction"/>s accordingly with their dependencies.
  /// </summary>
  public static class ActionSorter
  {
    /// <summary>
    /// Sorts <see cref="NodeAction"/>s accordingly with their dependencies.
    /// </summary>
    /// <param name="actions">The actions to sort.</param>
    /// <returns>The list of sorted actions.</returns>
    /// <exception cref="InvalidOperationException">Loop in action dependency chain is detected.</exception>
    public static List<NodeAction> SortByDependency(IEnumerable<NodeAction> actions)
    {
      var invertedRequiredDependencies = new Dictionary<string, HashSet<SNode>>();
      var nodes = new List<SNode>();

      foreach (var action in actions) {
        var node = new SNode(action);
        nodes.Add(node);
        HashSet<SNode> hashSet;
        foreach (var requiredDependency in action.GetRequiredDependencies()) {
          if (!invertedRequiredDependencies.TryGetValue(requiredDependency, out hashSet)) {
            hashSet = new HashSet<SNode>();
            invertedRequiredDependencies.Add(requiredDependency, hashSet);
          }
          hashSet.Add(node);
        }
      }
      // using (Log.InfoRegion("Dependencies"))
      foreach (var source in nodes) {
        // Log.Info("{0}:", source.Item);
        foreach (var dependency in source.Item.GetDependencies()) {
          HashSet<SNode> destinations;
          if (!invertedRequiredDependencies.TryGetValue(dependency, out destinations))
            continue;
          foreach (var destination in destinations) {
            // Log.Info("->{0}", destination.Item);
            source.AddConnection(destination, true, null);
          }
        }
      }

      List<SNode> loops;
      var result = TopologicalSorter.Sort(nodes, out loops);
      if (loops!=null && loops.Count!=0)
        throw new InvalidOperationException(Strings.ExLoopInActionDependencyChain);
      return result;
    }
  }
}
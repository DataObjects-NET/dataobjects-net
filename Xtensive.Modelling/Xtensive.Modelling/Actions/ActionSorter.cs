// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2009.03.30

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Xtensive.Core;
using Xtensive.Core.Collections;
using Xtensive.Core.Sorting;


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
    public static List<NodeAction> SortByDependency(IEnumerable<NodeAction> actions)
    {
      var connections =
        new Dictionary<
          string,
          Pair<
            List<NodeAction>,
            List<NodeAction>>>();
      var sortNodes = new Dictionary<NodeAction, Core.Sorting.Node<NodeAction, string>>(actions.Count());

      foreach (var action in actions) {
        sortNodes.Add(action, new Core.Sorting.Node<NodeAction, string>(action));
        foreach (var dependency in action.GetDependencies()) {
          Pair<List<NodeAction>, List<NodeAction>> connection;
          if (!connections.TryGetValue(dependency, out connection)) {
            connection = new Pair<List<NodeAction>, List<NodeAction>>(
              new List<NodeAction>(), new List<NodeAction>());
            connections.Add(dependency, connection);
          }
          connection.First.Add(action);
        }
        foreach (var dependency in action.GetRequiredDependencies()) {
          Pair<List<NodeAction>, List<NodeAction>> connection;
          if (!connections.TryGetValue(dependency, out connection)) {
            connection = new Pair<List<NodeAction>, List<NodeAction>>(
              new List<NodeAction>(), new List<NodeAction>());
            connections.Add(dependency, connection);
          }
          connection.Second.Add(action);
        }
      }

      foreach (var pair in connections.Values) {
        foreach (var source in pair.First) {
          foreach (var target in pair.Second)
          {
            new NodeConnection<NodeAction, string>(
              sortNodes[source],
              sortNodes[target],
              null);
          } 
        }
      }

      List<Core.Sorting.Node<NodeAction, string>> loops;
      var result = TopologicalSorter.Sort(sortNodes.Values.ToList(), out loops);

      return result;
    }
  }
}
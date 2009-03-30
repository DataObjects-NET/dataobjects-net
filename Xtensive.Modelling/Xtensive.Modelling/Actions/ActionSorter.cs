// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2009.03.30

using System;
using System.Collections.Generic;
using System.Diagnostics;

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
      throw new NotImplementedException();
    }
  }
}
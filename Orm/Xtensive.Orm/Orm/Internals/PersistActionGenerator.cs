// Copyright (C) 2012 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2012.02.22

using System.Collections.Generic;
using System.Linq;

namespace Xtensive.Orm.Internals
{
  internal class PersistActionGenerator
  {
    public virtual IEnumerable<PersistAction> GetPersistSequence(EntityChangeRegistry registry, bool selfForeignKeyIsError)
    {
      // Insert
      foreach (var action in GetInsertSequence(GetCreatedStates(registry)))
        yield return action;

      // Commit state differences, if any
      foreach (var state in GetCreatedStates(registry))
        state.CommitDifference();

      // Update
      foreach (var state in registry.GetItems(PersistenceState.Modified)) {
        if (state.IsNotAvailableOrMarkedAsRemoved)
          continue;
        yield return new PersistAction(state, PersistActionKind.Update);
        state.DifferentialTuple.Merge();
      }

      // Delete
      foreach (var action in GetDeleteSequence(GetRemovedStates(registry), selfForeignKeyIsError))
        yield return action;
    }

    private static IEnumerable<EntityState> GetCreatedStates(EntityChangeRegistry registry)
    {
      return registry
        .GetItems(PersistenceState.New)
        .Where(state => !state.IsNotAvailableOrMarkedAsRemoved);
    }

    private static IEnumerable<EntityState> GetRemovedStates(EntityChangeRegistry registry)
    {
      return registry
        .GetItems(PersistenceState.Removed)
        .Except(registry.GetItems(PersistenceState.New));
    }

    protected virtual IEnumerable<PersistAction> GetInsertSequence(IEnumerable<EntityState> entityStates)
    {
      return entityStates
        .Select(state => new PersistAction(state, PersistActionKind.Insert));
    }

    protected virtual IEnumerable<PersistAction> GetDeleteSequence(IEnumerable<EntityState> entityStates, bool selfForeignKeyIsError)
    {
      return entityStates
        .Select(state => new PersistAction(state, PersistActionKind.Remove));
    }
  }
}
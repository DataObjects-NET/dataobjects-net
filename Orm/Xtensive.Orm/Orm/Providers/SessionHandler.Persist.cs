// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.08.19

using System.Collections.Generic;
using Xtensive.Orm.Internals;

namespace Xtensive.Orm.Providers
{
  partial class SessionHandler
  {
    private bool persistRequiresTopologicalSort;

    /// <summary>
    /// Persists changed entities.
    /// </summary>
    /// <param name="registry">The registry.</param>
    /// <param name="allowPartialExecution">if set to <see langword="true"/> dirty flush is allowed.</param>
    public virtual void Persist(EntityChangeRegistry registry, bool allowPartialExecution)
    {
      var actionGenerator = persistRequiresTopologicalSort
        ? new SortingPersistActionGenerator()
        : new PersistActionGenerator();

      var persistActions = actionGenerator.GetPersistSequence(registry);

      Persist(persistActions, allowPartialExecution);
    }

    /// <summary>
    /// Persists changed entities.
    /// </summary>
    /// <param name="persistActions">The entity states and the corresponding actions.</param>
    /// <param name="allowPartialExecution">if set to <see langword="true"/> partial execution is allowed.</param>
    public abstract void Persist(IEnumerable<PersistAction> persistActions, bool allowPartialExecution);
  }
}
// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.07.01

using System;
using System.Collections.Generic;
using System.Linq;
using Xtensive.Storage.Internals;
using Xtensive.Storage.Model;

namespace Xtensive.Storage.ReferentialIntegrity
{
  internal class ReferenceManager : SessionBound
  {
    private static readonly CascadeProcessor CascadeProcessor = new CascadeProcessor();
    private static readonly DenyProcessor DenyProcessor = new DenyProcessor();
    private static readonly ClearProcessor ClearProcessor = new ClearProcessor();

    public RemovalContext Context { private get; set; }

    public void BreakAssociations(Entity target, bool notify)
    {
      if (Context!=null) {
        BreakAssociations(Context, target);
        return;
      }

      using (Context = new RemovalContext(this, notify))
        BreakAssociations(Context, target);
    }

    private static void BreakAssociations(RemovalContext context, Entity target)
    {
      context.RemovalQueue.Add(target.State);
      ApplyAction(context, target, OnRemoveAction.Deny);
      ApplyAction(context, target, OnRemoveAction.Clear);
      ApplyAction(context, target, OnRemoveAction.Cascade);
    }

    private static void ApplyAction(RemovalContext context, Entity entity, OnRemoveAction action)
    {
      var associations = new HashSet<AssociationInfo>(entity.Type.GetTargetAssociations().Where(a => a.OnTargetRemove==action));
      associations.UnionWith(entity.Type.GetOwnerAssociations().Where(a => a.OnOwnerRemove==action));

      if (associations.Count==0)
        return;

      ActionProcessor processor = GetProcessor(action);

      foreach (AssociationInfo association in associations) {
        if (association.OwnerType == entity.Type)
          foreach (Entity target in association.FindTargets(entity))
            processor.Process(context, association, entity, target);
        else
          foreach (Entity owner in association.FindOwners(entity))
            processor.Process(context, association, owner, entity);
      }
    }

    private static ActionProcessor GetProcessor(OnRemoveAction action)
    {
      switch (action) {
        case OnRemoveAction.Clear:
          return ClearProcessor;
        case OnRemoveAction.Default:
          return DenyProcessor;
        case OnRemoveAction.Cascade:
          return CascadeProcessor;
        default:
          throw new ArgumentOutOfRangeException("action");
      }
    }


    // Constructors

    public ReferenceManager(Session session)
      : base(session)
    {
    }
  }
}
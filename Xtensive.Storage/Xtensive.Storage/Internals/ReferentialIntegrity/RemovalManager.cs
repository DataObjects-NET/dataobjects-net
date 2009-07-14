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
  internal class RemovalManager : SessionBound
  {
    private static readonly CascadeActionProcessor CascadeActionProcessor = new CascadeActionProcessor();
    private static readonly DenyActionProcessor DenyActionProcessor = new DenyActionProcessor();
    private static readonly ClearActionProcessor ClearActionProcessor = new ClearActionProcessor();

    public RemovalContext Context { private get; set; }

    public void Remove(Entity entity)
    {
      if (Context!=null) {
        Remove(Context, entity);
        return;
      }

      using (Context = new RemovalContext(this))
        Remove(Context, entity);
    }

    private static void Remove(RemovalContext context, Entity entity)
    {
      context.RemovalQueue.Add(entity.State);
      var sequence = entity.Type.GetAssociationSequenceForRemoval();
      foreach (var association in sequence)
        Process(context, entity, association);
    }

    private static void Process(RemovalContext context, Entity entity, AssociationInfo association)
    {
      ActionProcessor processor;

      if (association.OwnerType == entity.Type) {
        processor = GetProcessor(association.OnOwnerRemove.Value);
        foreach (var referencedObject in association.FindReferencedObjects(entity))
          processor.Process(context, association, entity, referencedObject, entity, referencedObject);
      }
      if (association.TargetType == entity.Type) {
        processor = GetProcessor(association.OnTargetRemove.Value);
        foreach (var referencingObject in association.FindReferencingObjects(entity))
          processor.Process(context, association, entity, referencingObject, referencingObject, entity);
      }
    }

    private static ActionProcessor GetProcessor(OnRemoveAction action)
    {
      switch (action) {
        case OnRemoveAction.Clear:
          return ClearActionProcessor;
        case OnRemoveAction.Default:
          return DenyActionProcessor;
        case OnRemoveAction.Cascade:
          return CascadeActionProcessor;
        default:
          throw new ArgumentOutOfRangeException("action");
      }
    }


    // Constructors

    public RemovalManager(Session session)
      : base(session)
    {
    }
  }
}
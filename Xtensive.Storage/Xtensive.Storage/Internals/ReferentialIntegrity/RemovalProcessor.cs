// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.07.01

using System.Linq;
using Xtensive.Core.Aspects;
using Xtensive.Storage.Internals;

namespace Xtensive.Storage.ReferentialIntegrity
{
  internal class RemovalProcessor : SessionBound
  {
    private static readonly CascadeActionProcessor cascadeActionProcessor = new CascadeActionProcessor();
    private static readonly DenyActionProcessor    denyActionProcessor    = new DenyActionProcessor();
    private static readonly ClearActionProcessor   clearActionProcessor   = new ClearActionProcessor();

    private readonly RemovalContext context;

    [Infrastructure]
    public void Remove(Entity item)
    {
      if (context.IsEmpty) {
        try {
          Session.EntityChangeRegistry.EnforceSizeLimit();
          // Session.Persist(); // Remove
          ProcessItem(context, item);
          ProcessQueue(context);
          MarkItemsAsRemoved(context);
          // Session.Persist(); // Remove
        }
        finally {
          context.Clear();
          Session.EntityChangeRegistry.EnforceSizeLimit();
        }
      }
      else {
        if (!context.Items.Contains(item))
          context.Queue.Enqueue(item);
      }
    }

    private void MarkItemsAsRemoved(RemovalContext context)
    {
      foreach (var item in context.Items)
        item.State.PersistenceState = PersistenceState.Removed;
    }

    private static void ProcessQueue(RemovalContext context)
    {
      while (context.Queue.Count!=0) {
        var item = context.Queue.Dequeue();
        if (!context.Items.Contains(item))
          ProcessItem(context, item);
      }
    }

    private static void ProcessItem(RemovalContext context, Entity item)
    {
      ActionProcessor actionProcessor;

      context.Items.Add(item);

      var sequence = item.Type.GetRemovalAssociationSequence();
      if (sequence==null || sequence.Count==0)
        return;

      foreach (var association in sequence) {
        if (association.OwnerType.UnderlyingType.IsAssignableFrom(item.Type.UnderlyingType)) {
          actionProcessor = GetProcessor(association.OnOwnerRemove.Value);
          foreach (var referencedObject in association.FindReferencedObjects(item).ToList())
            actionProcessor.Process(context, association, item, referencedObject, item, referencedObject);
        }

        if (association.TargetType.UnderlyingType.IsAssignableFrom(item.Type.UnderlyingType)) {
          actionProcessor = GetProcessor(association.OnTargetRemove.Value);
          foreach (var referencingObject in association.FindReferencingObjects(item).ToList())
            actionProcessor.Process(context, association, item, referencingObject, referencingObject, item);
        }
      }
    }

    private static ActionProcessor GetProcessor(OnRemoveAction action)
    {
      switch (action) {
        case OnRemoveAction.Clear:
          return clearActionProcessor;
        case OnRemoveAction.Cascade:
          return cascadeActionProcessor;
        default:
          return denyActionProcessor;
      }
    }


    // Constructors

    public RemovalProcessor(Session session)
      : base(session)
    {
      context = new RemovalContext();
    }
  }
}
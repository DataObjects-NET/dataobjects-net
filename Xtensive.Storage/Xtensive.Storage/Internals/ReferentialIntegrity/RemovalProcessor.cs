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
    private static readonly CascadeActionProcessor CascadeActionProcessor = new CascadeActionProcessor();
    private static readonly DenyActionProcessor DenyActionProcessor = new DenyActionProcessor();
    private static readonly ClearActionProcessor ClearActionProcessor = new ClearActionProcessor();

    private readonly RemovalContext context;

    [Infrastructure]
    public void Remove(Entity item)
    {
      if (context.IsEmpty) {
        try {
          Session.EntityStateRegistry.EnforceSizeLimit();
          ProcessItem(context, item);
          ProcessQueue(context);
          MarkItemsAsRemoved(context);
        }
        finally {
          context.Clear();
          Session.EntityStateRegistry.EnforceSizeLimit();
        }
      }
      else {
        if (!context.Items.Contains(item))
          context.Queue.Enqueue(item);
      }
    }

    [Infrastructure]
    private void MarkItemsAsRemoved(RemovalContext context)
    {
      foreach (var item in context.Items)
        item.State.PersistenceState = PersistenceState.Removed;
    }

    [Infrastructure]
    private static void ProcessQueue(RemovalContext context)
    {
      while (context.Queue.Count!=0) {
        var item = context.Queue.Dequeue();
        if (!context.Items.Contains(item))
          ProcessItem(context, item);
      }
    }

    [Infrastructure]
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

    [Infrastructure]
    private static ActionProcessor GetProcessor(OnRemoveAction action)
    {
      switch (action) {
        case OnRemoveAction.Clear:
          return ClearActionProcessor;
        case OnRemoveAction.Cascade:
          return CascadeActionProcessor;
        default:
          return DenyActionProcessor;
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
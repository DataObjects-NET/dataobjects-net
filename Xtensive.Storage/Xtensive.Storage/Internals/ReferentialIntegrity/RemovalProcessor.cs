// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.07.01

using System;
using System.Collections.Generic;
using System.Linq;
using Xtensive.Core.Aspects;
using Xtensive.Core.Collections;
using Xtensive.Storage.Disconnected;
using Xtensive.Storage.Internals;
using Xtensive.Storage.Model;
using Xtensive.Storage.Providers;
using Xtensive.Core.Disposing;

namespace Xtensive.Storage.ReferentialIntegrity
{
  [Infrastructure]
  internal class RemovalProcessor : SessionBound
  {
    private static readonly CascadeActionProcessor cascadeActionProcessor = new CascadeActionProcessor();
    private static readonly DenyActionProcessor    denyActionProcessor    = new DenyActionProcessor();
    private static readonly ClearActionProcessor   clearActionProcessor   = new ClearActionProcessor();

    internal readonly RemovalContext Context;

    public void Remove(Entity item)
    {
      if (Context.IsEmpty) {
        try {
          Session.EnforceChangeRegistrySizeLimit();
          // Session.Persist(); // Remove
          ProcessItem(item);
          ProcessQueue();
          MarkItemsAsRemoved();
          // Session.Persist(); // Remove
        }
        finally {
          Context.Clear();
          Session.EnforceChangeRegistrySizeLimit();
        }
      }
      else {
        if (!Context.Items.Contains(item))
          Context.Queue.Enqueue(item);
      }
    }

    private void MarkItemsAsRemoved()
    {
      foreach (var item in Context.Items)
        item.State.PersistenceState = PersistenceState.Removed;
    }

    private void ProcessQueue()
    {
      var queue = Context.Queue;
      if (queue.Count == 0)
        return;
      TypeInfo prevItemType = null;
      var itemList = new List<Entity>();
      Action<List<Entity>> processItems = (itemsToProcess) => {
        ExecutePrefetchAction(itemsToProcess);
        foreach (var entity in itemsToProcess)
          ProcessItem(entity);
        itemsToProcess.Clear();
      };
      while(queue.Count != 0) {
        var item = queue.Dequeue();
        if (!Context.Items.Contains(item)) {
          if (itemList.Count!=0 && item.Type!=(prevItemType ?? item.Type))
            processItems(itemList);
          itemList.Add(item);
          prevItemType = item.Type;
        }
        if (queue.Count != 0 || itemList.Count == 0)
          continue;
        processItems(itemList);
      }
    }

    private void ProcessItem(Entity item)
    {
      ActionProcessor actionProcessor;
      var isEmpty = Context.IsEmpty;
      Context.Items.Add(item);

      var sequence = item.Type.GetRemovalAssociationSequence();
      if (sequence==null || sequence.Count==0)
        return;

      if (isEmpty)
        ExecutePrefetchAction(new List<Entity>() { item });

      foreach (var association in sequence) {
        if (association.OwnerType.UnderlyingType.IsAssignableFrom(item.Type.UnderlyingType)) {
          actionProcessor = GetProcessor(association.OnOwnerRemove.Value);
          foreach (var reference in ReferenceFinder.GetReferencesFrom(item, association).ToList())
            actionProcessor.Process(Context, association, item, reference.ReferencedEntity, item, reference.ReferencedEntity);
        }

        if (association.TargetType.UnderlyingType.IsAssignableFrom(item.Type.UnderlyingType)) {
          actionProcessor = GetProcessor(association.OnTargetRemove.Value);
          foreach (var reference in ReferenceFinder.GetReferencesTo(item, association).ToList())
            actionProcessor.Process(Context, association, item, reference.ReferencingEntity, reference.ReferencingEntity, item);
        }
      }
    }

    private void ExecutePrefetchAction(List<Entity> itemList)
    {
      if ((Session.Handler is DisconnectedSessionHandler))
        return;
      var item = itemList[0];
      Action<SessionHandler, IEnumerable<Key>> action;
      if (Session.Domain.PrefetchActionMap.TryGetValue(item.Type, out action))
        action(Session.Handler, itemList.Select(i => i.Key));
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
      Context = new RemovalContext();
    }
  }
}
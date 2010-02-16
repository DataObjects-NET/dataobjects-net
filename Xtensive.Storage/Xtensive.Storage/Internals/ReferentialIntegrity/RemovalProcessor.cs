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

    class ReferenceContainer
    {
      public Entity RemovingEntity;
      public ActionProcessor Processor;
      public AssociationInfo Association;
      public IEnumerable<ReferenceInfo> References;
      public bool IsOutgoing;
    }

    internal readonly RemovalContext Context;

    public void Remove(Entity item)
    {
      if (Context.IsEmpty) {
        try {
          Session.EnforceChangeRegistrySizeLimit();
          ProcessItems(new List<Entity>(){item});
          ProcessQueue();
          MarkItemsAsRemoved();
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
      while(queue.Count != 0) {
        var item = queue.Dequeue();
        if (!Context.Items.Contains(item)) {
          if (itemList.Count!=0 && item.Type!=(prevItemType ?? item.Type)) {
            ProcessItems(itemList);
            itemList.Clear();
          }
          itemList.Add(item);
          prevItemType = item.Type;
        }
        if (queue.Count != 0 || itemList.Count == 0)
          continue;
        ProcessItems(itemList);
        itemList.Clear();
      }
    }

    private void ProcessItems(List<Entity> entities)
    {
      if (entities.Count == 0)
        return;

      foreach (var entity in entities)
        Context.Items.Add(entity);

      var entityType = entities[0].Type;
      var sequence = entityType.GetRemovalAssociationSequence();
      if (sequence==null || sequence.Count==0)
        return;

      ExecutePrefetchAction(entities);

      var referenceContainers = new List<ReferenceContainer>();
      foreach (var association in sequence) {
        if (association.OwnerType.UnderlyingType.IsAssignableFrom(entityType.UnderlyingType)) {
          foreach (var entity in entities) {
            var container = new ReferenceContainer() {
              RemovingEntity = entity,
              Processor = GetProcessor(association.OnOwnerRemove.Value),
              Association = association,
              References = ReferenceFinder.GetReferencesFrom(entity, association),
              IsOutgoing = true
            };
            referenceContainers.Add(container);
          }
        }
        if (association.TargetType.UnderlyingType.IsAssignableFrom(entityType.UnderlyingType)) {
          foreach (var entity in entities) {
            var container = new ReferenceContainer() {
              RemovingEntity = entity,
              Processor = GetProcessor(association.OnTargetRemove.Value),
              Association = association,
              References = ReferenceFinder.GetReferencesTo(entity, association),
              IsOutgoing = false
            };
            referenceContainers.Add(container);
          }
        }
      }

      if (Session.Handler.ExecutePrefetchTasks()==null);
        Session.ExecuteDelayedQueries();

      foreach (var container in referenceContainers) {
        var processor = container.Processor;
        var association = container.Association;
        var removingEntity = container.RemovingEntity;
        if (container.IsOutgoing)
          foreach (var reference in container.References.ToList())
            processor.Process(Context, association, removingEntity, reference.ReferencedEntity, removingEntity, reference.ReferencedEntity);
        else
          foreach (var reference in container.References.ToList())
            processor.Process(Context, association, removingEntity, reference.ReferencingEntity, reference.ReferencingEntity, removingEntity);
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
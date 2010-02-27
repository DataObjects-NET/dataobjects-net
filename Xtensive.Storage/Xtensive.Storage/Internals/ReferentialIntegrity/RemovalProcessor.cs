// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.07.01

using System;
using System.Collections.Generic;
using System.Linq;
using Xtensive.Core.Aspects;
using Xtensive.Storage.Disconnected;
using Xtensive.Storage.Internals;
using Xtensive.Storage.Model;
using Xtensive.Storage.Operations.Internals;
using Xtensive.Storage.Providers;
using Xtensive.Storage.Operations;

namespace Xtensive.Storage.ReferentialIntegrity
{
  [Infrastructure]
  internal class RemovalProcessor : SessionBound
  {
    #region Nested type: ReferenceDescriptor

    class ReferenceDescriptor
    {
      public Entity RemovingEntity;
      public ActionProcessor Processor;
      public AssociationInfo Association;
      public IEnumerable<ReferenceInfo> References;
      public bool IsOutgoing;
    }

    #endregion

    private static readonly CascadeActionProcessor cascadeActionProcessor = new CascadeActionProcessor();
    private static readonly DenyActionProcessor    denyActionProcessor    = new DenyActionProcessor();
    private static readonly ClearActionProcessor   clearActionProcessor   = new ClearActionProcessor();
    
    public RemovalContext Context { get; set; }

    public void EnqueueForRemoval(IEnumerable<Entity> entities)
    {
      if (Context != null)
        Context.Enqueue(entities);
      else
        Remove(entities);
    }

    public void Remove(IEnumerable<Entity> entities)
    {
      var processedEntities = new List<Entity>();
      try {
        using (var region = Validation.Disable())
        using (var operationContext = OpenOperationContext()) 
        using (Context = new RemovalContext(this)) {
          Session.EnforceChangeRegistrySizeLimit();
          foreach (var entity in entities) {
            entity.EnsureNotRemoved();
            if (operationContext.IsLoggingEnabled)
              operationContext.LogOperation(
                new EntityRemoveOperation(entity.Key));
          }
          Context.Enqueue(entities);
          while (!Context.QueueIsEmpty) {
            var entitiesForProcessing = Context.GatherEntitiesForProcessing();
            foreach (var entity in entitiesForProcessing)
              entity.SystemBeforeRemove();
            ProcessItems(entitiesForProcessing);
          }
          processedEntities = Context.GetProcessedEntities().ToList();
          foreach (var entity in processedEntities) {
            entity.SystemRemove();
            entity.State.PersistenceState = PersistenceState.Removed;
          }
          Session.EnforceChangeRegistrySizeLimit();
          region.Complete();
          operationContext.Complete();
          foreach (var entity in processedEntities)
            entity.SystemRemoveCompleted(null);
        }
      }
      catch (Exception e) {
        foreach (var entity in processedEntities)
          entity.SystemRemoveCompleted(e);
        throw;
      }
    }

    private void ProcessItems(IList<Entity> entities)
    {
      if (entities.Count == 0)
        return;

      var entityType = entities[0].Type;
      var sequence = entityType.GetRemovalAssociationSequence();
      if (sequence==null || sequence.Count==0)
        return;

      ExecutePrefetchAction(entities);

      var referenceDescriptors = new List<ReferenceDescriptor>();
      foreach (var association in sequence) {
        if (association.OwnerType.UnderlyingType.IsAssignableFrom(entityType.UnderlyingType)) {
          foreach (var entity in entities) {
            var descriptor = new ReferenceDescriptor {
              RemovingEntity = entity,
              Processor = GetProcessor(association.OnOwnerRemove.Value),
              Association = association,
              References = ReferenceFinder.GetReferencesFrom(entity, association),
              IsOutgoing = true
            };
            referenceDescriptors.Add(descriptor);
          }
        }
        if (association.TargetType.UnderlyingType.IsAssignableFrom(entityType.UnderlyingType)) {
          foreach (var entity in entities) {
            var descriptor = new ReferenceDescriptor {
              RemovingEntity = entity,
              Processor = GetProcessor(association.OnTargetRemove.Value),
              Association = association,
              References = ReferenceFinder.GetReferencesTo(entity, association),
              IsOutgoing = false
            };
            referenceDescriptors.Add(descriptor);
          }
        }
      }

      if (Session.Handler.ExecutePrefetchTasks()==null);
        Session.ExecuteDelayedQueries();

      foreach (var container in referenceDescriptors) {
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

    private void ExecutePrefetchAction(IList<Entity> itemList)
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
    {}
  }
}
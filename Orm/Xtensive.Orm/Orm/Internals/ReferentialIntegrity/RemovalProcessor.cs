// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.07.01

using System;
using System.Collections.Generic;
using System.Linq;
using Xtensive.Core;
using Xtensive.Orm.Internals;
using Xtensive.Orm.Model;
using Xtensive.Orm.Providers;
using Xtensive.Orm.Operations;

namespace Xtensive.Orm.ReferentialIntegrity
{
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
    private static readonly NoneActionProcessor    noneActionProcessor    = new NoneActionProcessor();
    
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
      ArgumentValidator.EnsureArgumentNotNull(entities, "entities");
      bool isEmpty = true;
      foreach (var entity in entities) {
        isEmpty = false;
        entity.EnsureNotRemoved();
      }
      if (isEmpty)
        return;
      var processedEntities = new List<Entity>();
      var notifiedEntities = new HashSet<Entity>();
      try {
        var operations = Session.Operations;
        using (var scope = operations.BeginRegistration(OperationType.System))
        using (Context = new RemovalContext(this)) {
          Session.EnforceChangeRegistrySizeLimit();
          if (operations.CanRegisterOperation)
            operations.RegisterOperation(
              new EntitiesRemoveOperation(entities.Select(e => e.Key)));

          Context.Enqueue(entities);

          bool isOperationStarted = false;
          while (!Context.QueueIsEmpty) {
            var entitiesForProcessing = Context.GatherEntitiesForProcessing();
            foreach (var entity in entitiesForProcessing)
              entity.SystemBeforeRemove();
            if (!isOperationStarted) {
              isOperationStarted = true;
              operations.NotifyOperationStarting();
            }
            ProcessItems(entitiesForProcessing);
          }
          if (!isOperationStarted)
            operations.NotifyOperationStarting();

          processedEntities = Context.GetProcessedEntities().ToList();
          foreach (var entity in processedEntities) {
            entity.SystemRemove();
            entity.State.PersistenceState = PersistenceState.Removed;
          }
          Context.ProcessFinalizers();
          Session.EnforceChangeRegistrySizeLimit();

          scope.Complete(); // Successful anyway

          using (var ea = new ExceptionAggregator()) {
            foreach (var entity in processedEntities) {
              ea.Execute(() => {
                notifiedEntities.Add(entity);
                entity.SystemRemoveCompleted(null);
              });
            }
            ea.Complete();
          }
        }
      }
      catch (Exception e) {
        foreach (var entity in processedEntities) {
          if (notifiedEntities.Contains(entity))
            continue;
          try {
            entity.SystemRemoveCompleted(e);
          }
// ReSharper disable EmptyGeneralCatchClause
          catch {}
// ReSharper restore EmptyGeneralCatchClause
        }
        throw;
      }
    }

    private void ProcessItems(IList<Entity> entities)
    {
      if (entities.Count == 0)
        return;

      var entityType = entities[0].TypeInfo;
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

      if (Session.Handler.ExecutePrefetchTasks()==null)
        Session.ExecuteDelayedQueries(false);

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
      var item = itemList[0];
      Action<SessionHandler, IEnumerable<Key>> action;
      if (Session.Domain.PrefetchActionMap.TryGetValue(item.TypeInfo, out action))
        action(Session.Handler, itemList.Select(i => i.Key));
    }

    private static ActionProcessor GetProcessor(OnRemoveAction action)
    {
      switch (action) {
        case OnRemoveAction.Clear:
          return clearActionProcessor;
        case OnRemoveAction.Cascade:
          return cascadeActionProcessor;
        case OnRemoveAction.Deny:
          return denyActionProcessor;
        default:
          return noneActionProcessor;
      }
    }


    // Constructors

    public RemovalProcessor(Session session)
      : base(session)
    {}
  }
}
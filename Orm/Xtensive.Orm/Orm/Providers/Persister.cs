// Copyright (C) 2012 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2012.04.02

using System;
using System.Collections.Generic;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Internals;
using Xtensive.Tuples;

namespace Xtensive.Orm.Providers
{
  internal sealed class Persister
  {
    private readonly PersistRequestBuilder requestBuilder;
    private readonly bool sortingRequired;

    public void Persist(EntityChangeRegistry registry, CommandProcessor processor)
    {
      var selfReferencingRowRemovalIsError = requestBuilder.Handlers.ProviderInfo.Supports(ProviderFeatures.SelfReferencingRowRemovalIsError);
      var node = registry.Session.StorageNode;
      var actionGenerator = sortingRequired
        ? new SortingPersistActionGenerator(node, selfReferencingRowRemovalIsError)
        : new PersistActionGenerator(node);

      var validateVersion = registry.Session.Configuration.Supports(SessionOptions.ValidateEntityVersions);

      var actions = actionGenerator.GetPersistSequence(registry);
      foreach (var action in actions)
        processor.RegisterTask(CreatePersistTask(action, validateVersion));
    }

    #region Private / internal methods

    private SqlPersistTask CreatePersistTask(PersistAction action, bool validateVersion)
    {
      switch (action.ActionKind) {
      case PersistActionKind.Insert:
        return CreateInsertTask(action);
      case PersistActionKind.Update:
        return CreateUpdateTask(action, validateVersion && action.EntityState.Type.HasVersionFields);
      case PersistActionKind.Remove:
        return CreateRemoveTask(action, validateVersion && action.EntityState.Type.HasVersionFields);
      default:
        throw new ArgumentOutOfRangeException("action.ActionKind");
      }
    }
    
    private SqlPersistTask CreateInsertTask(PersistAction action)
    {
      var entityState = action.EntityState;
      var tuple = entityState.Tuple.ToRegular();
      var request = GetOrBuildRequest(action.Node, PersistRequestBuilderTask.Insert(entityState.Type));
      return new SqlPersistTask(entityState.Key, request, tuple);
    }

    private SqlPersistTask CreateUpdateTask(PersistAction action, bool validateVersion)
    {
      var entityState = action.EntityState;
      var dTuple = entityState.DifferentialTuple;
      var tuple = entityState.Tuple.ToRegular();
      var changedFields = dTuple.Difference.GetFieldStateMap(TupleFieldState.Available);
      if (validateVersion) {
        var availableFields = dTuple.Origin.GetFieldStateMap(TupleFieldState.Available);
        var request = GetOrBuildRequest(action.Node,
          PersistRequestBuilderTask.UpdateWithVersionCheck(entityState.Type, availableFields, changedFields));
        var versionTuple = dTuple.Origin.ToRegular();
        return new SqlPersistTask(entityState.Key, request, tuple, versionTuple, true);
      }
      else {
        var request = GetOrBuildRequest(action.Node, PersistRequestBuilderTask.Update(entityState.Type, changedFields));
        return new SqlPersistTask(entityState.Key, request, tuple);
      }
    }

    private SqlPersistTask CreateRemoveTask(PersistAction action, bool validateVersion)
    {
      var entityState = action.EntityState;
      var tuple = entityState.Key.Value;
      
      if (validateVersion) {
        var versionTuple = entityState.DifferentialTuple.Origin.ToRegular();
        var availableFields = versionTuple.GetFieldStateMap(TupleFieldState.Available);
        var request = GetOrBuildRequest(action.Node,
          PersistRequestBuilderTask.RemoveWithVersionCheck(entityState.Type, availableFields));
        return new SqlPersistTask(entityState.Key, request, tuple, versionTuple, true);
      }
      else {
        var request = GetOrBuildRequest(action.Node, PersistRequestBuilderTask.Remove(entityState.Type));
        return new SqlPersistTask(entityState.Key, request, tuple);
      }
    }

    private IEnumerable<PersistRequest> GetOrBuildRequest(StorageNode node, PersistRequestBuilderTask task)
    {
      var cache = node.PersistRequestCache;
      IEnumerable<PersistRequest> result;
      if (cache.TryGetValue(task, out result))
        return result;
      result = requestBuilder.Build(node, task);
      return cache.TryAdd(task, result) ? result : cache[task];
    }

    #endregion

    // Constructors

    public Persister(HandlerAccessor handlers, PersistRequestBuilder requestBuilder)
    {
      var providerInfo = handlers.ProviderInfo;
      var configuration = handlers.Domain.Configuration;

      this.requestBuilder = requestBuilder;

      sortingRequired =
        configuration.Supports(ForeignKeyMode.Reference)
        && providerInfo.Supports(ProviderFeatures.ForeignKeyConstraints)
        && !providerInfo.Supports(ProviderFeatures.DeferrableConstraints);
    }
  }
}
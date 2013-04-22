// Copyright (C) 2012 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2012.04.02

using System;
using System.Collections.Generic;
using Xtensive.Collections;
using Xtensive.Orm.Internals;
using Xtensive.Tuples;

namespace Xtensive.Orm.Providers
{
  internal sealed class Persister
  {
    private readonly PersistRequestBuilder requestBuilder;
    private readonly bool sortingRequired;

    private ThreadSafeDictionary<PersistRequestBuilderTask, IEnumerable<PersistRequest>> requestCache
      = ThreadSafeDictionary<PersistRequestBuilderTask, IEnumerable<PersistRequest>>.Create(new object());

    public void Persist(EntityChangeRegistry registry, CommandProcessor processor)
    {
      var actionGenerator = sortingRequired
        ? new SortingPersistActionGenerator()
        : new PersistActionGenerator();

      var actions = actionGenerator.GetPersistSequence(registry);
      foreach (var action in actions)
        processor.RegisterTask(CreatePersistTask(action));
    }

    #region Private / internal methods

    private SqlPersistTask CreatePersistTask(PersistAction action)
    {
      switch (action.ActionKind) {
      case PersistActionKind.Insert:
        return CreateInsertTask(action);
      case PersistActionKind.Update:
        return CreateUpdateTask(action);
      case PersistActionKind.Remove:
        return CreateRemoveTask(action);
      default:
        throw new ArgumentOutOfRangeException("action.ActionKind");
      }
    }
    
    private SqlPersistTask CreateInsertTask(PersistAction action)
    {
      var task = new PersistRequestBuilderTask(PersistRequestKind.Insert, action.EntityState.Type);
      var request = GetRequest(task);
      var tuple = action.EntityState.Tuple.ToRegular();
      return new SqlPersistTask(request, tuple);
    }

    private SqlPersistTask CreateUpdateTask(PersistAction action)
    {
      var entityState = action.EntityState;
      var dTuple = entityState.DifferentialTuple;
      var source = dTuple.Difference;
      var fieldStateMap = source.GetFieldStateMap(TupleFieldState.Available);
      var task = new PersistRequestBuilderTask(PersistRequestKind.Update, entityState.Type, fieldStateMap);
      var request = GetRequest(task);
      var tuple = entityState.Tuple.ToRegular();
      return new SqlPersistTask(request, tuple);
    }

    private SqlPersistTask CreateRemoveTask(PersistAction action)
    {
      var task = new PersistRequestBuilderTask(PersistRequestKind.Remove, action.EntityState.Type);
      var request = GetRequest(task);
      var tuple = action.EntityState.Key.Value;
      return new SqlPersistTask(request, tuple);
    }

    private IEnumerable<PersistRequest> GetRequest(PersistRequestBuilderTask task)
    {
      return requestCache.GetValue(task, requestBuilder.Build);
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
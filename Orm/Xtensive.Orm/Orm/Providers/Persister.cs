﻿// Copyright (C) 2012 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2012.04.02

using System;
using System.Collections.Generic;
using Xtensive.Collections;
using Xtensive.Orm.Configuration;
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
        return CreateUpdateTask(action, validateVersion);
      case PersistActionKind.Remove:
        return CreateRemoveTask(action, validateVersion);
      default:
        throw new ArgumentOutOfRangeException("action.ActionKind");
      }
    }
    
    private SqlPersistTask CreateInsertTask(PersistAction action)
    {
      var task = new PersistRequestBuilderTask(PersistRequestKind.Insert, action.EntityState.Type, null, false);
      var request = GetOrBuildRequest(task);
      var tuple = action.EntityState.Tuple.ToRegular();
      return new SqlPersistTask(request, tuple);
    }

    private SqlPersistTask CreateUpdateTask(PersistAction action, bool validateVersion)
    {
      var entityState = action.EntityState;
      var dTuple = entityState.DifferentialTuple;
      var source = dTuple.Difference;
      var fieldStateMap = source.GetFieldStateMap(TupleFieldState.Available);
      var task = new PersistRequestBuilderTask(PersistRequestKind.Update, entityState.Type, fieldStateMap, validateVersion);
      var request = GetOrBuildRequest(task);
      var tuple = entityState.Tuple.ToRegular();
      if (validateVersion) {
        var versionTuple = dTuple.Origin.ToRegular();
        return new SqlPersistTask(request, tuple, versionTuple, true);
      }
      return new SqlPersistTask(request, tuple);
    }

    private SqlPersistTask CreateRemoveTask(PersistAction action, bool validateVersion)
    {
      var entityState = action.EntityState;
      var task = new PersistRequestBuilderTask(PersistRequestKind.Remove, entityState.Type, null, validateVersion);
      var request = GetOrBuildRequest(task);
      var tuple = entityState.Key.Value;
      if (validateVersion) {
        var versionTuple = entityState.DifferentialTuple.Origin.ToRegular();
        return new SqlPersistTask(request, tuple, versionTuple, true);
      }
      return new SqlPersistTask(request, tuple);
    }

    private IEnumerable<PersistRequest> GetOrBuildRequest(PersistRequestBuilderTask task)
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
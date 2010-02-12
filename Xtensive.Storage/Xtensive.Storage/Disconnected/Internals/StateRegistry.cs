// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Ivan Galkin
// Created:    2009.10.23

using System;
using System.Collections.Generic;
using Xtensive.Core;
using Xtensive.Core.Internals.DocTemplates;
using Xtensive.Core.Tuples;
using Xtensive.Storage.Internals;
using Xtensive.Storage.Model;
using Xtensive.Storage.Operations;
using Xtensive.Storage.Resources;

namespace Xtensive.Storage.Disconnected
{
  /// <summary>
  /// Disconnected state registry.
  /// </summary>
  internal sealed class StateRegistry
  {
    private readonly StateRegistry origin;
    private readonly Dictionary<Key, DisconnectedEntityState> items;
    private readonly AssociationCache associationCache;

    public StateRegistry Origin { get { return origin; } }

    public IEnumerable<DisconnectedEntityState> EntityStates { get { return items.Values; } }
    public OperationSet Operations { get; set; }

    public DisconnectedEntityState Get(Key key)
    {
      DisconnectedEntityState state;
      if (items.TryGetValue(key, out state))
        return state;
      if (Origin!=null)
        return Origin.Get(key);
      return null;
    }

    public DisconnectedEntityState GetOrCreate(Key key)
    {
      DisconnectedEntityState state;
      if (items.TryGetValue(key, out state))
        return state;
      if (Origin!=null)
        state = new DisconnectedEntityState(Origin.GetOrCreate(key));
      else 
        state = new DisconnectedEntityState(key);
      items.Add(key, state);
      return state;
    }

    public void Insert(Key key, Tuple tuple)
    {
      if (Origin==null)
        return;

      var state = GetOrCreate(key);
      if (state.IsLoaded)
        throw new InvalidOperationException(string.Format(
          Strings.ExStateWithKeyXIsAlreadyExists, key));

      state.Tuple = tuple.Clone();
      OnStateChanged(key, null, state.Tuple);
      foreach (var fieldInfo in associationCache.GetEntitySetFields(key.TypeRef.Type))
        state.GetEntitySetState(fieldInfo).IsFullyLoaded = true;
    }

    public void Update(Key key, Tuple difference)
    {
      if (Origin==null)
        return;

      var state = GetOrCreate(key);
      if (!state.IsLoaded)
        throw new InvalidOperationException(Strings.ExStateIsNotLoaded);

      var prevValue = state.Tuple.ToRegular();
      state.Update(difference.Clone());
      var newValue = state.Tuple.ToRegular();

      var type = state.Key.TypeRef.Type;
      var baseType = type.UnderlyingType.BaseType;
      var isAuxEntity = baseType.IsGenericType && baseType.GetGenericTypeDefinition()==typeof (EntitySetItem<,>);
      if (isAuxEntity)
        return;
      OnStateChanged(key, prevValue, newValue);
    }

    public void Remove(Key key)
    {
      if (Origin==null)
        return;
      
      var state = GetOrCreate(key);
      if (!state.IsLoaded)
        throw new InvalidOperationException(Strings.ExStateIsNotLoaded);
      
      OnStateChanged(key, state.Tuple, null);
      state.Remove();
    }

    public void Register(Key key, Tuple tuple)
    {
      if (Origin!=null)
        throw new InvalidOperationException(Strings.ExCantRegisterState);

      var state = GetOrCreate(key);
      if (state.IsRemoved)
        return;

      state.Tuple = tuple.Clone();
      OnStateChanged(key, null, state.Tuple);
    }

    public void MergeUnavailableFields(Key key, Tuple newValue)
    {
      if (Origin!=null)
        throw new InvalidOperationException(Strings.ExCantMergeState);

      var state = GetOrCreate(key);
      if (state.IsRemoved || !state.IsLoaded)
        throw new InvalidOperationException(Strings.ExCantMergeState);

      if (state.MergeValue(newValue))
        OnStateChanged(key, null, state.Tuple);
    }

    public void Merge(Key key, Tuple newValue)
    {
      if (Origin!=null)
        throw new InvalidOperationException(Strings.ExCantMergeState);

      var state = GetOrCreate(key);
      if (state.IsRemoved || !state.IsLoaded)
        throw new InvalidOperationException(Strings.ExCantMergeState);

      var preValue = state.Tuple.Clone();
      state.SetNewValue(newValue);
      OnStateChanged(key, preValue, state.Tuple);
    }

    public void Commit(bool clearLoggedOperations)
    {
      try {
        if (Origin==null)
          return;

        foreach (var state in items)
          state.Value.Commit();
        if (Origin.Operations!=null)
          Origin.Operations.Append(Operations);
      }
      finally {
        if (clearLoggedOperations)
          Operations.Clear();
      }
    }

    /// <exception cref="InvalidOperationException">Origin!=null</exception>
    public void Remap(KeyMapping keyMapping)
    {
      if (origin!=null)
        throw Exceptions.InternalError("Origin!=null", Log.Instance);
      foreach (var map in keyMapping.Map) {
        var hasToBeReplaced = false;
        foreach (var itemPair in items) {
          if (!hasToBeReplaced && itemPair.Key==map.Key)
            hasToBeReplaced = true;
          itemPair.Value.Remap(map.Key, map.Value);
        }
        if (!hasToBeReplaced)
          continue;
        DisconnectedEntityState entityState;
        if (items.TryGetValue(map.Key, out entityState)) {
          items.Remove(map.Key);
          items.Add(map.Value, entityState);
        }
      }
    }

    public void AddState(DisconnectedEntityState state)
    {
      items.Add(state.Key, state);
    }

    public void OnStateChanged(Key key, Tuple prevValue, Tuple newValue)
    {
      // Inserting
      if (prevValue==null) {
        foreach (var item in associationCache.GetEntitySetItems(key, newValue))
          InsertIntoEntitySet(item.OwnerKey, item.Field, item.ItemKey);
        foreach (var reference in associationCache.GetReferencesFrom(key, newValue))
          AddReference(reference.TargetKey, reference.Field, reference.ReferencingKey);
      }
      // Deleting
      else if (newValue==null) {
        foreach (var item in associationCache.GetEntitySetItems(key, prevValue))
          RemoveFromEntitySet(item.OwnerKey, item.Field, item.ItemKey);
        foreach (var reference in associationCache.GetReferencesFrom(key, prevValue))
          RemoveReference(reference.TargetKey, reference.Field, reference.ReferencingKey);
      }
      // Updating
      else {
        foreach (var pair in associationCache.GetEntitySets(key.Type)) {
          var prevOwnerKey = associationCache.GetKeyFieldValue(pair.First, prevValue);
          var ownerKey = associationCache.GetKeyFieldValue(pair.First, newValue);
          if (ownerKey!=prevOwnerKey) {
            if (prevOwnerKey!=null)
              RemoveFromEntitySet(prevOwnerKey, pair.Second, key);
            if (ownerKey!=null)
              InsertIntoEntitySet(ownerKey, pair.Second, key);
          }
        }
        foreach (var field in associationCache.GetReferencingFields(key.Type)) {
          var prevOwnerKey = associationCache.GetKeyFieldValue(field, prevValue);
          var ownerKey = associationCache.GetKeyFieldValue(field, newValue);
          if (ownerKey!=prevOwnerKey) {
            if (prevOwnerKey!=null)
              RemoveReference(prevOwnerKey, field, key);
            if (ownerKey!=null)
              AddReference(ownerKey, field, key);
          }
        }
      }
    }

    private void InsertIntoEntitySet(Key ownerKey, FieldInfo field, Key itemKey)
    {
      var state = GetOrCreate(ownerKey);
      var entitySet = state.GetEntitySetState(field);
      if (!entitySet.Items.ContainsKey(itemKey))
        entitySet.Items.Add(itemKey, itemKey);
    }

    private void RemoveFromEntitySet(Key ownerKey, FieldInfo field, Key itemKey)
    {
      var state = GetOrCreate(ownerKey);
      var entitySet = state.GetEntitySetState(field);
      if (entitySet.Items.ContainsKey(itemKey))
        entitySet.Items.Remove(itemKey);
    }

    private void AddReference(Key targetKey, FieldInfo fieldInfo, Key itemKey)
    {
      var state = GetOrCreate(targetKey);
      var references = state.GetReferences(fieldInfo);
      if (!references.ContainsKey(itemKey))
        references.Add(itemKey, itemKey);
    }

    private void RemoveReference(Key targetKey, FieldInfo fieldInfo, Key itemKey)
    {
      var state = GetOrCreate(targetKey);
      var references = state.GetReferences(fieldInfo);
      if (references.ContainsKey(itemKey))
        references.Remove(itemKey);
    }


    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    
    public StateRegistry(AssociationCache associationCache)
    {
      ArgumentValidator.EnsureArgumentNotNull(associationCache, "modelRequestCache");

      items = new Dictionary<Key, DisconnectedEntityState>();
      this.associationCache = associationCache;
      Operations = new OperationSet();
    }

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="origin">The origin registry.</param>
    public StateRegistry(StateRegistry origin)
    {
      ArgumentValidator.EnsureArgumentNotNull(origin, "origin");

      items = new Dictionary<Key, DisconnectedEntityState>();
      this.origin = origin;
      associationCache = origin.associationCache;
      Operations = new OperationSet();
    }
  }
}
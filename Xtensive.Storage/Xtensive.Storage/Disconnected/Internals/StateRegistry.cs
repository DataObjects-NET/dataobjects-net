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
    private readonly DisconnectedState disconnectedState;
    private readonly Dictionary<Key, DisconnectedEntityState> states;

    private ModelHelper ModelHelper { get { return disconnectedState.ModelHelper; } }

    public IEnumerable<DisconnectedEntityState> States { get { return states.Values; } }

    public OperationSet OperationSet { get; set; }

    public StateRegistry Origin { get { return origin; } }

    public DisconnectedEntityState GetState(Key key)
    {
      DisconnectedEntityState state;
      if (states.TryGetValue(key, out state))
        return state;
      if (Origin!=null)
        return Origin.GetState(key);
      return null;
    }

    public DisconnectedEntityState GetForUpdate(Key key)
    {
      DisconnectedEntityState state;
      if (states.TryGetValue(key, out state))
        return state;
      if (Origin!=null)
        state = new DisconnectedEntityState(Origin.GetForUpdate(key));
      else 
        state = new DisconnectedEntityState(key);
      states.Add(key, state);
      return state;
    }
    
    public void Insert(Key key, Tuple tuple)
    {
      if (Origin==null)
        return;

      var state = GetForUpdate(key);
      if (state.IsLoaded)
        throw new InvalidOperationException(string.Format(
          Strings.ExStateWithKeyXIsAlreadyExists, key));

      state.Tuple = tuple.Clone();
      OnStateChanged(key, null, state.Tuple);
      foreach (var fieldInfo in ModelHelper.GetEntitySetFields(key.TypeRef.Type))
        state.GetEntitySetState(fieldInfo).IsFullyLoaded = true;
    }

    public void Update(Key key, Tuple difference)
    {
      if (Origin==null)
        return;

      var state = GetForUpdate(key);
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
      
      var state = GetForUpdate(key);
      if (!state.IsLoaded)
        throw new InvalidOperationException(Strings.ExStateIsNotLoaded);
      
      OnStateChanged(key, state.Tuple, null);
      state.Remove();
    }

    public void Register(Key key, Tuple tuple)
    {
      if (Origin!=null)
        throw new InvalidOperationException(Strings.ExCantRegisterState);

      var state = GetForUpdate(key);
      if (state.IsRemoved)
        return;

      state.Tuple = tuple.Clone();
      OnStateChanged(key, null, state.Tuple);
    }

    public void MergeUnloadedFields(Key key, Tuple newValue)
    {
      if (Origin!=null)
        throw new InvalidOperationException(Strings.ExCantMergeState);

      var state = GetForUpdate(key);
      if (state.IsRemoved || !state.IsLoaded)
        throw new InvalidOperationException(Strings.ExCantMergeState);

      if (state.MergeValue(newValue))
        OnStateChanged(key, null, state.Tuple);
    }

    public void Merge(Key key, Tuple newValue)
    {
      if (Origin!=null)
        throw new InvalidOperationException(Strings.ExCantMergeState);

      var state = GetForUpdate(key);
      if (state.IsRemoved || !state.IsLoaded)
        throw new InvalidOperationException(Strings.ExCantMergeState);

      var preValue = state.Tuple.Clone();
      state.SetNewValue(newValue);
      OnStateChanged(key, preValue, state.Tuple);
    }

    public void Commit()
    {
      if (Origin==null)
        return;

      foreach (var state in states)
        state.Value.Commit();
      if (Origin.OperationSet!=null)
        Origin.OperationSet.Register(OperationSet);
    }

    public void AddState(DisconnectedEntityState state)
    {
      states.Add(state.Key, state);
    }

    public void OnStateChanged(Key key, Tuple prevValue, Tuple newValue)
    {
      // Inserting
      if (prevValue==null) {
        foreach (var item in ModelHelper.GetEntitySetItems(key, newValue))
          InsertIntoEntitySet(item.OwnerKey, item.Field, item.ItemKey);
        foreach (var reference in ModelHelper.GetReferencesFrom(key, newValue))
          AddReference(reference.TargetKey, reference.Field, reference.ReferencingKey);
      }
      // Deleting
      else if (newValue==null) {
        foreach (var item in ModelHelper.GetEntitySetItems(key, prevValue))
          RemoveFromEntitySet(item.OwnerKey, item.Field, item.ItemKey);
        foreach (var reference in ModelHelper.GetReferencesFrom(key, prevValue))
          RemoveReference(reference.TargetKey, reference.Field, reference.ReferencingKey);
      }
      // Updating
      else {
        foreach (var pair in ModelHelper.GetEntitySets(key.Type)) {
          var prevOwnerKey = ModelHelper.GetKeyFieldValue(pair.First, prevValue);
          var ownerKey = ModelHelper.GetKeyFieldValue(pair.First, newValue);
          if (ownerKey!=prevOwnerKey) {
            if (prevOwnerKey!=null)
              RemoveFromEntitySet(prevOwnerKey, pair.Second, key);
            if (ownerKey!=null)
              InsertIntoEntitySet(ownerKey, pair.Second, key);
          }
        }
        foreach (var field in ModelHelper.GetReferencingFields(key.Type)) {
          var prevOwnerKey = ModelHelper.GetKeyFieldValue(field, prevValue);
          var ownerKey = ModelHelper.GetKeyFieldValue(field, newValue);
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
      var state = GetForUpdate(ownerKey);
      var entitySet = state.GetEntitySetState(field);
      if (!entitySet.Items.ContainsKey(itemKey))
        entitySet.Items.Add(itemKey, itemKey);
    }

    private void RemoveFromEntitySet(Key ownerKey, FieldInfo field, Key itemKey)
    {
      var state = GetForUpdate(ownerKey);
      var entitySet = state.GetEntitySetState(field);
      if (entitySet.Items.ContainsKey(itemKey))
        entitySet.Items.Remove(itemKey);
    }

    private void AddReference(Key targetKey, FieldInfo fieldInfo, Key itemKey)
    {
      var state = GetForUpdate(targetKey);
      var references = state.GetReferences(fieldInfo);
      if (!references.ContainsKey(itemKey))
        references.Add(itemKey, itemKey);
    }

    private void RemoveReference(Key targetKey, FieldInfo fieldInfo, Key itemKey)
    {
      var state = GetForUpdate(targetKey);
      var references = state.GetReferences(fieldInfo);
      if (references.ContainsKey(itemKey))
        references.Remove(itemKey);
    }


    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="disconnectedState">State of the disconnected.</param>
    public StateRegistry(DisconnectedState disconnectedState)
    {
      ArgumentValidator.EnsureArgumentNotNull(disconnectedState, "disconnectedState");

      states = new Dictionary<Key, DisconnectedEntityState>();
      this.disconnectedState = disconnectedState;
      OperationSet = new OperationSet();
    }

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="origin">The origin registry.</param>
    public StateRegistry(StateRegistry origin)
    {
      ArgumentValidator.EnsureArgumentNotNull(origin, "origin");

      states = new Dictionary<Key, DisconnectedEntityState>();
      this.origin = origin;
      disconnectedState = origin.disconnectedState;
      OperationSet = new OperationSet();
    }
  }
}
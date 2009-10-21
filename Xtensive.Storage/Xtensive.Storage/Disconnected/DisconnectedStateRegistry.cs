using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Xtensive.Core;
using Xtensive.Core.Collections;
using Xtensive.Core.Internals.DocTemplates;
using Xtensive.Core.Tuples;
using System.Linq;
using Xtensive.Storage.Internals;
using Xtensive.Storage.Model;

namespace Xtensive.Storage.Disconnected
{
  internal sealed class DisconnectedStateRegistry
  {
    # region Nested types

    private struct EntitySetItem
    {
      public Key OwnerKey { get; private set; }

      public FieldInfo Field { get; private set; }

      public Key ItemKey { get; private set; }

      public EntitySetItem(Key ownerKey, FieldInfo field, Key itemKey)
        : this()
      {
        OwnerKey = ownerKey;
        Field = field;
        ItemKey = itemKey;
      }
    }

    private struct Reference
    {
      public Key TargetKey { get; private set; }

      public FieldInfo Field { get; private set; }

      public Key ItemKey { get; private set; }

      public Reference(Key targetKey, FieldInfo field, Key itemKey)
        : this()
      {
        TargetKey = targetKey;
        Field = field;
        ItemKey = itemKey;
      }
    }

    # endregion

    # region Caches

    private Dictionary<Key, DisconnectedEntityState> entityCache;
    private HashSet<Key> entityChanges;
    private Dictionary<Pair<Key, string>, DisconnectedEntitySetState> entitySetCache;
    private Dictionary<Pair<Key, string>, HashSet<Key>> referenceMapping;

    public bool HasChanges { get { return entityChanges.Count != 0; } }

    public IEnumerable<DisconnectedEntityState> ChangedEntityStates
    {
      get
      {
        foreach (var key in entityChanges)
          yield return entityCache[key];
      }
    }
    
    # endregion

    public DisconnectedEntityState GetEntityState(Key key)
    {
      DisconnectedEntityState state;
      if (entityCache.TryGetValue(key, out state))
        return state;
      return null;
    }
    
    public DisconnectedEntitySetState GetEntitySetState(Key ownerKey, string fieldName)
    {
      DisconnectedEntitySetState state;
      var key = new Pair<Key, string>(ownerKey, fieldName);
      if (entitySetCache.TryGetValue(key, out state))
        return state;

      var ownerState = GetEntityState(ownerKey);
      if (ownerState!=null && ownerState.PersistenceState==PersistenceState.New) {
        var cachedState = new DisconnectedEntitySetState(ownerKey, fieldName);
        cachedState.IsFullyLoaded = true;
        entitySetCache.Add(key, cachedState);
        return cachedState;
      }

      return null;
    }

    public IEnumerable<Key> GetReferencesTo(Key targetKey, string fieldInfo)
    {
      HashSet<Key> state;
      if (referenceMapping.TryGetValue(new Pair<Key, string>(targetKey, fieldInfo), out state))
        return state;
      return null;
    }

    public DisconnectedEntityState RegisterEntityState(Key key, Tuple tuple)
    {
      var entityState = GetEntityState(key);

      // If not exists
      if (entityState==null) {
        entityState = new DisconnectedEntityState(key);
        entityState.OriginalTuple = tuple!=null ? tuple.Clone() : null;
        entityState.PersistenceState = PersistenceState.Synchronized;
        entityCache.Add(key, entityState);
        OnStateAdded(entityState);
        return entityState;
      }

      // If removed
      if (entityState.PersistenceState == PersistenceState.Removed)
        return entityState;

      // If not loaded
      var original = entityState.OriginalTuple;
      if (original==null) {
        entityState.OriginalTuple = tuple!=null ? tuple.Clone() : null;
        return entityState;
      }

      // If fields not loaded
      for (int i = 0; i < original.Count; i++)
        if (!original.GetFieldState(i).IsAvailable() 
          && tuple.GetFieldState(i).IsAvailable())
          original.SetValue(i, tuple.GetValue(i));

      return entityState;
    }

    public DisconnectedEntitySetState RegisterEntitySetState(Key ownerKey, string fieldName, 
      IEnumerable<Key> items, bool isFullyLoaded)
    {
      var cachedState = GetEntitySetState(ownerKey, fieldName);
      if (cachedState==null) {
        cachedState = new DisconnectedEntitySetState(ownerKey, fieldName);
        cachedState.IsFullyLoaded = isFullyLoaded;
        foreach (var item in items)
          cachedState.Items.Add(item);
        entitySetCache.Add(new Pair<Key, string>(ownerKey, fieldName), cachedState);
        return cachedState;
      }
      if (cachedState.IsFullyLoaded)
        return cachedState;
      foreach (var item in items.Except(cachedState.RemovedItems)) {
        var itemState = GetEntityState(item);
        if (itemState==null || itemState.PersistenceState!=PersistenceState.Removed)
          cachedState.Items.Add(item);
      }
      cachedState.IsFullyLoaded = isFullyLoaded;
      return cachedState;
    }

    public DisconnectedEntitySetState RegisterEntitySetState(DisconnectedEntitySetState state)
    {
      var clone = state.Clone();
      entitySetCache.Add(new Pair<Key, string>(state.OwnerKey, state.FieldName), clone);
      return clone;
    }
    
    public DisconnectedEntityState AddEntityState(DisconnectedEntityState state, 
      IEnumerable<DisconnectedEntitySetState> entitySets, IEnumerable<Pair<string, HashSet<Key>>> references)
    {
      var clone = state.Clone();
      clone.PersistenceState = PersistenceState.Synchronized;
      entityCache.Add(state.Key, clone);
      foreach (var entitySet in entitySets)
        entitySetCache.Add(new Pair<Key, string>(entitySet.OwnerKey, entitySet.FieldName), entitySet.Clone());
      foreach (var reference in references)
        referenceMapping.Add(new Pair<Key, string>(state.Key, reference.First), reference.Second.ToHashSet());
      return clone;
    }

    public IEnumerable<DisconnectedEntitySetState> GetEntitySetStates(Key ownerKey)
    {
      return entitySetCache
        .Where(pair => pair.Key.First==ownerKey)
        .Select(pair => pair.Value);
    }

    public IEnumerable<Pair<string, HashSet<Key>>> GetReferences(Key targetKey)
    {
      return referenceMapping
        .Where(pair => pair.Key.First==targetKey)
        .Select(pair => new Pair<string, HashSet<Key>>(pair.Key.Second, pair.Value));
    }

    public void Insert(Key key, Tuple tuple)
    {
      var entityState = GetEntityState(key);
      if (entityState != null)
        throw DuplicateKeyException(key);

      var state = new DisconnectedEntityState(key);
      state.OriginalTuple = tuple.Clone();
      state.PersistenceState = PersistenceState.New;
      entityCache.Add(key, state);
      OnStateAdded(state);
      entityChanges.Add(key);
    }

    public void Remove(Key key)
    {
      var entityState = GetEntityState(key);
      if (entityState==null)
        throw KeyNotFoundException(key);
      if (entityState.PersistenceState==PersistenceState.Removed)
        throw EntityIsRemovedException(key);
      OnStateRemoved(entityState);
      if (entityState.PersistenceState==PersistenceState.New) {
        entityCache.Remove(key);
        entityChanges.Remove(key);
        return;
      }
      entityState.PersistenceState = PersistenceState.Removed;
      entityState.OriginalTuple = null;
      entityState.DifferenceTuple = null;
      entityChanges.Add(key);
    }

    public void Update(Key key, Tuple difference)
    {
      var entityState = GetEntityState(key);
      if (entityState == null)
        throw KeyNotFoundException(key);
      if (entityState.PersistenceState == PersistenceState.Removed)
        throw EntityIsRemovedException(key);
      var prevValue = entityState.OriginalTuple.Clone();
      if (entityState.PersistenceState != PersistenceState.New)
        entityState.PersistenceState = PersistenceState.Modified;
      entityState.OriginalTuple.MergeWith(difference, MergeBehavior.PreferDifference);
      if (entityState.DifferenceTuple==null)
        entityState.DifferenceTuple = difference.Clone();
      else
        entityState.DifferenceTuple.MergeWith(difference, MergeBehavior.PreferDifference);
      entityChanges.Add(key);
      OnStateChanged(entityState, prevValue);
    }

    private void InsertIntoEntitySet(Key ownerKey, string fieldName, Key itemKey)
    {
      DisconnectedEntitySetState state;
      var key = new Pair<Key, string>(ownerKey, fieldName);
      if (!entitySetCache.TryGetValue(key, out state))
        state = RegisterEntitySetState(ownerKey, fieldName, Enumerable.Empty<Key>(), false);
      state.Items.Add(itemKey);
      if (state.RemovedItems.Contains(itemKey))
        state.RemovedItems.Remove(itemKey);
    }

    private void RemoveFromEntitySet(Key ownerKey, string fieldName, Key itemKey)
    {
      DisconnectedEntitySetState state;
      var key = new Pair<Key, string>(ownerKey, fieldName);
      if (!entitySetCache.TryGetValue(key, out state))
        state = RegisterEntitySetState(ownerKey, fieldName, Enumerable.Empty<Key>(), false);
      state.Items.Remove(itemKey);
      state.RemovedItems.Add(itemKey);
    }

    private void AddReference(Key targetKey, string fieldInfo, Key itemKey)
    {
      HashSet<Key> state;
      var key = new Pair<Key, string>(targetKey, fieldInfo);
      if (referenceMapping.TryGetValue(key, out state))
        state.Add(itemKey);
      else
        referenceMapping.Add(key, new HashSet<Key> {itemKey});
    }

    private void RemoveReference(Key targetKey, string fieldInfo, Key itemKey)
    {
      HashSet<Key> state;
      var key = new Pair<Key, string>(targetKey, fieldInfo);
      if (referenceMapping.TryGetValue(key, out state))
        state.Remove(itemKey);
    }

    private void OnStateAdded(DisconnectedEntityState state)
    {
      if (state.OriginalTuple==null)
        return;
      foreach (var item in GetEntitySetItems(state.Key, state.OriginalTuple))
        InsertIntoEntitySet(item.OwnerKey, item.Field.Name, item.ItemKey);
      foreach (var reference in GetReferences(state.Key, state.OriginalTuple))
        AddReference(reference.TargetKey, reference.Field.Name, reference.ItemKey);
    }

    private void OnStateChanged(DisconnectedEntityState state, Tuple previousValue)
    {
      if (state.OriginalTuple==null)
        return;
      var baseType = state.Type.UnderlyingType.BaseType;
      var isAuxEntity = baseType.IsGenericType && baseType.GetGenericTypeDefinition()==typeof (EntitySetItem<,>);
      if (isAuxEntity)
        return;
      // Add/remove to entity sets
      foreach (var pair in GetEntitySetFields(state.Type)) {
        var prevOwnerKey = GetKeyFieldValue(pair.First, previousValue);
        var ownerKey = GetKeyFieldValue(pair.First, state.OriginalTuple);
        if (ownerKey!=prevOwnerKey) {
          if (prevOwnerKey!=null)
            RemoveFromEntitySet(prevOwnerKey, pair.Second.Name, state.Key);
          if (ownerKey!=null)
            InsertIntoEntitySet(ownerKey, pair.Second.Name, state.Key);
        }
      }
      // Add/remove to reference mappings
      foreach (var field in GetReferencingFields(state.Type)) {
        var prevOwnerKey = GetKeyFieldValue(field, previousValue);
        var ownerKey = GetKeyFieldValue(field, state.OriginalTuple);
        if (ownerKey!=prevOwnerKey) {
          if (prevOwnerKey!=null)
            RemoveReference(prevOwnerKey, field.Name, state.Key);
          if (ownerKey!=null)
            AddReference(ownerKey, field.Name, state.Key);
        }
      }
    }

    private void OnStateRemoved(DisconnectedEntityState state)
    {
      if (state.OriginalTuple==null)
        return;
      foreach (var item in GetEntitySetItems(state.Key, state.OriginalTuple))
        RemoveFromEntitySet(item.OwnerKey, item.Field.Name, item.ItemKey);
      foreach (var reference in GetReferences(state.Key, state.OriginalTuple))
        RemoveReference(reference.TargetKey, reference.Field.Name, reference.ItemKey);
    }
    
    # region Static helpers

    private static IEnumerable<EntitySetItem> GetEntitySetItems(Key key, Tuple tuple)
    {
      var type = key.TypeRef.Type;
      var baseType = type.UnderlyingType.BaseType;
      var isAuxEntity = baseType.IsGenericType && baseType.GetGenericTypeDefinition()==typeof (EntitySetItem<,>);
      if (isAuxEntity) {
        var association = type.Model.Associations.First(a => a.AuxiliaryType==type);
        var masterField = type.Fields[WellKnown.MasterFieldName];
        var slaveField = type.Fields[WellKnown.SlaveFieldName];
        var ownerKey = GetKeyFieldValue(masterField, tuple);
        var itemKey = GetKeyFieldValue(slaveField, tuple);
        if (ownerKey!=null && itemKey!=null) {
          if (association.Multiplicity==Multiplicity.ManyToMany)
            yield return new EntitySetItem(itemKey, association.OwnerField, ownerKey);
          yield return new EntitySetItem(ownerKey, association.Master.OwnerField, itemKey);
        }
      }
      else {
        foreach (var pair in GetEntitySetFields(type)) {
          var ownerKey = GetKeyFieldValue(pair.First, tuple);
          if (ownerKey!=null)
            yield return new EntitySetItem(ownerKey, pair.Second, key);
        }
      }
    }

    private static IEnumerable<Reference> GetReferences(Key key, Tuple tuple)
    {
      var type = key.TypeRef.Type;
      var baseType = type.UnderlyingType.BaseType;
      var isAuxEntity = baseType.IsGenericType && baseType.GetGenericTypeDefinition()==typeof (EntitySetItem<,>);
      if (isAuxEntity) {
        var association = type.Model.Associations.First(a => a.AuxiliaryType==type);
        if (association.Multiplicity==Multiplicity.ZeroToMany) {
          var field = association.Master.OwnerField;
          var masterField = type.Fields[WellKnown.MasterFieldName];
          var slaveField = type.Fields[WellKnown.SlaveFieldName];
          var ownerKey = GetKeyFieldValue(masterField, tuple);
          var itemKey = GetKeyFieldValue(slaveField, tuple);
          if (ownerKey!=null && itemKey!=null)
            yield return new Reference(itemKey, association.OwnerField, ownerKey);
        }
      }
      else {
        foreach (var field in GetReferencingFields(type)) {
          var ownerKey = GetKeyFieldValue(field, tuple);
          if (ownerKey!=null)
            yield return new Reference(ownerKey, field, key);
        }
      }
    }

    private static IEnumerable<Pair<FieldInfo>> GetEntitySetFields(TypeInfo typeInfo)
    {
      var associations = typeInfo.GetOwnerAssociations();
      return associations
        .Where(association =>
          association.Multiplicity==Multiplicity.ManyToOne)
        .Select(association => new Pair<FieldInfo>(association.OwnerField, association.Reversed.OwnerField));
    }

    private static Key GetKeyFieldValue(FieldInfo field, Tuple tuple)
    {
      if (!field.IsEntity)
        throw new InvalidOperationException();

      var types = Session.Current.Domain.Model.Types;
      var type = types[field.ValueType];
      if (tuple.ContainsEmptyValues(field.MappingInfo))
        return null;
      int typeIdFieldIndex = type.Hierarchy.KeyProviderInfo.TypeIdColumnIndex;
      bool exactType = typeIdFieldIndex >= 0;
      var keyValue = field.ExtractValue(tuple);
      if (exactType) {
        int typeId = keyValue.GetValueOrDefault<int>(typeIdFieldIndex);
        if (typeId!=TypeInfo.NoTypeId) // != default(int) != 0
          type = types[typeId];
        else
          // This may happen if referense is null
          exactType = false;
      }
      var key = Key.Create(type, keyValue);
      return key;
    }

    private static IEnumerable<FieldInfo> GetReferencingFields(TypeInfo typeInfo)
    {
      return typeInfo.GetOwnerAssociations()
        .Where(association => association.Multiplicity==Multiplicity.ZeroToOne
          || association.Multiplicity==Multiplicity.ManyToOne)
        .Select(association => association.OwnerField);
    }

    private static InvalidOperationException KeyNotFoundException(Key key)
    {
      return new InvalidOperationException(string.Format(
        "Entity with key {0} is not found.", key));
    }

    private static InvalidOperationException EntityIsRemovedException(Key key)
    {
      return new InvalidOperationException(string.Format(
        "Entity with key {0} is already removed.", key));
    }

    private static InvalidOperationException DuplicateKeyException(Key key)
    {
      return new InvalidOperationException(string.Format(
        "Entity with key {0} is already exists.", key));
    }

    # endregion
    
    // Contrustors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    public DisconnectedStateRegistry()
    {
      entityCache = new Dictionary<Key, DisconnectedEntityState>();
      entityChanges = new HashSet<Key>();
      entitySetCache = new Dictionary<Pair<Key, string>, DisconnectedEntitySetState>();
      referenceMapping = new Dictionary<Pair<Key, string>, HashSet<Key>>();
    }
  }
}
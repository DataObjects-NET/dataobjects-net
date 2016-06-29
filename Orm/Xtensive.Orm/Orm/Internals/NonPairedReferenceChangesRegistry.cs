// Copyright (C) 2016 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kulakov
// Created:    2016.06.21

using System;
using System.Collections.Generic;
using System.Linq;
using Xtensive.Collections;
using Xtensive.Core;
using Xtensive.Orm.Model;

namespace Xtensive.Orm.Internals
{
  internal sealed class NonPairedReferenceChangesRegistry : SessionBound
  {
    private class Identifier : IEquatable<Identifier>
    {
      public AssociationInfo Association { get; private set; }

      public EntityState EntityState { get; private set; }

      public bool Equals(Identifier other)
      {
        if (ReferenceEquals(other, null))
          return false;
        if (ReferenceEquals(this, other))
          return true;
        if (!Association.Equals(other.Association))
          return false;
        if (!EntityState.Equals(other.EntityState))
          return false;
        return true;
      }

      public override bool Equals(object obj)
      {
        return Equals(obj as Identifier);
      }

      public override int GetHashCode()
      {
        unchecked {
          int hash = 139;
          hash = hash * 311 + EntityState.GetHashCode();
          hash = hash * 311 + Association.GetHashCode();
          return hash;
        }
      }

      public Identifier(EntityState entityState, AssociationInfo association)
      {
        EntityState = entityState;
        Association = association;
      }
    }

    private readonly IDictionary<Identifier, List<EntityState>> removedReferences = new Dictionary<Identifier, List<EntityState>>();
    private readonly IDictionary<Identifier, List<EntityState>> addedReferences = new Dictionary<Identifier, List<EntityState>>();
    private readonly object accessGuard = new object();

    public int RemovedReferencesCount { get { return removedReferences.Values.Sum(el => el.Count); } }

    public int AddedReferencesCount { get { return addedReferences.Values.Sum(el => el.Count); } }

    public IEnumerable<EntityState> GetRemovedReferencesTo(EntityState target, AssociationInfo association)
    {
      ArgumentValidator.EnsureArgumentNotNull(target, "target");
      ArgumentValidator.EnsureArgumentNotNull(association, "association");

      if (association.IsPaired)
        return Enumerable.Empty<EntityState>();

      var key = MakeKey(target, association);
      List<EntityState> removedMap;
      if (removedReferences.TryGetValue(key, out removedMap))
        return removedMap;
      return EnumerableUtils<EntityState>.Empty;
    }

    public IEnumerable<EntityState> GetAddedReferenceTo(EntityState target, AssociationInfo association)
    {
      ArgumentValidator.EnsureArgumentNotNull(target, "target");
      ArgumentValidator.EnsureArgumentNotNull(association, "association");

      if (association.IsPaired)
        return Enumerable.Empty<EntityState>();

      var key = MakeKey(target, association);
      List<EntityState> removedMap;
      if (addedReferences.TryGetValue(key, out removedMap))
        return removedMap;
      return EnumerableUtils<EntityState>.Empty;
    }

    public void Invalidate()
    {
      Clear();
    }

    public void Clear()
    {
      lock (accessGuard) {
        removedReferences.Clear();
        addedReferences.Clear();
      }
    }

    private void RegisterChange(EntityState referencedState, EntityState referencingState, EntityState noLongerReferencedState, AssociationInfo association)
    {
      ArgumentValidator.EnsureArgumentNotNull(association, "association");
      ArgumentValidator.EnsureArgumentNotNull(referencingState, "referencingState");
      if (!Session.DisableAutoSaveChanges)
        return;

      if (association.IsPaired)
        return;

      if (referencedState==null && noLongerReferencedState==null)
        return;
      if (referencedState!=null && noLongerReferencedState!=null) {
        var oldKey = MakeKey(noLongerReferencedState, association);
        var newKey = MakeKey(referencedState, association);
        RegisterRemoveInternal(oldKey, referencingState);
        RegisterAddInternal(newKey, referencingState);
      }
      else if (noLongerReferencedState!=null) {
        var oldKey = MakeKey(noLongerReferencedState, association);
        RegisterRemoveInternal(oldKey, referencingState);
      }
      else {
        var newKey = MakeKey(referencedState, association);
        RegisterAddInternal(newKey, referencingState);
      }
    }

    private void RegisterRemoveInternal(Identifier oldKey, EntityState referencingState)
    {
      List<EntityState> references;
      if (addedReferences.TryGetValue(oldKey, out references)) {
        if (references.Remove(referencingState)) {
          if (references.Count==0)
            addedReferences.Remove(oldKey);
          return;
        }
      }
      if (removedReferences.TryGetValue(oldKey, out references)) {
        if (references.Contains(referencingState))
          throw new InvalidOperationException(Strings.ExReferenceRregistrationErrorReferenceRemovalIsAlreadyRegistered);
        references.Add(referencingState);
        return;
      }
      removedReferences.Add(oldKey, new List<EntityState>{referencingState});
    }

    private void RegisterAddInternal(Identifier newKey, EntityState referencingState)
    {
      List<EntityState> references;
      if (removedReferences.TryGetValue(newKey, out references)) {
        if (references.Remove(referencingState))
          if (references.Count==0)
            removedReferences.Remove(newKey);
        return;
      }
      if (addedReferences.TryGetValue(newKey, out references)) {
        if (references.Contains(referencingState))
          throw new InvalidOperationException(Strings.ExReferenceRegistrationErrorReferenceAdditionIsAlreadyRegistered);
        references.Add(referencingState);
        return;
      }
      addedReferences.Add(newKey, new List<EntityState>{referencingState});
    }

    private Identifier MakeKey(EntityState state, AssociationInfo association)
    {
      return new Identifier(state, association);
    }

    private void Initialize()
    {
      Session.SystemEvents.EntityFieldValueSetCompleted += OnEntityFieldValueSetCompleted;
      Session.SystemEvents.EntitySetItemAddCompleted += OnEntitySetItemAddCompleted;
      Session.SystemEvents.EntitySetItemRemoveCompleted += OnEntitySetItemRemoveCompleted;
      Session.SystemEvents.Persisted += OnSessionPersisted;
    }

    private void OnSessionPersisted(object sender, EventArgs e)
    {
      Invalidate();
    }

    private void OnEntitySetItemRemoveCompleted(object sender, EntitySetItemActionCompletedEventArgs e)
    {
      if (ShouldSkipRegistration(e))
        return;
      RegisterChange(null, e.Entity.State, e.Item.State, e.Field.Associations.First());
    }

    private void OnEntitySetItemAddCompleted(object sender, EntitySetItemActionCompletedEventArgs e)
    {
      if (ShouldSkipRegistration(e))
        return;
      RegisterChange(e.Item.State, e.Entity.State, null, e.Field.Associations.First());
    }

    private void OnEntityFieldValueSetCompleted(object sender, EntityFieldValueSetCompletedEventArgs e)
    {
      if (ShouldSkipRegistration(e))
        return;
      if (e.Field.IsStructure)
        HandleStructureValues(e.Entity, e.Field, (Structure) e.OldValue, (Structure) e.NewValue);
      else
        HandleEntityValues(e.Entity, e.Field, (Entity) e.OldValue, (Entity) e.NewValue);
    }

    private void HandleStructureValues(Entity owner, FieldInfo fieldOfOwner, Structure oldValue, Structure newValue)
    {
      var referenceFields = oldValue.TypeInfo.Fields.Where(f => f.IsEntity).Union(oldValue.TypeInfo.StructureFieldMapping.Values.Where(f => f.IsEntity));

      foreach (var referenceFieldOfStructure in referenceFields) {
        var realField = owner.TypeInfo.Fields[BuildNameOfEntityField(fieldOfOwner, referenceFieldOfStructure)];
        if (realField.Associations.Count>1)
          continue;
        var realAssociation = realField.Associations.First();
        var oldFieldValue = GetStructureFieldValue(referenceFieldOfStructure, oldValue);
        var newFieldValue = GetStructureFieldValue(referenceFieldOfStructure, newValue);
        RegisterChange(newFieldValue, owner.State, oldFieldValue, realAssociation);
      }
    }

    private void HandleEntityValues(Entity owner, FieldInfo field, Entity oldValue, Entity newValue)
    {
      var oldEntityState = (oldValue!=null) ? oldValue.State : null;
      var newEntityState = (newValue!=null) ? newValue.State : null;
      var association = field.GetAssociation((oldValue ?? newValue).TypeInfo);
      RegisterChange(newEntityState, owner.State, oldEntityState, association);
    }

    private bool ShouldSkipRegistration(EntityFieldValueSetCompletedEventArgs e)
    {
      if (!Session.DisableAutoSaveChanges)
        return true;
      if (Session.IsPersisting)
        return true;
      if (e.Exception!=null)
        return true;
      if (!e.Field.IsEntity && !e.Field.IsStructure)
        return true;
      if (e.Field.IsEntity && e.Field.Associations.First().IsPaired)
        return true;
      if (e.NewValue==null && e.OldValue==null)
        return true;
      return false;
    }

    private bool ShouldSkipRegistration(EntitySetItemActionCompletedEventArgs e)
    {
      if (!Session.DisableAutoSaveChanges)
        return true;
      if (Session.IsPersisting)
        return true;
      if (e.Exception!=null)
        return true;
      if (e.Field.Associations.First().IsPaired)
        return true;
      return false;
    }

    private EntityState GetStructureFieldValue(FieldInfo fieldOfStructure, Structure structure)
    {
      EntityState value;
      if (structure==null)
        value = null;
      else {
        var accessor = structure.GetFieldAccessor(fieldOfStructure);
        var entity = (Entity) accessor.GetUntypedValue(structure);
        value = (entity!=null) ? entity.State : null;
      }
      return value;
    }

    private string BuildNameOfEntityField(FieldInfo fieldOfOwner, FieldInfo referenceFieldOfStructure)
    {
      var name = string.Format("{0}.{1}", fieldOfOwner.Name, referenceFieldOfStructure.Name);
      return name;
    }

    internal NonPairedReferenceChangesRegistry(Session session)
      : base(session)
    {
      Initialize();
    }
  }
}
// Copyright (C) 2016-2025 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Alexey Kulakov
// Created:    2016.06.21

using System;
using System.Collections.Generic;
using System.Linq;
using Xtensive.Core;
using Xtensive.Orm.Model;

namespace Xtensive.Orm.Internals
{
  internal sealed class NonPairedReferenceChangesRegistry : SessionBoundRegistry
  {
    private readonly struct Identifier : IEquatable<Identifier>
    {
      public readonly AssociationInfo Association;
      public readonly EntityState EntityState;

      public static Identifier Make(EntityState state, AssociationInfo association) => new(state, association);

      public bool Equals(Identifier other) =>
        Association.Equals(other.Association) && EntityState.Equals(other.EntityState);

      public override bool Equals(object obj) => Equals((Identifier) obj);

      public override int GetHashCode()
      {
        unchecked {
          var hash = 139;
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

    private readonly IDictionary<Identifier, HashSet<EntityState>> removedReferences = new Dictionary<Identifier, HashSet<EntityState>>();
    private readonly IDictionary<Identifier, HashSet<EntityState>> addedReferences = new Dictionary<Identifier, HashSet<EntityState>>();
    private readonly object accessGuard = new();

    public int RemovedReferencesCount => removedReferences.Values.Sum(el => el.Count);

    public int AddedReferencesCount => addedReferences.Values.Sum(el => el.Count);

    public IEnumerable<EntityState> GetRemovedReferencesTo(EntityState target, AssociationInfo association)
    {
      ArgumentValidator.EnsureArgumentNotNull(target, "target");
      ArgumentValidator.EnsureArgumentNotNull(association, "association");

      return association.IsPaired || !removedReferences.TryGetValue(Identifier.Make(target, association), out var removedMap)
        ? Array.Empty<EntityState>()
        : removedMap;
    }

    public IEnumerable<EntityState> GetAddedReferenceTo(EntityState target, AssociationInfo association)
    {
      ArgumentValidator.EnsureArgumentNotNull(target, "target");
      ArgumentValidator.EnsureArgumentNotNull(association, "association");

      return association.IsPaired || !addedReferences.TryGetValue(Identifier.Make(target, association), out var removedMap)
        ? Array.Empty<EntityState>()
        : removedMap;
    }

    public void Invalidate() => Clear();

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

      if (!Session.DisableAutoSaveChanges || association.IsPaired
        || (referencedState == null && noLongerReferencedState == null)) {
        return;
      }

      if (referencedState!=null && noLongerReferencedState!=null) {
        var oldKey = Identifier.Make(noLongerReferencedState, association);
        var newKey = Identifier.Make(referencedState, association);
        RegisterRemoveInternal(oldKey, referencingState);
        RegisterAddInternal(newKey, referencingState);
      }
      else if (noLongerReferencedState!=null) {
        var oldKey = Identifier.Make(noLongerReferencedState, association);
        RegisterRemoveInternal(oldKey, referencingState);
      }
      else {
        var newKey = Identifier.Make(referencedState, association);
        RegisterAddInternal(newKey, referencingState);
      }
    }

    private void RegisterRemoveInternal(Identifier oldKey, EntityState referencingState)
    {
      if (addedReferences.TryGetValue(oldKey, out var addedRefs)) {
        EnsureRegistrationsAllowed();
        if (addedRefs.Remove(referencingState)) {
          if (addedRefs.Count == 0) {
            _ = addedReferences.Remove(oldKey);
          }
          return;
        }
      }
      if (removedReferences.TryGetValue(oldKey, out var renivedRefs)) {
        if (!renivedRefs.Add(referencingState)) {
          throw new InvalidOperationException(Strings.ExReferenceRregistrationErrorReferenceRemovalIsAlreadyRegistered);
        }
        return;
      }
      EnsureRegistrationsAllowed();
      removedReferences.Add(oldKey, new HashSet<EntityState>{referencingState});
    }

    private void RegisterAddInternal(Identifier newKey, EntityState referencingState)
    {
      if (removedReferences.TryGetValue(newKey, out var removedRefs)) {
        EnsureRegistrationsAllowed();
        if (removedRefs.Remove(referencingState)) {
          if (removedRefs.Count == 0) {
            _ = removedReferences.Remove(newKey);
          }
        }
        return;
      }
      if (addedReferences.TryGetValue(newKey, out var addedRefs)) {
        if (!addedRefs.Add(referencingState)) {
          throw new InvalidOperationException(Strings.ExReferenceRegistrationErrorReferenceAdditionIsAlreadyRegistered);
        }
        return;
      }
      EnsureRegistrationsAllowed();
      addedReferences.Add(newKey, new HashSet<EntityState>{referencingState});
    }

    private void Initialize()
    {
      Session.SystemEvents.EntityFieldValueSetCompleted += OnEntityFieldValueSetCompleted;
      Session.SystemEvents.EntitySetItemAddCompleted += OnEntitySetItemAddCompleted;
      Session.SystemEvents.EntitySetItemRemoveCompleted += OnEntitySetItemRemoveCompleted;
      Session.SystemEvents.Persisted += OnSessionPersisted;
    }

    private void OnSessionPersisted(object sender, EventArgs e) => Invalidate();

    private void OnEntitySetItemRemoveCompleted(object sender, EntitySetItemActionCompletedEventArgs e)
    {
      if (ShouldSkipRegistration(e)) {
        return;
      }

      RegisterChange(null, e.Entity.State, e.Item.State, e.Field.Associations.First());
    }

    private void OnEntitySetItemAddCompleted(object sender, EntitySetItemActionCompletedEventArgs e)
    {
      if (ShouldSkipRegistration(e)) {
        return;
      }

      RegisterChange(e.Item.State, e.Entity.State, null, e.Field.Associations.First());
    }

    private void OnEntityFieldValueSetCompleted(object sender, EntityFieldValueSetCompletedEventArgs e)
    {
      if (ShouldSkipRegistration(e)) {
        return;
      }
      if (e.Field.IsStructure) {
        HandleStructureValues(e.Entity, e.Field, (Structure) e.OldValue, (Structure) e.NewValue);
      }
      else {
        HandleEntityValues(e.Entity, e.Field, (Entity) e.OldValue, (Entity) e.NewValue);
      }
    }

    private void HandleStructureValues(Entity owner, FieldInfo fieldOfOwner, Structure oldValue, Structure newValue)
    {
      var referenceFields = oldValue.TypeInfo.Fields.Where(f => f.IsEntity)
        .Union(oldValue.TypeInfo.StructureFieldMapping.Values.Where(f => f.IsEntity));

      foreach (var referenceFieldOfStructure in referenceFields) {
        var realField = owner.TypeInfo.Fields[BuildNameEntityFieldName(fieldOfOwner, referenceFieldOfStructure)];
        if (realField.Associations.Count > 1) {
          continue;
        }

        var realAssociation = realField.Associations.First();
        var oldFieldValue = GetStructureFieldValue(referenceFieldOfStructure, oldValue);
        var newFieldValue = GetStructureFieldValue(referenceFieldOfStructure, newValue);
        RegisterChange(newFieldValue, owner.State, oldFieldValue, realAssociation);
      }
    }

    private void HandleEntityValues(Entity owner, FieldInfo field, Entity oldValue, Entity newValue)
    {
      var association = field.GetAssociation((oldValue ?? newValue).TypeInfo);
      RegisterChange(newValue?.State, owner.State, oldValue?.State, association);
    }

    private bool ShouldSkipRegistration(EntityFieldValueSetCompletedEventArgs e) =>
      !Session.DisableAutoSaveChanges || Session.IsPersisting
        || e.Exception != null
        || (!e.Field.IsEntity && !e.Field.IsStructure)
        || (e.Field.IsEntity && e.Field.Associations.First().IsPaired)
        || (e.NewValue == null && e.OldValue == null);
    

    private bool ShouldSkipRegistration(EntitySetItemActionCompletedEventArgs e) =>
      !Session.DisableAutoSaveChanges || Session.IsPersisting
        || e.Exception != null
        || e.Field.Associations.First().IsPaired;

    private EntityState GetStructureFieldValue(FieldInfo fieldOfStructure, Structure structure)
    {
      if (structure == null) {
        return null;
      }
      var accessor = structure.GetFieldAccessor(fieldOfStructure);
      var entity = (Entity) accessor.GetUntypedValue(structure);
      return entity?.State;
    }

    private string BuildNameEntityFieldName(FieldInfo fieldOfOwner, FieldInfo referenceFieldOfStructure) =>
      $"{fieldOfOwner.Name}.{referenceFieldOfStructure.Name}";


    internal NonPairedReferenceChangesRegistry(Session session)
      : base(session)
    {
      Initialize();
    }
  }
}
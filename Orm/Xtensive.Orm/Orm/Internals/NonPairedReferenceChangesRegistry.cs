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
      ArgumentValidator.EnsureArgumentNotNull(association, "");

      if (association.IsPaired)
        return Enumerable.Empty<EntityState>();

      var key = MakeKey(target, association);
      List<EntityState> removedMap;
      if (addedReferences.TryGetValue(key, out removedMap))
        return removedMap;
      return EnumerableUtils<EntityState>.Empty;
    }

    public void RegisterChange(EntityState referencedState, EntityState referencingState, EntityState noLongerReferencedState, AssociationInfo association)
    {
      ArgumentValidator.EnsureArgumentNotNull(association, "association");
      ArgumentValidator.EnsureArgumentNotNull(referencingState, "referencingState");

      if (association.IsPaired)
        return;

      if(referencedState==null && noLongerReferencedState==null)
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
          throw new InvalidOperationException("Reference registration error: Unable to register reference remove twice.");
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
          throw new InvalidOperationException("Reference registration error: Unable to register new reference twice.");
        references.Add(referencingState);
        return;
      }
      addedReferences.Add(newKey, new List<EntityState>{referencingState});
    }

    private Identifier MakeKey(EntityState state, AssociationInfo association)
    {
      return new Identifier(state, association);
    }

    internal NonPairedReferenceChangesRegistry(Session session)
      : base(session)
    {
    }
  }
}
// Copyright (C) 2014 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kulakov
// Created:    2014.05.06

using System;
using System.Linq;
using Xtensive.Tuples.Transform;

namespace Xtensive.Orm.Internals
{
  internal class KeyRemapper : SessionBound
  {
    private RemapContext context;
    
    /// <summary>
    /// Remap temporary (local) keys to real (will be saved to storage) keys.
    /// </summary>
    /// <param name="registry">Registry that contains changed <see cref="EntityState">states of entity.</see></param>
    /// <returns>Mapping temporary keys to real keys.</returns>
    public KeyMapping Remap(EntityChangeRegistry registry)
    {
      var context = new RemapContext(registry);
      RemapEntityKeys(context);
      RemapReferencesToEntities(context);
      Session.ReferenceFieldsChangesRegistry.Clear();
      return context.KeyMapping;
    }
    
    private void RemapEntityKeys(RemapContext context)
    {
      foreach (var entityState in context.EntitiesToRemap.Where(el => el.Key.IsTemporary(Session.Domain))) {
        var newKey = Key.Generate(Session, entityState.Entity.TypeInfo);
        context.RegisterKeyMap(entityState.Key, newKey);
      }
    }

    private void RemapReferencesToEntities(RemapContext context)
    {
      foreach (var setInfo in Session.ReferenceFieldsChangesRegistry.GetItems())
        if (setInfo.Field.IsEntitySet)
          RemapEntitySetReference(context, setInfo);
        else
          RemapEntityReference(context, setInfo);
    }

    private void RemapEntitySetReference(RemapContext context, ReferenceFieldChangeInfo info)
    {
      var fieldAssociation = info.Field.GetAssociation(info.FieldValue.TypeInfo);
      if (!fieldAssociation.IsMaster && fieldAssociation.IsPaired)
        return;

      var oldCombinedKey = info.AuxiliaryEntity;

      var fieldOwnerKey = context.TryRemapKey(info.FieldOwner);
      var fieldValueKey = context.TryRemapKey(info.FieldValue);

      var transformer = new CombineTransform(false, fieldOwnerKey.Value.Descriptor, fieldValueKey.Value.Descriptor);
      var combinedTuple = transformer.Apply(TupleTransformType.Tuple, fieldOwnerKey.Value, fieldValueKey.Value);

      var newCombinedKey = Key.Create(Session.Domain, Session.StorageNodeId, fieldAssociation.AuxiliaryType, TypeReferenceAccuracy.ExactType, combinedTuple);
      context.RegisterKeyMap(oldCombinedKey, newCombinedKey);
    }

    private void RemapEntityReference(RemapContext context, ReferenceFieldChangeInfo info)
    {
      var entity = Session.Query.SingleOrDefault(info.FieldOwner);
      if (entity==null)
        return;
      var value = context.TryRemapKey(info.FieldValue);
      entity.SetReferenceKey(info.Field, value);
    }

    public KeyRemapper(Session session)
      :base(session)
    {
    }
  }
}

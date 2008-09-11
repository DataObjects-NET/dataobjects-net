// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2008.05.26

using System;
using Xtensive.Core;
using Xtensive.Core.Tuples;
using Xtensive.Core.Tuples.Transform;
using Xtensive.Storage.Model;
using Xtensive.Storage.Resources;

namespace Xtensive.Storage.Internals
{
  internal class EntityFieldAccessor<T> : FieldAccessorBase<T>
  {
    private static readonly FieldAccessorBase<T> instance = new EntityFieldAccessor<T>();

    public static FieldAccessorBase<T> Instance
    {
      get { return instance; }
    }

    public override void SetValue(Persistent obj, FieldInfo field, T value)
    {
      var entity = value as Entity;

      if (!ReferenceEquals(value, null) && entity==null)
        throw new InvalidOperationException(
          string.Format(Strings.ExValueShouldBeXDescendant, typeof (Entity)));

      if (entity!=null && entity.Session!=obj.Session)
        throw new InvalidOperationException(
          string.Format(Strings.EntityXIsBoundToAnotherSession, entity.Key));


      AssociationInfo association = field.Association;
      Key originalKey = association==null ? null : GetKey(field, obj);

      if (entity==null)
        for (int i = field.MappingInfo.Offset; i < field.MappingInfo.Offset + field.MappingInfo.Length; i++)
          obj.Tuple.SetValue(i, null);
      else {
        ValidateType(field);
        entity.Key.Tuple.CopyTo(obj.Tuple, 0, field.MappingInfo.Offset, field.MappingInfo.Length);
      }

      if (association!=null)
        ProcessAssociation(obj, field, entity, originalKey);
    }

    private void ProcessAssociation(Persistent obj, FieldInfo field, Entity newValue, Key originalKey)
    {
      AssociationInfo association = field.Association;
      AssociationInfo pairedAssociation = association.PairTo;
      Key newKey = newValue == null ? null : newValue.Key;
      if (!ReferenceEquals(originalKey, newKey)) {
        switch (association.Multiplicity) {
        case Multiplicity.OneToZero:
          // Do nothing.
          break;
        case Multiplicity.OneToOne:
          var pairedAccessor = pairedAssociation.ReferencingField.GetAccessor<Entity>();
          if (!ReferenceEquals(originalKey, null)) {
            var originalValue = originalKey.Resolve();
            pairedAccessor.SetValue(originalValue, pairedAssociation.ReferencingField, null);
          }
          if (!ReferenceEquals(newValue, null)) {
            pairedAccessor.SetValue(newValue, pairedAssociation.ReferencingField, (Entity) obj);
          }
          break;
        case Multiplicity.OneToMany:
          if (IsResolved(obj.Session, originalKey)) {
            var originalValue = originalKey.Resolve();
            var entitySetFieldAccessor = pairedAssociation.ReferencingField.GetAccessor<EntitySet>();
            entitySetFieldAccessor.GetValue(originalValue, pairedAssociation.ReferencingField).RemoveFromCache(((Entity)obj).Key);
          }
          if (IsResolved(obj.Session, newKey)) {
            var entitySetFieldAccessor = pairedAssociation.ReferencingField.GetAccessor<EntitySet>();
            entitySetFieldAccessor.GetValue(newValue, pairedAssociation.ReferencingField).AddToCache(((Entity)obj).Key);
          }
          break;
        default:
          throw new InvalidOperationException(String.Format(Strings.ExAssociationMultiplicityIsNotValidForField, association.Multiplicity, field.Name));
        }
      }
    }

    public override T GetValue(Persistent obj, FieldInfo field)
    {
      ValidateType(field);
      Key key = GetKey(field, obj);
      if (key==null)
        return default(T);
      return (T) (object) key.Resolve();
    }

    private bool IsResolved(Session session, Key key)
    {
      return key!=null && session.DataCache[key]!=null;
    }

    private static Key GetKey(FieldInfo field, Persistent obj)
    {
      return obj.Session.Domain.KeyManager.Get(field, new SegmentTransform(false, obj.Tuple.Descriptor, new Segment<int>(field.MappingInfo.Offset, field.MappingInfo.Length)).Apply(TupleTransformType.TransformedTuple, obj.Tuple));
    }


    // Constructors

    private EntityFieldAccessor()
    {
    }
  }
}
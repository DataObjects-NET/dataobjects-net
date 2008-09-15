// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Aleksey Gamzov
// Created:    2008.09.10

using System;
using System.Reflection;
using Xtensive.Core.Internals.DocTemplates;
using Xtensive.Storage.Internals;
using Xtensive.Storage.Model;
using Xtensive.Storage.Resources;
using FieldInfo=Xtensive.Storage.Model.FieldInfo;

namespace Xtensive.Storage
{
  public abstract class EntitySet : SessionBound,
    IFieldHandler
  {
    internal static IFieldHandler Activate(Type type, Persistent obj, FieldInfo field)
    {
      AssociationInfo association = field.Association;
      if (association==null) 
        throw new InvalidOperationException(String.Format(Strings.ExUnableToActivateEntitySetWithoutAssociation, field.Name));
      if (association.MasterAssociation.EntityType==null) {
        Type simpleEntitySetType = typeof (EntitySet<>).MakeGenericType(type);
        return (IFieldHandler)simpleEntitySetType.InvokeMember("", BindingFlags.CreateInstance, null, null, new object[] {obj, field});
      }
      if (association.IsMaster) {
        // direct
        Type directEntitySetType = typeof (EntitySet<,>).MakeGenericType(type, association.EntityType);
        return (IFieldHandler)directEntitySetType.InvokeMember("", BindingFlags.CreateInstance, null, null, new object[] { obj, field, false });
      }
      // reverse
      Type reverseEntitySetType = typeof(EntitySet<,>).MakeGenericType(type, association.MasterAssociation.EntityType);
      return (IFieldHandler)reverseEntitySetType.InvokeMember("", BindingFlags.CreateInstance, null, null, new object[] { obj, field, true });
    }

    /// <inheritdoc/>
    public Persistent Owner { get; private set; }

    /// <inheritdoc/>
    public FieldInfo Field { get; private set; }

    internal abstract void ClearCache();

    internal abstract void AddToCache(Key key, bool mandatoryProcess);

    internal abstract void RemoveFromCache(Key key, bool refresh);

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="owner">Persistent this entity set belongs to.</param>
    /// <param name="field">Field corresponds to this entity set.</param>
    protected EntitySet(Persistent owner, FieldInfo field)
    {
      Field = field;
      Owner = owner;
    }
  }
}
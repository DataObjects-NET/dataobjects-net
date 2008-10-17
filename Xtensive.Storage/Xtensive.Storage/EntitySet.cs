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
using Xtensive.Storage.Rse;
using FieldInfo=Xtensive.Storage.Model.FieldInfo;

namespace Xtensive.Storage
{
  public abstract class EntitySet : SessionBound,
    IFieldHandler,
    IHasTransactionalState<EntitySetState>
  {
    /// <inheritdoc/>
    public Persistent Owner { get; private set; }

    /// <inheritdoc/>
    public FieldInfo Field { get; private set; }

    internal Entity OwnerEntity 
    {
      get { return (Entity) Owner; }
    }
    protected IndexInfo Index { get; private set; }

    protected RecordSet RecordSet { get; private set; }

    /// <inheritdoc/>
    EntitySetState IHasTransactionalState<EntitySetState>.State
    {
      get { return State;}
    }

    protected EntitySetState State { get; private set; }

    internal abstract bool Add(Entity item);

    internal abstract bool Remove(Entity item);

    protected internal virtual void Initialize()
    {
      Index = GetIndex();
      RecordSet = GetRecordSet();
      State = new EntitySetState(RecordSet);
    }

    protected abstract IndexInfo GetIndex();

    protected abstract RecordSet GetRecordSet();

    #region Activation members

    internal static IFieldHandler Activate(Type type, Persistent owner, FieldInfo field)
    {
      if (field.Association==null) 
        throw new InvalidOperationException(String.Format(Strings.ExUnableToActivateEntitySetWithoutAssociation, field.Name));

      Type instanceType;
      if (field.Association.Master.UnderlyingType==null)
        instanceType = typeof (EntitySet<>).MakeGenericType(type);
      else {
        if (field.Association.IsMaster)
          instanceType = typeof (EntitySet<,>).MakeGenericType(type, field.Association.UnderlyingType);
        else
          instanceType = typeof (ReversedEntitySet<,>).MakeGenericType(type, field.Association.Master.UnderlyingType);
      }
      return ActivateInstance(instanceType, owner, field);
    }

    private static IFieldHandler ActivateInstance(Type type, Persistent owner, FieldInfo field)
    {
      return (IFieldHandler)type.InvokeMember(string.Empty, BindingFlags.CreateInstance, null, null, new object[] { owner, field});
    }

    #endregion


    // Constructors

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
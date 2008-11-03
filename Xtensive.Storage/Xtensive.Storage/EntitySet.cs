// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Aleksey Gamzov
// Created:    2008.09.10

using System;
using System.Linq;
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
    IFieldHandler
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

    protected internal RecordSet RecordSet { get; private set; }

    protected EntitySetState State { get; private set; }

    internal abstract bool Add(Entity item);

    internal abstract bool Remove(Entity item);

    protected internal virtual void Initialize()
    {
      Index = GetIndex();
      RecordSet = GetRecordSet();
      State = new EntitySetState(() => (int) RecordSet.Count(), Session.Transaction);
    }

    protected abstract IndexInfo GetIndex();

    protected abstract RecordSet GetRecordSet();


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
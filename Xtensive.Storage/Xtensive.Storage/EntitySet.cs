// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Aleksey Gamzov
// Created:    2008.09.10

using System;
using Xtensive.Core.Internals.DocTemplates;
using Xtensive.Storage.Model;

namespace Xtensive.Storage
{
  public abstract class EntitySet : SessionBound,
    IFieldHandler
  {
    internal static IFieldHandler Activate(Type type, Persistent obj, FieldInfo field)
    {
      throw new NotImplementedException();
    }

    /// <inheritdoc/>
    public Persistent Owner { get; private set; }

    /// <inheritdoc/>
    public FieldInfo Field { get; private set; }

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
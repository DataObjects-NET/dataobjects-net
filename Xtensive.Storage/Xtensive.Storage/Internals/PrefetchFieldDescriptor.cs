// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexander Nikolaev
// Created:    2009.09.23

using System;
using Xtensive.Core;
using Xtensive.Core.Internals.DocTemplates;
using Xtensive.Storage.Model;

namespace Xtensive.Storage.Internals
{
  /// <summary>
  /// Descriptor of a field's fetching request.
  /// </summary>
  [Serializable]
  public struct PrefetchFieldDescriptor
  {
    /// <summary>
    /// The field which value will be fetched.
    /// </summary>
    public readonly FieldInfo Field;

    /// <summary>
    /// If it is set to <see langword="true" /> then fields' values of 
    /// an <see cref="Entity"/> referenced by <see cref="Field"/> will be fetched.
    /// </summary>
    public readonly bool FetchFieldsOfReferencedEntity;

    /// <summary>
    /// The maximal count of items which will be loaded during prefetch of an <see cref="EntitySet{TItem}"/>.
    /// </summary>
    public readonly int? EntitySetItemCountLimit;

    /// <inheritdoc/>
    public bool Equals(PrefetchFieldDescriptor other)
    {
      return Equals(other.Field, Field);
    }

    /// <inheritdoc/>
    public override bool Equals(object obj)
    {
      if (ReferenceEquals(null, obj))
        return false;
      if (obj.GetType()!=typeof (PrefetchFieldDescriptor))
        return false;
      return Equals((PrefetchFieldDescriptor) obj);
    }

    /// <inheritdoc/>
    public override int GetHashCode()
    {
      return Field.GetHashCode();
    }


    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="field">The field which value will be fetched.</param>
    /// <param name="entitySetItemCountLimit">The maximal count of items 
    /// which will be loaded during prefetch of an <see cref="EntitySet{TItem}"/>.</param>
    /// <param name="fetchFieldsOfReferencedEntity">If it is set to <see langword="true" /> 
    /// then fields' values of an <see cref="Entity"/> referenced by <see cref="Field"/> 
    /// will be fetched.</param>
    public PrefetchFieldDescriptor(FieldInfo field, int? entitySetItemCountLimit,
      bool fetchFieldsOfReferencedEntity)
    {
      ArgumentValidator.EnsureArgumentNotNull(field, "field");
      if (entitySetItemCountLimit != null)
        ArgumentValidator.EnsureArgumentIsGreaterThan(entitySetItemCountLimit.Value, 0,
          "entitySetItemCountLimit");
      Field = field;
      FetchFieldsOfReferencedEntity = fetchFieldsOfReferencedEntity;
      EntitySetItemCountLimit = entitySetItemCountLimit;
    }

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="field">The field which value will be fetched.</param>
    /// <param name="entitySetItemCountLimit">The maximal count of items 
    /// which will be loaded during prefetch of an <see cref="EntitySet{TItem}"/>.</param>
    public PrefetchFieldDescriptor(FieldInfo field, int? entitySetItemCountLimit) :
      this(field, entitySetItemCountLimit, false)
    {}

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="field">The field which value will be fetched.</param>
    /// <param name="fetchFieldsOfReferencedEntity">If it is set to <see langword="true" /> 
    /// then fields' values of an <see cref="Entity"/> referenced by <see cref="Field"/> 
    /// will be fetched.</param>
    public PrefetchFieldDescriptor(FieldInfo field, bool fetchFieldsOfReferencedEntity) :
      this(field, null, fetchFieldsOfReferencedEntity)
    {}

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="field">The field which value will be fetched.</param>
    /// which will be loaded during prefetch of an <see cref="EntitySet{TItem}"/>.</param>
    public PrefetchFieldDescriptor(FieldInfo field)
      : this(field, null, false)
    {}
  }
}
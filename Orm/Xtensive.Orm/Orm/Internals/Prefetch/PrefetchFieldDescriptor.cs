// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexander Nikolaev
// Created:    2009.09.23

using System;
using Xtensive.Core;

using Xtensive.Orm.Model;

namespace Xtensive.Orm.Internals.Prefetch
{
  /// <summary>
  /// Descriptor of a field's fetching request.
  /// </summary>
  [Serializable]
  public sealed class PrefetchFieldDescriptor
  {
    private readonly Action<Key, FieldInfo, Key> keyExtractionSubscriber;

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

    /// <summary>
    /// Indicates whether children lazy-load fields will be fetched, or not.
    /// </summary>
    public readonly bool FetchLazyFields;

    /// <summary>
    /// Unique identifier of prefetch operation which created this instance. Used for <see cref="EntitySet{TItem}"/> items prefetching.
    /// </summary>
    public readonly Guid? PrefetchOperationId;

    /// <inheritdoc/>
    public bool Equals(PrefetchFieldDescriptor other)
    {
      return Equals(other.Field, Field);
    }

    /// <inheritdoc/>
    public override bool Equals(object obj) =>
      obj is PrefetchFieldDescriptor other && Equals(other);

    /// <inheritdoc/>
    public override int GetHashCode()
    {
      return Field.GetHashCode();
    }

    internal void NotifySubscriber(Key ownerKey, Key referencedKey)
    {
      if (keyExtractionSubscriber != null) {
        keyExtractionSubscriber.Invoke(ownerKey, Field, referencedKey);
      }
    }


    // Constructors

    /// <summary>
    /// Initializes a new instance of this class.
    /// </summary>
    /// <param name="field">The field whose value will be fetched.</param>
    public PrefetchFieldDescriptor(FieldInfo field)
      : this(field, null, true, true, null, null)
    { }

    /// <summary>
    /// Initializes a new instance of this class.
    /// </summary>
    /// <param name="field">The field whose value will be fetched.</param>
    /// <param name="entitySetItemCountLimit">The maximal count of items 
    /// which will be loaded during prefetch of an <see cref="EntitySet{TItem}"/>.</param>
    public PrefetchFieldDescriptor(FieldInfo field, int? entitySetItemCountLimit)
      : this(field, entitySetItemCountLimit, false, false, null, null)
    { }

    /// <summary>
    /// Initializes a new instance of this class.
    /// </summary>
    /// <param name="field">The field whose value will be fetched.</param>
    /// <param name="fetchFieldsOfReferencedEntity">If it is set to <see langword="true" /> 
    /// then fields' values of an <see cref="Entity"/> referenced by <see cref="Field"/> 
    /// will be fetched.</param>
    /// <param name="fetchLazyFields">if set to <see langword="true"/> 
    /// children lazy-load fields will be fetched.</param>
    public PrefetchFieldDescriptor(FieldInfo field, bool fetchFieldsOfReferencedEntity, bool fetchLazyFields)
      : this(field, null, fetchFieldsOfReferencedEntity, fetchLazyFields, null, null)
    { }

    /// <summary>
    /// Initializes a new instance of this class.
    /// </summary>
    /// <param name="field">The field whose value will be fetched.</param>
    /// <param name="fetchFieldsOfReferencedEntity">If it is set to <see langword="true" /> 
    /// then fields' values of an <see cref="Entity"/> referenced by <see cref="Field"/> 
    /// will be fetched.</param>
    /// <param name="fetchLazyFields">if set to <see langword="true"/> 
    /// children lazy-load fields will be fetched.</param>
    /// <param name="prefetchOperationId">The prefetch operation identifier which creates the descriptor</param>
    public PrefetchFieldDescriptor(FieldInfo field, bool fetchFieldsOfReferencedEntity, bool fetchLazyFields, Guid prefetchOperationId)
      : this(field, null, fetchFieldsOfReferencedEntity, fetchLazyFields, null, prefetchOperationId)
    { }

    /// <summary>
    /// Initializes a new instance of this class.
    /// </summary>
    /// <param name="field">The field whose value will be fetched.</param>
    /// <param name="entitySetItemCountLimit">The maximal count of items
    /// which will be loaded during prefetch of an <see cref="EntitySet{TItem}"/>.</param>
    /// <param name="fetchFieldsOfReferencedEntity">If it is set to <see langword="true"/>
    /// then fields' values of an <see cref="Entity"/> referenced by <see cref="Field"/>
    /// will be fetched.</param>
    /// <param name="fetchLazyFields">if set to <see langword="true"/> 
    /// children lazy-load fields will be fetched.</param>
    /// <param name="keyExtractionSubscriber">The delegate which will be invoked
    /// if a key of a referenced entity has been extracted and
    /// its exact type can't be get or inferred.</param>
    public PrefetchFieldDescriptor(
      FieldInfo field,
      int? entitySetItemCountLimit,
      bool fetchFieldsOfReferencedEntity,
      bool fetchLazyFields,
      Action<Key, FieldInfo, Key> keyExtractionSubscriber)
    {
      ArgumentValidator.EnsureArgumentNotNull(field, "field");
      if (entitySetItemCountLimit != null) {
        ArgumentValidator.EnsureArgumentIsGreaterThan(entitySetItemCountLimit.Value, 0,
          "entitySetItemCountLimit");
      }
      Field = field;
      FetchFieldsOfReferencedEntity = fetchFieldsOfReferencedEntity;
      EntitySetItemCountLimit = entitySetItemCountLimit;
      FetchLazyFields = fetchLazyFields;
      this.keyExtractionSubscriber = keyExtractionSubscriber;
    }

    /// <summary>
    /// Initializes a new instance of this class.
    /// </summary>
    /// <param name="field">The field whose value will be fetched.</param>
    /// <param name="entitySetItemCountLimit">The maximal count of items
    /// which will be loaded during prefetch of an <see cref="EntitySet{TItem}"/>.</param>
    /// <param name="fetchFieldsOfReferencedEntity">If it is set to <see langword="true"/>
    /// then fields' values of an <see cref="Entity"/> referenced by <see cref="Field"/>
    /// will be fetched.</param>
    /// <param name="fetchLazyFields">if set to <see langword="true"/> 
    /// children lazy-load fields will be fetched.</param>
    /// <param name="keyExtractionSubscriber">The delegate which will be invoked
    /// if a key of a referenced entity has been extracted and
    /// its exact type can't be get or inferred.</param>
    /// <param name="prefetchOperationId">Identifies the perfetch operation which creates the descriptor.</param>
    private PrefetchFieldDescriptor(
      FieldInfo field,
      int? entitySetItemCountLimit,
      bool fetchFieldsOfReferencedEntity,
      bool fetchLazyFields,
      Action<Key, FieldInfo, Key> keyExtractionSubscriber,
      Guid? prefetchOperationId)
    {
      ArgumentValidator.EnsureArgumentNotNull(field, "field");
      if (entitySetItemCountLimit != null) {
        ArgumentValidator.EnsureArgumentIsGreaterThan(entitySetItemCountLimit.Value, 0,
          "entitySetItemCountLimit");
      }
      Field = field;
      FetchFieldsOfReferencedEntity = fetchFieldsOfReferencedEntity;
      EntitySetItemCountLimit = entitySetItemCountLimit;
      FetchLazyFields = fetchLazyFields;
      this.keyExtractionSubscriber = keyExtractionSubscriber;
      PrefetchOperationId = prefetchOperationId;
    }
  }
}
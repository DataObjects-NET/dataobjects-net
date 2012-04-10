// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.11.02

using System;
using Xtensive.Aspects;
using Xtensive.Core;
using Xtensive.IoC;
using Xtensive.Tuples;
using Tuple = Xtensive.Tuples.Tuple;
using Xtensive.Orm.Model;

using Activator=Xtensive.Orm.Internals.Activator;

namespace Xtensive.Orm.Services
{
  /// <summary>
  /// Provides access to low-level operations with <see cref="Persistent"/> descendants.
  /// </summary>
  [Service(typeof(DirectPersistentAccessor))]
  [Infrastructure]
  public class DirectPersistentAccessor : SessionBound,
    ISessionService
  {
    #region Entity/Structure-related methods

    /// <summary>
    /// Creates new entity instance of the specified type.
    /// </summary>
    /// <param name="entityType">The type of entity to create. Must be descendant of the <see cref="Entity"/> type.</param>
    /// <returns>Newly created entity.</returns>
    public Entity CreateEntity(Type entityType)
    {
      using (Session.OpenSystemLogicOnlyRegion()) {
        ArgumentValidator.EnsureArgumentNotNull(entityType, "entityType");
        if (!typeof (Entity).IsAssignableFrom(entityType))
          throw new InvalidOperationException(
            string.Format(Strings.TypeXIsNotAnYDescendant, entityType, typeof (Entity)));

        var key = Key.Create(Session, entityType);
        return Session.CreateOrInitializeExistingEntity(entityType, key);
      }
    }

    /// <summary>
    /// Creates new entity instance of the specified type with the specified value.
    /// </summary>
    /// <param name="entityType">The type of structure to create. Must be descendant of the <see cref="Entity"/> type.</param>
    /// <param name="tuple">The tuple with entity data.</param>
    /// <returns>Created entity.</returns>
    public Entity CreateEntity(Type entityType, Tuple tuple)
    {
      using (Session.OpenSystemLogicOnlyRegion()) {
        ArgumentValidator.EnsureArgumentNotNull(entityType, "entityType");
        ArgumentValidator.EnsureArgumentNotNull(tuple, "tuple");
        if (!typeof (Entity).IsAssignableFrom(entityType))
          throw new InvalidOperationException(
            string.Format(Strings.TypeXIsNotAnYDescendant, entityType, typeof (Entity)));

        var domain = Session.Domain;
        var key = Key.Create(domain, domain.Model.Types[entityType], TypeReferenceAccuracy.ExactType, tuple);
        return Session.CreateOrInitializeExistingEntity(entityType, key);
      }
    }

    /// <summary>
    /// Creates new entity instance with the specified key. Key should have exact type.
    /// </summary>
    /// <param name="key">The key.</param>
    /// <returns>Created entity.</returns>
    public Entity CreateEntity(Key key)
    {
      using (Session.OpenSystemLogicOnlyRegion())
      {
        ArgumentValidator.EnsureArgumentNotNull(key, "key");
        if (key.TypeReference.Accuracy != TypeReferenceAccuracy.ExactType)
          throw new InvalidOperationException(string.Format(Strings.ExKeyXShouldHaveExactType, key));
        var entityType = key.TypeReference.Type;
        var domain = Session.Domain;
        return Session.CreateOrInitializeExistingEntity(entityType.UnderlyingType, key);
      }
    }

    /// <summary>
    /// Creates new <see cref="Structure"/> of the specified type.
    /// </summary>
    /// <param name="structureType">The type of structure to create. Must be descendant of the <see cref="Structure"/> type.</param>
    /// <returns>Created structure.</returns>
    public Structure CreateStructure(Type structureType)
    {
      var tuple = Session.Domain.Model.Types[structureType].TuplePrototype.CreateNew();
      return CreateStructure(structureType, tuple);
    }

    /// <summary>
    /// Creates new <see cref="Structure"/> of the specified type filled with provided data.
    /// </summary>
    /// <param name="structureType">The type of structure to create. Must be descendant of the <see cref="Structure"/> type.</param>
    /// <param name="structureData">The structure data tuple.</param>
    /// <returns>Created structure.</returns>
    public Structure CreateStructure(Type structureType, Tuple structureData)
    {
      using (Session.OpenSystemLogicOnlyRegion()) {
        ArgumentValidator.EnsureArgumentNotNull(structureType, "structureType");
        if (!typeof(Structure).IsAssignableFrom(structureType))
          throw new InvalidOperationException(string.Format(Strings.TypeXIsNotAnYDescendant, structureType, typeof(Structure)));

        return Activator.CreateStructure(Session, structureType, structureData);
      }
    }

    /// <summary>
    /// Gets the value of the specified persistent field of the target.
    /// </summary>
    /// <param name="target">The target entity or structure.</param>
    /// <param name="field">The field.</param>
    /// <returns>Field value.</returns>
    public object GetFieldValue(Persistent target, FieldInfo field)
    {
      using (Session.OpenSystemLogicOnlyRegion()) {
        ValidateArguments(target, field);
        return target.GetFieldValue(field);
      }
    }

    /// <summary>
    /// Gets the value of the specified persistent field of the target.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="target">The target entity or structure.</param>
    /// <param name="field">The field.</param>
    /// <returns>Field value.</returns>
    public T GetFieldValue<T>(Persistent target, FieldInfo field)
    {
      using (Session.OpenSystemLogicOnlyRegion()) {
        ValidateArguments(target, field);
        return target.GetFieldValue<T>(field);
      }
    }

    /// <summary>
    /// Indicates whether specified field values are equal.
    /// </summary>
    /// <param name="target">The target entity or structure.</param>
    /// <param name="field">The field.</param>
    /// <param name="value1">The first value.</param>
    /// <param name="value2">The second value.</param>
    /// <returns>Comparison result.</returns>
    public bool AreSameValues(Persistent target, FieldInfo field, object value1, object value2)
    {
      ValidateArguments(target, field);
      return target.GetFieldAccessor(field).AreSameValues(value1, value2);
    }

    /// <summary>
    /// Gets the key of the entity, that is referenced by specified field 
    /// of the target persistent object.
    /// </summary>
    /// <remarks>
    /// Result is the same as <c>target.GetValue&lt;Entity&gt;(field).Key</c>, 
    /// but referenced entity will not be materialized.
    /// </remarks>
    /// <param name="target">The target persistent object.</param>
    /// <param name="field">The reference field. Field value type must be 
    /// <see cref="Entity"/> descendant.</param>
    /// <returns>Referenced entity key.</returns>
    /// <exception cref="InvalidOperationException">Field is not a reference field.</exception>
    public Key GetReferenceKey(Persistent target, FieldInfo field)
    {
      using (Session.OpenSystemLogicOnlyRegion()) {
        ValidateArguments(target, field);
        return target.GetReferenceKey(field);
      }
    }

    /// <summary>
    /// Sets the value of the specified persistent field of the target.
    /// </summary>
    /// <param name="target">The target persistent object.</param>
    /// <param name="field">The field to set value for.</param>
    /// <param name="value">The value to set.</param>
    public void SetFieldValue(Persistent target, FieldInfo field, object value)
    {
      using (Session.OpenSystemLogicOnlyRegion()) {
        ValidateArguments(target, field);
        target.SetFieldValue(field, value);
      }
    }

    /// <summary>
    /// Sets the value of the specified persistent field of the target.
    /// </summary>
    /// <typeparam name="T">Value type</typeparam>
    /// <param name="target">The target persistent object.</param>
    /// <param name="field">The field to set value for.</param>
    /// <param name="value">The value to set.</param>
    public void SetFieldValue<T>(Persistent target, FieldInfo field, T value)
    {
      using (Session.OpenSystemLogicOnlyRegion()) {
        ValidateArguments(target, field);
        target.SetFieldValue(field, value);
      }
    }

    /// <summary>
    /// Removes the specified entity.
    /// </summary>
    /// <param name="target">The entity to remove.</param>
    public void Remove(Entity target)
    {
      using (Session.OpenSystemLogicOnlyRegion()) {
        ArgumentValidator.EnsureArgumentNotNull(target, "target");
        target.Remove();
      }
    }

    #endregion

    #region Protected members

    /// <summary>
    /// Validates the arguments passed to some of methods.
    /// </summary>
    /// <param name="target">The persistent type.</param>
    /// <param name="field">The field of persistent type.</param>
    protected static void ValidateArguments(Persistent target, FieldInfo field)
    {
      ArgumentValidator.EnsureArgumentNotNull(target, "target");
      ArgumentValidator.EnsureArgumentNotNull(field, "field");
      if (!target.TypeInfo.Fields.Contains(field))
        throw new InvalidOperationException(string.Format(
          Strings.ExTypeXDoesNotContainYField, target.TypeInfo.Name, field.Name));
    }

    #endregion


    // Constructors

    
   [ServiceConstructor]
   public DirectPersistentAccessor(Session session)
      : base(session)
    {
    }
  }
}
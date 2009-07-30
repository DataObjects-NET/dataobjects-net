// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.11.06

using System;
using Xtensive.Core;
using Xtensive.Core.Aspects;
using Xtensive.Core.Internals.DocTemplates;
using Xtensive.Storage.Model;
using Xtensive.Storage.Resources;

namespace Xtensive.Storage
{
  /// <summary>
  /// Provides access to low-level operations with <see cref="EntitySetBase"/> descendants.
  /// </summary>
  public sealed class EntitySetAccessor : SessionBound
  {
    /// <summary>
    /// Gets the entity set for the specified property.
    /// </summary>
    /// <param name="target">The target.</param>
    /// <param name="field">The field.</param>
    /// <returns></returns>
    [Infrastructure]
    public EntitySetBase GetEntitySet(Entity target, FieldInfo field)
    {
      using (CoreServices.OpenSystemLogicOnlyRegion()) {
        ValidateArguments(target, field);
        return target.GetFieldValue<EntitySetBase>(field);
      }
    }

//    [Infrastructure]
//    public RecordSet GetRecordSet(EntitySetBase target)
//    {
//      ValidateArguments(target);
//      return target.items;
//    }
//
//    [Infrastructure]
//    public RecordSet GetRecordSet(Entity target, FieldInfo field)
//    {
//      ValidateArguments(target, field);
//      return GetRecordSet(GetEntitySet(target, field));
//    }

    /// <summary>
    /// Adds the item to the specified target.
    /// </summary>
    /// <param name="target">The target.</param>
    /// <param name="item">The item.</param>
    /// <returns>
    /// <see langword="true"/>, if the item was added;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    [Infrastructure]
    public bool Add(EntitySetBase target, Entity item)
    {
      using (CoreServices.OpenSystemLogicOnlyRegion()) {
        ValidateArguments(target, item);
        return target.Add(item);
      }
    }

    /// <summary>
    /// Adds the item to the specified target.
    /// </summary>
    /// <param name="target">The target.</param>
    /// <param name="field">The field.</param>
    /// <param name="item">The item.</param>
    /// <returns>
    /// <see langword="true"/>, if the item was added;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    [Infrastructure]
    public bool Add(Entity target, FieldInfo field, Entity item)
    {
      ValidateArguments(target, field, item);
      return Add(GetEntitySet(target, field), item);
    }

    /// <summary>
    /// Removes the item from the specified target.
    /// </summary>
    /// <param name="target">The target.</param>
    /// <param name="item">The item.</param>
    /// <returns>
    /// <see langword="true"/>, if the item was added;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    [Infrastructure]
    public bool Remove(EntitySetBase target, Entity item)
    {
      using (CoreServices.OpenSystemLogicOnlyRegion()) {
        ValidateArguments(target, item);
        return target.Remove(item);
      }
    }

    /// <summary>
    /// Removes the item from the specified target.
    /// </summary>
    /// <param name="target">The target.</param>
    /// <param name="field">The field.</param>
    /// <param name="item">The item.</param>
    /// <returns>
    /// <see langword="true"/>, if the item was added;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    [Infrastructure]
    public bool Remove(Entity target, FieldInfo field, Entity item)
    {
      ValidateArguments(target, field, item);
      return Remove(GetEntitySet(target, field), item);
    }

    /// <summary>
    /// Clears the specified target.
    /// </summary>
    /// <param name="target">The target.</param>
    [Infrastructure]
    public void Clear(EntitySetBase target)
    {
      using (CoreServices.OpenSystemLogicOnlyRegion()) {
        ValidateArguments(target);
        target.Clear();
      }
    }

    /// <summary>
    /// Clears the entity set of the specified target field.
    /// </summary>
    /// <param name="target">The target.</param>
    /// <param name="field">The field.</param>
    [Infrastructure]
    public void Clear(Entity target, FieldInfo field)
    {
      ValidateArguments(target, field);
      Clear(GetEntitySet(target, field));
    }

    #region Private \ internal methods

    private static void ValidateArguments(object target)
    {
      ArgumentValidator.EnsureArgumentNotNull(target, "target");
    }

    private static void ValidateArguments(Persistent target, FieldInfo field)
    {
      ValidateArguments(target);
      ArgumentValidator.EnsureArgumentNotNull(field, "field");
      if (!target.Type.Fields.Contains(field))
        throw new InvalidOperationException(string.Format(
          Strings.ExTypeXDoesNotContainYField, target.Type.Name, field.Name));
      if (!field.IsEntitySet)
        throw new InvalidOperationException(string.Format(
          Strings.ExFieldXIsNotAnEntitySetField, field.Name));
    }

    private static void ValidateArguments(EntitySetBase target, Entity item)
    {
      ValidateArguments(target);
      ArgumentValidator.EnsureArgumentNotNull(item, "item");
    }

    private static void ValidateArguments(Persistent target, FieldInfo field, Entity item)
    {
      ValidateArguments(target, field);
      ArgumentValidator.EnsureArgumentNotNull(item, "item");
    }

    #endregion

    
    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="session">The session.</param>
    public EntitySetAccessor(Session session)
      : base(session)
    {
    }
  }
}
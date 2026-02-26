// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.11.06

using System;
using Xtensive.Core;

using Xtensive.IoC;
using Xtensive.Orm.Model;


namespace Xtensive.Orm.Services
{
  /// <summary>
  /// Provides access to low-level operations with <see cref="EntitySetBase"/> descendants.
  /// </summary>
  [Service(typeof(DirectEntitySetAccessor))]
  public sealed class DirectEntitySetAccessor : SessionBound,
    ISessionService
  {
    /// <summary>
    /// Gets the entity set for the specified property.
    /// </summary>
    /// <param name="target">The target.</param>
    /// <param name="field">The field.</param>
    /// <returns></returns>
    public EntitySetBase GetEntitySet(Entity target, FieldInfo field)
    {
      using (Session.OpenSystemLogicOnlyRegion()) {
        ValidateArguments(target, field);
        return (EntitySetBase) target.GetFieldValue(field);
      }
    }

//    public RecordSet GetRecordSet(EntitySetBase target)
//    {
//      ValidateArguments(target);
//      return target.items;
//    }
//
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
    public bool Add(EntitySetBase target, Entity item)
    {
      using (Session.OpenSystemLogicOnlyRegion()) {
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
    public bool Remove(EntitySetBase target, Entity item)
    {
      using (Session.OpenSystemLogicOnlyRegion()) {
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
    public bool Remove(Entity target, FieldInfo field, Entity item)
    {
      ValidateArguments(target, field, item);
      return Remove(GetEntitySet(target, field), item);
    }

    /// <summary>
    /// Clears the specified target.
    /// </summary>
    /// <param name="target">The target.</param>
    public void Clear(EntitySetBase target)
    {
      using (Session.OpenSystemLogicOnlyRegion()) {
        ValidateArguments(target);
        target.Clear();
      }
    }

    /// <summary>
    /// Clears the entity set of the specified target field.
    /// </summary>
    /// <param name="target">The target.</param>
    /// <param name="field">The field.</param>
    public void Clear(Entity target, FieldInfo field)
    {
      ValidateArguments(target, field);
      Clear(GetEntitySet(target, field));
    }

    #region Private \ internal methods

    private static void ValidateArguments(object target)
    {
      ArgumentNullException.ThrowIfNull(target, "target");
    }

    private static void ValidateArguments(Persistent target, FieldInfo field)
    {
      ValidateArguments(target);
      ArgumentNullException.ThrowIfNull(field, "field");
      if (!target.TypeInfo.Fields.Contains(field))
        throw new InvalidOperationException(string.Format(
          Strings.ExTypeXDoesNotContainYField, target.TypeInfo.Name, field.Name));
      if (!field.IsEntitySet)
        throw new InvalidOperationException(string.Format(
          Strings.ExFieldXIsNotAnEntitySetField, field.Name));
    }

    private static void ValidateArguments(EntitySetBase target, Entity item)
    {
      ValidateArguments(target);
      ArgumentNullException.ThrowIfNull(item, "item");
    }

    private static void ValidateArguments(Persistent target, FieldInfo field, Entity item)
    {
      ValidateArguments(target, field);
      ArgumentNullException.ThrowIfNull(item, "item");
    }

    #endregion

    
    // Constructors

    /// <inheritdoc/>
    [ServiceConstructor]
    public DirectEntitySetAccessor(Session session)
      : base(session)
    {
    }
  }
}
// Copyright (C) 2014 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kulakov
// Created:    2014.04.07

using System.Collections.Generic;
using Xtensive.Core;
using Xtensive.Orm.Model;

namespace Xtensive.Orm.Internals
{
  /// <summary>
  /// Registrates information about changed reference fields.
  /// </summary>
  internal sealed class ReferenceFieldsChangesRegistry : SessionBound
  {
    private readonly HashSet<ReferenceFieldChangeInfo> changes = new HashSet<ReferenceFieldChangeInfo>();

    /// <summary>
    /// Registrates information about field which value was set.
    /// </summary>
    /// <param name="fieldOwner">Key of entity which field was set.</param>
    /// <param name="fieldValue">Value of field.</param>
    /// <param name="field">Field which value was set./</param>
    public void Register(Key fieldOwner, Key fieldValue, FieldInfo field)
    {
      ArgumentValidator.EnsureArgumentNotNull(fieldOwner, "fieldOwner");
      ArgumentValidator.EnsureArgumentNotNull(fieldValue, "fieldValue");
      ArgumentValidator.EnsureArgumentNotNull(field, "field");
      Register(new ReferenceFieldChangeInfo(fieldOwner, fieldValue, field));
    }

    /// <summary>
    /// Registrates information about field which value was set.
    /// </summary>
    /// <param name="fieldOwner">Key of entity which field was set.</param>
    /// <param name="fieldValue">Value of field.</param>
    /// <param name="auxiliaryEntity">Key of auxiliary entity which associated with <see cref="EntitySet{T}"/> field.</param>
    /// <param name="field">Field which value was set.</param>
    public void Register(Key fieldOwner, Key fieldValue, Key auxiliaryEntity, FieldInfo field)
    {
      ArgumentValidator.EnsureArgumentNotNull(fieldOwner, "fieldOwner");
      ArgumentValidator.EnsureArgumentNotNull(fieldValue, "fieldValue");
      ArgumentValidator.EnsureArgumentNotNull(auxiliaryEntity, "auxiliaryEntity");
      ArgumentValidator.EnsureArgumentNotNull(field, "field");
      Register(new ReferenceFieldChangeInfo(fieldOwner, fieldValue, auxiliaryEntity, field));
    }

    /// <summary>
    /// Gets all registered items.
    /// </summary>
    /// <returns>All registered items.</returns>
    public IEnumerable<ReferenceFieldChangeInfo> GetItems()
    {
      return changes;
    }

    /// <summary>
    /// Removes all registered items.
    /// </summary>
    public void Clear()
    {
      changes.Clear();
    }
    
    private void Register(ReferenceFieldChangeInfo fieldChangeInfo)
    {
      changes.Add(fieldChangeInfo);
    }

    public ReferenceFieldsChangesRegistry(Session session)
      : base(session)
    {
    }
  }
}

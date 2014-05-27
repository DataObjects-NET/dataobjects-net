// Copyright (C) 2014 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kulakov
// Created:    2014.04.07

using Xtensive.Orm.Model;

namespace Xtensive.Orm.Internals
{
  /// <summary>
  /// Contains information about change of reference field.
  /// </summary>
  internal sealed class ReferenceFieldChangeInfo
  {
    /// <summary>
    /// Gets key of entity that owns the <see cref="ReferenceFieldChangeInfo.Field"/>.
    /// </summary>
    public Key FieldOwner { get; private set; }

    /// <summary>
    /// Gets value of field which was set.
    /// </summary>
    public Key FieldValue { get; private set; }

    /// <summary>
    /// Gets field which was set.
    /// </summary>
    public FieldInfo Field { get; private set; }

    /// <summary>
    /// Auxiliary entity which associated with <see cref="EntitySet{T}"/>.
    /// </summary>
    public Key AuxiliaryEntity { get; private set; }

    public ReferenceFieldChangeInfo(Key fieldOwner, Key fieldValue, FieldInfo field)
    {
      FieldOwner = fieldOwner;
      FieldValue = fieldValue;
      Field = field;
    }

    public ReferenceFieldChangeInfo(Key fieldOwner, Key fieldValue, Key auxiliaryEntity, FieldInfo field)
    {
      FieldOwner = fieldOwner;
      FieldValue = fieldValue;
      Field = field;
      AuxiliaryEntity = auxiliaryEntity;
    }
  }
}
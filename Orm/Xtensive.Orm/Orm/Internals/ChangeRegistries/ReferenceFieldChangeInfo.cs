// Copyright (C) 2014-2022 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
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
    public Key FieldOwner { get; }

    /// <summary>
    /// Gets value of field which was set.
    /// </summary>
    public Key FieldValue { get; }

    /// <summary>
    /// Gets field which was set.
    /// </summary>
    public FieldInfo Field { get; }

    /// <summary>
    /// Auxiliary entity which associated with <see cref="EntitySet{T}"/>.
    /// </summary>
    public Key AuxiliaryEntity { get; }

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